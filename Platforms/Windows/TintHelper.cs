using Aeonpulse.Attributes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using WinUIButton = Microsoft.UI.Xaml.Controls.Button;
using WinUIImage = Microsoft.UI.Xaml.Controls.Image;
using WinUIColor = Windows.UI.Color;

namespace Aeonpulse
{
    /// <summary>
    /// Windows (WinUI 3) implementation of the <see cref="MauiProgram"/> tint partials.
    ///
    /// <para>
    /// <b>Tinting strategy:</b> Uses Win2D (<c>Microsoft.Graphics.Canvas</c>) to apply
    /// a <see cref="ColorMatrixEffect"/> that replaces every pixel's RGB with the
    /// requested tint colour while preserving the source alpha mask.  This exactly
    /// matches the behaviour of Android's <c>PorterDuff.SrcIn</c> and iOS's
    /// <c>UIImageRenderingMode.AlwaysTemplate</c>.
    /// </para>
    /// <para>
    /// <b>File resolution:</b> the source filename is obtained from the MAUI handler's
    /// <c>VirtualView.Source</c> (<see cref="FileImageSource.File"/>) and resolved to
    /// the scaled PNG in <see cref="AppContext.BaseDirectory"/> - the exe output
    /// directory where <c>WindowsPackageType=None</c> copies all
    /// <c>MauiImage</c> assets (e.g. <c>info.scale-100.png</c>).
    /// </para>
    /// <para>
    /// <b>Caching:</b> tinted <see cref="WriteableBitmap"/> instances are cached by
    /// (filename, colour) so repeated theme switches do not re-run the Win2D pipeline.
    /// </para>
    /// <para>
    /// <b>ImageButton tint lifecycle:</b> MAUI sets the inner <see cref="WinUIImage"/>
    /// source asynchronously and may reset it on source-binding changes (e.g. chevron
    /// toggle via <c>BoolToImageSourceConverter</c>). To survive these resets, once the
    /// inner <see cref="WinUIImage"/> is found we store the desired tint parameters for
    /// it and subscribe to <c>ImageOpened</c>. Every time MAUI loads a new bitmap into
    /// the image, our handler fires and immediately replaces the source with the tinted
    /// <see cref="WriteableBitmap"/>. The subscription is updated (not duplicated) on
    /// each call via a <see cref="System.Runtime.CompilerServices.ConditionalWeakTable"/>.
    /// </para>
    /// </summary>
    [AIContext("PlatformTintImplementation")]
    public static partial class MauiProgram
    {
        // Cache: (scaledFileName, colour) -> WriteableBitmap already tinted to that colour.
        private static readonly Dictionary<(string file, WinUIColor colour), WriteableBitmap> _tintCache = new();

        // Shared CanvasDevice - expensive to create, safe to reuse across calls.
        private static CanvasDevice? _canvasDevice;

        // Per-WinUIImage: the tint params (file + colour) to apply when ImageOpened fires.
        // Keyed on the inner Image so the subscription survives source changes.
        private sealed class ImageTintState
        {
            public string File  = "";
            public Color? Tint;
            public RoutedEventHandler? OpenedHandler;
        }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<WinUIImage, ImageTintState>
            _imageStates = new();

        // Per-WinUIButton: the Loaded handler box so the exact delegate can be unsubscribed.
        private sealed class LoadedHandlerBox { public RoutedEventHandler? Handler; }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<WinUIButton, LoadedHandlerBox>
            _pendingLoadedHandlers = new();

        // Per-WinUIButton: the tint params to retry on LayoutUpdated if ApplyTemplate fails.
        private sealed class PendingButtonTint
        {
            public string File = "";
            public Color? Tint;
            public EventHandler<object>? LayoutHandler;
        }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<WinUIButton, PendingButtonTint>
            _pendingButtonTints = new();

