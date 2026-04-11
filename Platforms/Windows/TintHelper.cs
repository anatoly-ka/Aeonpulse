using Aeonpulse.Attributes;
using Aeonpulse.Services;
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
            // Live reference to the MAUI BindableObject so ImageOpened can read
            // ImageTint.GetColor() at fire-time rather than relying on the cached
            // Tint field, which can be stale when Handler was null during a theme change
            // (OnColorChanged -> UpdateValue no-op -> AttachAndTint never called).
            public BindableObject? MauiView;
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
            public String File = "";
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
        ///
        /// <para>
        /// <b>Why AttachAndTint instead of a direct ScheduleTint:</b>
        /// When <c>LandmarkImage.Source</c> is assigned a <c>StreamImageSource</c>,
        /// MAUI decodes the stream asynchronously and calls <c>nativeImage.Source =
        /// decodedBitmap</c> some time after this mapper fires.  A bare
        /// <c>ScheduleTint</c> would replace the source with the tinted bitmap, but
        /// MAUI's decode completion then overwrites it with the raw untinted bitmap.
        /// <c>AttachAndTint</c> subscribes to <c>ImageOpened</c> so the tint is
        /// re-applied every time WinUI finishes loading a new bitmap — the same
        /// mechanism already used by <c>ApplyImageButtonTint</c>.
        /// </para>
        /// </summary>
        static partial void ApplyImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Color? tint)
        {
            if (handler.PlatformView is not WinUIImage nativeImage)
                return;

            var mauiView = handler.VirtualView as BindableObject;
            var fallback = (mauiView as Microsoft.Maui.Controls.Element)?.AutomationId;
            var file = GetScaledFileName(handler.VirtualView?.Source, fallback);
            if (file is null) return;

            AttachAndTint(nativeImage, file, tint, mauiView);
        }

        /// <summary>
        /// No-op on Windows: MAUI's built-in GIF decoder (<c>ParseGIFBitmapHeaderAsync</c>
        /// in <c>Microsoft.Maui.Controls.dll</c>) implements its own frame-by-frame
        /// <see cref="WriteableBitmap"/> renderer driven by a timer, producing native
        /// animated GIF playback on Windows without any platform-side intervention.
        /// The static PNG swap introduced in the previous session was incorrect � it
        /// intercepted the <c>"Source"</c> mapper key and replaced the GIF with a frozen
        /// PNG before MAUI's decoder could run, which is why the GIF appeared static.
        /// Removing the swap lets MAUI's decoder run normally.
        /// </summary>
        static partial void ApplyGifToStaticPngMapper()
        {
            // Intentionally empty. MAUI handles GIF animation on Windows natively.
        }

        // Windows handles deferred tinting via the ImageOpened subscription in
        // AttachAndTint - no additional post-decode callback is needed here.
        static partial void ApplyDeferredImageTint(
            Microsoft.Maui.Handlers.ImageHandler handler, Microsoft.Maui.Graphics.Color? tint)
        {
        }

        /// <summary>
        /// Pre-warms the Win2D tint cache for <paramref name="fileName"/> by running
        /// <see cref="GetTintedBitmapAsync"/> inline on the calling (UI) thread.
        /// Called from <c>ApplyVolumeCubeAsync</c> and awaited before
        /// <c>LandmarkImage.Source</c> is assigned, so the cache is hot by the time
        /// <see cref="ScheduleTint"/> runs in response to the Source change.
        /// </summary>
        internal static async partial Task PrewarmTintCache(string fileName, Microsoft.Maui.Graphics.Color tint)
        {
            var scaledFile = GetScaledFileName(null, fileName);
            if (scaledFile is null) return;

            await GetTintedBitmapAsync(scaledFile, tint);
        }

        /// <summary>
        /// On Windows, landmark PNGs are <c>MauiAsset</c> files copied verbatim to
        /// <c>AppContext.BaseDirectory</c>.  <c>FromFile</c> produces a <c>BitmapImage</c>
        /// which fires <c>ImageOpened</c> on WinUI <c>Image</c>, allowing
        /// <c>AttachAndTint</c>'s handler to re-apply the tint after every decode.
        /// <c>FromStream</c> would produce a <c>WriteableBitmap</c> which does NOT fire
        /// <c>ImageOpened</c>, leaving the raw bitmap visible.
        /// </summary>
        internal static partial Microsoft.Maui.Controls.ImageSource LandmarkImageSource(string fileName)
            => Microsoft.Maui.Controls.ImageSource.FromFile(fileName);

        /// <summary>Number of tinted bitmaps in the cache. Exposed for diagnostics.</summary>
        internal static int TintCacheCount => _tintCache.Count;

        /// <summary>
        /// Pre-warns the Win2D tint cache for every image that carries
        /// <c>helpers:ImageTint.Color</c> in the app, using <paramref name="tint"/>
        /// as the colour (all tinted images use the same <c>CyberCyan</c> token).
        ///
        /// <para>Called from <c>MainPage.OnAppearing</c> on Windows while the
        /// <c>LoadingOverlay</c> is visible. After this method returns every
        /// subsequent <see cref="ScheduleTint"/> call is a synchronous cache hit
        /// and completes before yielding to any competing async continuation.</para>
        /// </summary>
        internal static async Task WarmAllTintCachesAsync(Microsoft.Maui.Graphics.Color tint)
        {
            var swTotal = System.Diagnostics.Stopwatch.StartNew();
            AeonLog.Info("TINT", nameof(WarmAllTintCachesAsync),
                $"START tint={tint.ToArgbHex()}");

            // Fixed MauiImage icons that are tinted in MainPage.xaml.
            // Resizetizer adds .scale-100 suffix; GetScaledFileName resolves it.
            var fixedImages = new[]
            {
                "aeonpulse.png",
                "chevron_up.png", "chevron_down.png",
                "square_chevron_up.png", "square_chevron_down.png",
                "in_favorites.png", "to_favorites.png",
                "info.png", "refresh.png",
                "icon_taxonomy.png",
                // MainMenuPopup.xaml icons - pre-warmed so first menu open is a cache hit.
                "profiles.png", "settings.png", "exit.png",
            };

            // All landmark MauiAsset PNGs (digit-prefixed, no scale suffix).
            var landmarkImages = new[]
            {
                "01.7_human.png", "04.4_double-decker-bus.png",
                "07_Stonehenge.png", "10_moai-statues.png",
                "14_hollywood-sign.png", "15_parthenon.png",
                "16.5_itsukushima-shrine.png", "20_great-sphinx-of-giza.png",
                "21_white-house.png", "26_brandenburg-gate.png",
                "30_blue-whale.png", "38_christ-the-redeemer.png",
                "46_statue-of-liberty.png", "48_colosseum.png",
                "50_arc-de-triomphe.png", "53_ruiguang-tower.png",
                "56_tower-of-pisa.png", "61_egyptian-pyramids-icon.png",
                "65_tower-bridge.png", "69_notre-dame.png",
                "73_taj-mahal.png",
            };

            int ok = 0, fail = 0;
            int imgNum = 0;
            var swImg = new System.Diagnostics.Stopwatch();

            foreach (var name in fixedImages)
            {
                imgNum++;
                var scaledFile = GetScaledFileName(new FileImageSource { File = name });
                if (scaledFile is null)
                {
                    AeonLog.Warn("TINT", nameof(WarmAllTintCachesAsync),
                        $"icon not found on disk  name={name}");
                    fail++;
                    continue;
                }
                swImg.Restart();
                var wb = await GetTintedBitmapAsync(scaledFile, tint);
                swImg.Stop();
                if (wb is not null)
                {
                    ok++;
                    AeonLog.Debug("TINT", nameof(WarmAllTintCachesAsync),
                        $"img#{imgNum:D2} icon  name={scaledFile}  ms={swImg.ElapsedMilliseconds}  total_ms={swTotal.ElapsedMilliseconds}",
                        "IMG_TIMING");
                }
                else
                {
                    fail++;
                    AeonLog.Warn("TINT", nameof(WarmAllTintCachesAsync),
                        $"img#{imgNum:D2} icon FAILED  name={scaledFile}  ms={swImg.ElapsedMilliseconds}");
                }
            }

            foreach (var name in landmarkImages)
            {
                imgNum++;
                var scaledFile = GetScaledFileName(null, name);
                if (scaledFile is null)
                {
                    AeonLog.Warn("TINT", nameof(WarmAllTintCachesAsync),
                        $"landmark not found on disk  name={name}");
                    fail++;
                    continue;
                }
                swImg.Restart();
                var wb = await GetTintedBitmapAsync(scaledFile, tint);
                swImg.Stop();
                if (wb is not null)
                {
                    ok++;
                    AeonLog.Debug("TINT", nameof(WarmAllTintCachesAsync),
                        $"img#{imgNum:D2} landmark  name={scaledFile}  ms={swImg.ElapsedMilliseconds}  total_ms={swTotal.ElapsedMilliseconds}",
                        "IMG_TIMING");
                }
                else
                {
                    fail++;
                    AeonLog.Warn("TINT", nameof(WarmAllTintCachesAsync),
                        $"img#{imgNum:D2} landmark FAILED  name={scaledFile}  ms={swImg.ElapsedMilliseconds}");
                }
            }

            swTotal.Stop();
            AeonLog.Info("TINT", nameof(WarmAllTintCachesAsync),
                $"DONE  cached={ok}  failed={fail}  cacheSize={_tintCache.Count}  total_ms={swTotal.ElapsedMilliseconds}");
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

            // Pass the MAUI BindableObject so AttachAndTint can store it in
            // ImageTintState for use by the ImageOpened handler (live color read).
            var mauiView = handler.VirtualView as BindableObject;
            ApplyTintToButton(nativeBtn, file, tint, mauiView);
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
        private static void ApplyTintToButton(WinUIButton button, string file, Color? tint, BindableObject? mauiView = null)
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

            button.ApplyTemplate();

            var image = FindDescendantImage(button);
            if (image is not null)
            {
                AttachAndTint(image, file, tint, mauiView);
                return;
            }

            var pending = _pendingButtonTints.GetOrCreateValue(button);
            pending.File = file;
            pending.Tint = tint;

            EventHandler<object> layoutHandler = null!;
            layoutHandler = (s, e) =>
            {
                if (s is not WinUIButton btn) return;
                btn.ApplyTemplate();
                var img = FindDescendantImage(btn);
                if (img is null) return;

                btn.LayoutUpdated -= layoutHandler;
                if (_pendingButtonTints.TryGetValue(btn, out var pt))
                {
                    pt.LayoutHandler = null;
                    AttachAndTint(img, pt.File, pt.Tint, mauiView);
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
        private static void AttachAndTint(WinUIImage image, string file, Color? tint, BindableObject? mauiView = null)
        {
            var state = _imageStates.GetOrCreateValue(image);

            if (state.OpenedHandler is not null)
            {
                image.ImageOpened -= state.OpenedHandler;
                state.OpenedHandler = null;
            }

            state.File = file;
            state.Tint = tint;
            if (mauiView is not null)
                state.MauiView = mauiView;

            RoutedEventHandler openedHandler = null!;
            openedHandler = (s, e) =>
            {
                if (!_imageStates.TryGetValue(image, out var st)) return;

                var liveTint = st.MauiView is not null
                    ? Helpers.ImageTint.GetColor(st.MauiView)
                    : st.Tint;

                AeonLog.Debug("TINT", "ImageOpened",
                    $"file={st.File}  liveTint={liveTint?.ToArgbHex() ?? "NULL"}");
                ScheduleTint(image, st.File, liveTint);
            };
            state.OpenedHandler = openedHandler;
            image.ImageOpened += openedHandler;

            var immediateTint = mauiView is not null
                ? Helpers.ImageTint.GetColor(mauiView) ?? tint
                : tint;

            ScheduleTint(image, file, immediateTint);
        }

        /// <summary>
        /// Applies the tinted <see cref="WriteableBitmap"/> to <paramref name="image"/>.
        /// If the cache is already warm the bitmap is set synchronously on the calling
        /// thread (avoids queuing a dispatcher item when already on the UI thread).
        /// For a cold cache, file I/O and Win2D rendering run on the UI dispatcher via
        /// <see cref="GetTintedBitmapAsync"/>; the call is awaited directly so no
        /// additional dispatcher item is queued behind already-pending work.
        /// </summary>
        private static void ScheduleTint(WinUIImage image, string file, Color? tint)
        {
            var winColour = tint is not null ? (WinUIColor?)ToWinUIColor(tint) : null;
            if (winColour.HasValue && _tintCache.TryGetValue((file, winColour.Value), out var cached))
            {
                AeonLog.Debug("TINT", nameof(ScheduleTint),
                    $"FAST (cache hit)  file={file}");
                ApplyWbToImage(image, cached);
                return;
            }

            AeonLog.Warn("TINT", nameof(ScheduleTint),
                $"COLD (cache miss)  file={file}  tint={tint?.ToArgbHex() ?? "NULL"}  " +
                $"cacheSize={_tintCache.Count}");
            _ = ApplyTintAsync(image, file, tint);
        }

        private static async Task ApplyTintAsync(WinUIImage image, string? file, Color? tint)
        {
            var wb = await GetTintedBitmapAsync(file, tint);
            if (wb is null) return;
            ApplyWbToImage(image, wb);
        }

        /// <summary>
        /// Sets <paramref name="wb"/> as <paramref name="image"/>'s source, detaching
        /// the <c>ImageOpened</c> handler first to prevent re-entrancy, then re-attaching.
        /// </summary>
        private static void ApplyWbToImage(WinUIImage image, WriteableBitmap wb)
        {
            _imageStates.TryGetValue(image, out var st);
            if (st?.OpenedHandler is not null)
                image.ImageOpened -= st.OpenedHandler;

            image.Source = wb;

            if (st?.OpenedHandler is not null)
                image.ImageOpened += st.OpenedHandler;
        }

        /// <summary>
        /// Produces a tinted <see cref="WriteableBitmap"/> for <paramref name="scaledFile"/>.
        /// Must be called on the UI thread (Win2D APIs and WriteableBitmap require it).
        /// Results are cached by (filename, colour) so repeated calls for the same
        /// combination are instant cache hits with no async overhead.
        /// Returns <c>null</c> when <paramref name="tint"/> is <c>null</c> or on error.
        /// </summary>
        private static async Task<WriteableBitmap?> GetTintedBitmapAsync(string? scaledFile, Color? tint)
        {
            if (tint is null || scaledFile is null)
                return null;

            var winColour = ToWinUIColor(tint);
            var cacheKey  = (scaledFile, winColour);

            if (_tintCache.TryGetValue(cacheKey, out var cached))
                return cached;

            try
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();

                _canvasDevice ??= CanvasDevice.GetSharedDevice();

                var filePath = System.IO.Path.Combine(AppContext.BaseDirectory, scaledFile);

                if (!System.IO.File.Exists(filePath))
                {
                    AeonLog.Warn("TINT", nameof(GetTintedBitmapAsync),
                        $"file not on disk  path={filePath}");
                    return null;
                }

                CanvasBitmap source;
                using (var fileStream = System.IO.File.OpenRead(filePath))
                {
                    source = await CanvasBitmap.LoadAsync(
                        _canvasDevice, fileStream.AsRandomAccessStream(), 96f);
                }
                var msLoad = sw.ElapsedMilliseconds;

                int w = (int)source.SizeInPixels.Width;
                int h = (int)source.SizeInPixels.Height;

                float tr = winColour.R / 255f;
                float tg = winColour.G / 255f;
                float tb = winColour.B / 255f;

                var effect = new ColorMatrixEffect
                {
                    Source      = source,
                    ColorMatrix = new Matrix5x4
                    {
                        M11 = 0, M12 = 0, M13 = 0, M14 = 0,
                        M21 = 0, M22 = 0, M23 = 0, M24 = 0,
                        M31 = 0, M32 = 0, M33 = 0, M34 = 0,
                        M41 = 0, M42 = 0, M43 = 0, M44 = 1,
                        M51 = tr, M52 = tg, M53 = tb, M54 = 0
                    }
                };

                using var rt = new CanvasRenderTarget(_canvasDevice, w, h, 96f);
                using (var ds = rt.CreateDrawingSession())
                {
                    ds.Clear(WinUIColor.FromArgb(0, 0, 0, 0));
                    ds.DrawImage(effect);
                }
                var msRender = sw.ElapsedMilliseconds - msLoad;

                var pixels = rt.GetPixelBytes();
                var wb = new WriteableBitmap(w, h);
                using (var stream = wb.PixelBuffer.AsStream())
                    await stream.WriteAsync(pixels, 0, pixels.Length);
                var msPixelCopy = sw.ElapsedMilliseconds - msLoad - msRender;

                wb.Invalidate();
                _tintCache[cacheKey] = wb;
                AeonLog.Debug("TINT", nameof(GetTintedBitmapAsync),
                    $"cached  file={scaledFile}  size={w}x{h}  ms_load={msLoad}  ms_render={msRender}  ms_pixel={msPixelCopy}  ms_total={sw.ElapsedMilliseconds}");
                return wb;
            }
            catch (Exception ex)
            {
                AeonLog.Warn("TINT", nameof(GetTintedBitmapAsync),
                    $"FAILED  file={scaledFile}  ex={ex.GetType().Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Extracts the resolved PNG filename from a MAUI <see cref="IImageSource"/>.
        /// For <see cref="FileImageSource"/>: first tries the Resizetizer-scaled name
        /// (<c>{stem}.scale-100.{ext}</c>); if that file does not exist on disk, falls
        /// back to the plain filename so <c>MauiAsset</c> files (which are not processed
        /// by Resizetizer and have no scale suffix) are also found.
        /// For <see cref="StreamImageSource"/>: the source itself carries no filename,
        /// so the caller must pass the original filename via <paramref name="fallbackName"/>.
        /// Returns <c>null</c> when no filename can be resolved.
        /// </summary>
        private static string? GetScaledFileName(Microsoft.Maui.IImageSource? source,
                                                  string? fallbackName = null)
        {
            string? plain;

            if (source is FileImageSource fis)
            {
                plain = fis.File;
            }
            else if (!string.IsNullOrEmpty(fallbackName))
            {
                // StreamImageSource (e.g. landmark PNGs loaded via OpenAppPackageFileAsync):
                // the source itself has no filename; use the caller-supplied fallback.
                plain = fallbackName;
            }
            else
            {
                return null;
            }

            if (string.IsNullOrEmpty(plain))
                return null;

            // Resizetizer renames MauiImage files to {stem}.scale-100{ext} on Windows.
            var stem   = System.IO.Path.GetFileNameWithoutExtension(plain);
            var ext    = System.IO.Path.GetExtension(plain);
            var scaled = $"{stem}.scale-100{ext}";

            // MauiAsset files are copied verbatim (no scale suffix). Try scaled first,
            // then fall back to the plain name so both MauiImage and MauiAsset are handled.
            var scaledPath = System.IO.Path.Combine(AppContext.BaseDirectory, scaled);
            return System.IO.File.Exists(scaledPath) ? scaled : plain;
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