        /// <summary>
        /// Resolves the source filename from the MAUI <c>Image</c> handler's
        /// <c>VirtualView</c>, loads it from the output directory, applies a
        /// Win2D <see cref="ColorMatrixEffect"/>, and replaces <c>Image.Source</c>
        /// with the tinted <see cref="WriteableBitmap"/>.
        /// </summary>
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Color? tint)
        {
            if (handler.PlatformView is not WinUIImage nativeImage)
                return;

            var file = GetScaledFileName(handler.VirtualView?.Source);
            if (file is null) return;

            _ = nativeImage.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                async () =>
                {
                    var wb = await GetTintedBitmapAsync(file, tint);
                    if (wb is not null)
                        nativeImage.Source = wb;
                });
        }

        /// <summary>
        /// Walks the WinUI visual tree of the <c>Button</c> backing
        /// <c>ImageButton</c> to find the inner <c>Image</c> child, then
        /// applies the Win2D tint pipeline and subscribes to <c>ImageOpened</c>
        /// so the tint survives any future source resets by MAUI.
        /// Calls <c>ApplyTemplate()</c> first to ensure the template is inflated
        /// even for buttons inside <c>Visibility.Collapsed</c> parents.
        /// </summary>
        static partial void ApplyImageButtonTint(
            Microsoft.Maui.Handlers.ImageButtonHandler handler, Color? tint)
        {
            if (handler.PlatformView is not WinUIButton nativeBtn)
                return;

            var file = GetScaledFileName(handler.VirtualView?.Source);
            if (file is null) return;

            ApplyTintToButton(nativeBtn, file, tint);
        }

        // --- Private helpers -------------------------------------------------

        /// <summary>
        /// Forces WinUI <c>ControlTemplate</c> inflation via <c>ApplyTemplate()</c>,
        /// finds the inner <see cref="WinUIImage"/>, then calls
        /// <see cref="AttachAndTint"/> to subscribe to <c>ImageOpened</c> and
        /// schedule the immediate tint pass.
        ///
        /// <para>
        /// <b>Why ApplyTemplate() and not Loaded:</b> <c>Loaded</c> fires once at
        /// tree-insertion (page load) for every element, including those inside
        /// <c>Visibility.Collapsed</c> containers. By the time our mapper runs,
        /// <c>Loaded</c> has already fired, so a Loaded-deferred handler registered
        /// after that point never fires. <c>ApplyTemplate()</c> is the correct API
        /// to force template inflation on demand, and it succeeds synchronously even
        /// for collapsed elements.
        /// </para>
        /// <para>
        /// <b>LayoutUpdated fallback:</b> in rare cases (buttons in sections that are
        /// <c>Visibility.Collapsed</c> at app startup before any layout pass runs),
        /// <c>ApplyTemplate()</c> succeeds but <c>FindDescendantImage</c> returns null
        /// because WinUI has not yet inflated the template children into the visual tree.
        /// In this case we store the desired tint and subscribe to <c>LayoutUpdated</c>,
        /// which fires on every layout pass including the first time the section becomes
        /// visible. The handler retries <c>ApplyTemplate</c> + <c>FindDescendantImage</c>,
        /// calls <c>AttachAndTint</c> when the inner Image is found, then unsubscribes.
        /// </para>
        /// </summary>
        private static void ApplyTintToButton(WinUIButton button, string file, Color? tint)
        {
            // Cancel any pending Loaded subscription - kept for safety in case a stale
            // one was registered by a prior version.
            if (_pendingLoadedHandlers.TryGetValue(button, out var existingBox) &&
                existingBox.Handler is not null)
            {
                button.Loaded -= existingBox.Handler;
                existingBox.Handler = null;
            }

            // Cancel any existing LayoutUpdated fallback before registering a new one.
            if (_pendingButtonTints.TryGetValue(button, out var existing) &&
                existing.LayoutHandler is not null)
            {
                button.LayoutUpdated -= existing.LayoutHandler;
                existing.LayoutHandler = null;
            }

            // ApplyTemplate() synchronously inflates the WinUI Button ControlTemplate
            // even when the button is inside a Visibility.Collapsed parent. Without this
            // call, FindDescendantImage returns null for all buttons in collapsed sections
            // because WinUI defers template inflation until first measure - which never
            // happens for collapsed elements. The Loaded event is NOT a substitute: it
            // fires once at tree-insertion (page load), before our mapper runs, so a
            // Loaded-deferred handler registered after that point never fires.
            button.ApplyTemplate();

            var image = FindDescendantImage(button);
            if (image is not null)
            {
                AttachAndTint(image, file, tint);
                return;
            }

            // ApplyTemplate() did not yet produce a visual tree (can happen for buttons in
            // sections that are Visibility.Collapsed at startup before any layout pass).
            // Store the desired tint params and subscribe to LayoutUpdated so we retry
            // on the first layout pass when the section becomes visible.
            var pending = _pendingButtonTints.GetOrCreateValue(button);
            pending.File = file;
            pending.Tint = tint;

            EventHandler<object> layoutHandler = null!;
            layoutHandler = (s, e) =>
            {
                if (s is not WinUIButton btn) return;
                btn.ApplyTemplate();
                var img = FindDescendantImage(btn);
                if (img is null) return;  // not yet visible - wait for next layout pass

                // Inner Image found - apply tint and stop watching.
                btn.LayoutUpdated -= layoutHandler;
                if (_pendingButtonTints.TryGetValue(btn, out var pt))
                {
                    pt.LayoutHandler = null;
                    AttachAndTint(img, pt.File, pt.Tint);
                }
            };
            pending.LayoutHandler = layoutHandler;
            button.LayoutUpdated += layoutHandler;
        }

        /// <summary>
        /// Updates the <see cref="ImageTintState"/> for <paramref name="image"/>,

        /// subscribes to <c>ImageOpened</c> (replacing any prior subscription) so
        /// every future MAUI source reset is immediately re-tinted, then schedules
        /// an immediate tint pass on the dispatcher queue.
        /// </summary>
        private static void AttachAndTint(WinUIImage image, string file, Color? tint)
        {
            var state = _imageStates.GetOrCreateValue(image);

            // Remove old ImageOpened handler before attaching a new one.
            if (state.OpenedHandler is not null)
            {
                image.ImageOpened -= state.OpenedHandler;
                state.OpenedHandler = null;
            }

            state.File = file;
            state.Tint = tint;

            // Subscribe so every time MAUI resets the source (e.g. chevron flip,
            // first paint of a newly-visible collapsed section), we re-tint.
            RoutedEventHandler openedHandler = null!;
            openedHandler = (s, e) =>
            {
                // s is the WinUIImage; read current state at the time the event fires.
                if (_imageStates.TryGetValue(image, out var st))
                    ScheduleTint(image, st.File, st.Tint);
            };
            state.OpenedHandler = openedHandler;
            image.ImageOpened += openedHandler;

            // Also schedule an immediate tint in case the source is already loaded
            // (WriteableBitmap does not fire ImageOpened, and BitmapImage may already
            // be decoded if it was cached by WinUI).
            ScheduleTint(image, file, tint);
        }

        /// <summary>
        /// Enqueues the async Win2D tint operation on the element's dispatcher
        /// and sets the resulting <see cref="WriteableBitmap"/> as the new source.
        /// </summary>
        private static void ScheduleTint(WinUIImage image, string file, Color? tint)
        {
            _ = image.DispatcherQueue.TryEnqueue(
                Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal,
                async () =>
                {
                    var wb = await GetTintedBitmapAsync(file, tint);
                    if (wb is not null)
                        image.Source = wb;
                });
        }

        /// <summary>
        /// Produces a tinted <see cref="WriteableBitmap"/> for <paramref name="scaledFile"/>
        /// by running a Win2D <see cref="ColorMatrixEffect"/> that sets every pixel's RGB
        /// to the tint colour while preserving the source alpha channel.
        /// Returns <c>null</c> when <paramref name="tint"/> is <c>null</c> or on error.
        /// </summary>
        private static async Task<WriteableBitmap?> GetTintedBitmapAsync(string scaledFile, Color? tint)
        {
            if (tint is null)
                return null;

            var winColour = ToWinUIColor(tint);
            var cacheKey  = (scaledFile, winColour);

            if (_tintCache.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {
                _canvasDevice ??= CanvasDevice.GetSharedDevice();

                // Load from the output directory where WindowsPackageType=None copies MauiImage files.
                var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, scaledFile);

                // Use the Stream overload to avoid Win2D URI resolution issues in unpackaged apps.
                // CanvasBitmap.LoadAsync(uri) uses WinRT StorageFile internally, which is
                // unreliable for absolute file:// paths without package identity.
                CanvasBitmap source;
                using (var fileStream = System.IO.File.OpenRead(filePath))
                {
                    source = await CanvasBitmap.LoadAsync(
                        _canvasDevice, fileStream.AsRandomAccessStream(), 96f);
                }

                // Use the source pixel dimensions for the render target.
                // The resulting WriteableBitmap is sized to match; MAUI's Image control
                // scales it to the declared WidthRequest/HeightRequest on screen.
                int w = (int)source.SizeInPixels.Width;
                int h = (int)source.SizeInPixels.Height;

                // ColorMatrixEffect - 5x4 matrix (Win2D convention, input vector [R,G,B,A,1]):
                //   R_out = M11*R + M21*G + M31*B + M41*A + M51
                //   G_out = M12*R + M22*G + M32*B + M42*A + M52
                //   B_out = M13*R + M23*G + M33*B + M43*A + M53
                //   A_out = M14*R + M24*G + M34*B + M44*A + M54
                // Goal: R_out=tr, G_out=tg, B_out=tb, A_out=A (preserve alpha).
                float tr = winColour.R / 255f;
                float tg = winColour.G / 255f;
                float tb = winColour.B / 255f;

                var effect = new ColorMatrixEffect
                {
                    Source      = source,
                    ColorMatrix = new Matrix5x4
                    {
                        // Zero out source RGB contribution entirely.
                        M11 = 0, M12 = 0, M13 = 0, M14 = 0,
                        M21 = 0, M22 = 0, M23 = 0, M24 = 0,
                        M31 = 0, M32 = 0, M33 = 0, M34 = 0,
                        M41 = 0, M42 = 0, M43 = 0, M44 = 1,  // preserve alpha unchanged
                        // Offset column: constant tint colour added to every pixel.
                        M51 = tr, M52 = tg, M53 = tb, M54 = 0
                    }
                };

                using var rt = new CanvasRenderTarget(_canvasDevice, w, h, 96f);
                using (var ds = rt.CreateDrawingSession())
                {
                    ds.Clear(WinUIColor.FromArgb(0, 0, 0, 0));
                    ds.DrawImage(effect);
                }

                var pixels = rt.GetPixelBytes();
                var wb     = new WriteableBitmap(w, h);
                using (var stream = wb.PixelBuffer.AsStream())
                    await stream.WriteAsync(pixels, 0, pixels.Length);

                wb.Invalidate();
                _tintCache[cacheKey] = wb;
                return wb;
            }
            catch (Exception ex)
            {
                // Log in debug builds so Win2D pipeline errors are visible in Output window.
                System.Diagnostics.Debug.WriteLine(
                    $"[TintHelper] GetTintedBitmapAsync failed for '{scaledFile}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts the scaled PNG filename (e.g. <c>info.scale-100.png</c>) from a
        /// MAUI <see cref="IImageSource"/>. Returns <c>null</c> for non-file sources.
        /// </summary>
        private static string? GetScaledFileName(Microsoft.Maui.IImageSource? source)
        {
            if (source is not FileImageSource fis)
                return null;

            var plain = fis.File; // e.g. "info.png"
            if (string.IsNullOrEmpty(plain))
                return null;

            // Resizetizer renames files to {stem}.scale-100.{ext} for Windows.
            var stem = System.IO.Path.GetFileNameWithoutExtension(plain);
            var ext  = System.IO.Path.GetExtension(plain);
            return $"{stem}.scale-100{ext}";
        }

        /// <summary>
        /// Walks the WinUI visual tree to find the first
        /// <see cref="WinUIImage"/> descendant of <paramref name="parent"/>.
        /// Returns <c>null</c> if the control template has not yet been applied.
        /// </summary>
        private static WinUIImage? FindDescendantImage(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is WinUIImage img)
                    return img;
                var found = FindDescendantImage(child);
                if (found is not null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Converts a MAUI <see cref="Color"/> to <see cref="WinUIColor"/>,

        /// clamping all components to [0, 1] before byte-scaling to avoid
        /// <c>ArgumentException</c> from out-of-range values.
        /// </summary>
        private static WinUIColor ToWinUIColor(Color c) =>
            WinUIColor.FromArgb(
                (byte)(Math.Clamp(c.Alpha, 0f, 1f) * 255),
                (byte)(Math.Clamp(c.Red,   0f, 1f) * 255),
                (byte)(Math.Clamp(c.Green, 0f, 1f) * 255),
                (byte)(Math.Clamp(c.Blue,  0f, 1f) * 255));
    }
}