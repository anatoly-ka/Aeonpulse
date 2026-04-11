using Aeonpulse.Resources;
using Aeonpulse.Services;

namespace Aeonpulse.Views
{
    /// <summary>
    /// Startup splash page shown while the Win2D tint cache is pre-warmed on Windows.
    ///
    /// <para>
    /// <b>Sequence:</b>
    /// <list type="number">
    ///   <item><description>
    ///     <see cref="App"/> sets <c>App.MainPage = new SplashPage()</c> — the splash
    ///     is the very first thing the user sees, with no white-window delay.
    ///   </description></item>
    ///   <item><description>
    ///     <see cref="OnAppearing"/> calls <see cref="RunStartupAsync"/>. On Windows,
    ///     it reads <c>CyberCyan</c> directly from <c>Application.Current.Resources</c>
    ///     (populated by <c>ThemeService.ApplyScheme</c> before <c>InitializeComponent</c>)
    ///     and calls <see cref="MauiProgram.WarmAllTintCachesAsync"/> while the splash
    ///     is visible. Non-Windows platforms skip warming and transition immediately.
    ///   </description></item>
    ///   <item><description>
    ///     When warming is done, <c>App.MainPage</c> is replaced with <see cref="MainPage"/>.
    ///     Every subsequent <see cref="MauiProgram.ScheduleTint"/> call is a synchronous
    ///     cache hit, so the landmark image is never shown un-tinted.
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    public partial class SplashPage : ContentPage
    {
        private const string LogCat = "TINT";

        public SplashPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = RunStartupAsync();
        }

        private async Task RunStartupAsync()
        {
            var t0 = DateTime.Now;
            AeonLog.Info(LogCat, nameof(RunStartupAsync),
                $"START  wall={t0:HH:mm:ss.fff}");
            SplashLabel.Text = AppResources.App_Initializing;

#if WINDOWS
            // Read CyberCyan directly from the merged ResourceDictionary.
            // We MUST NOT use Helpers.ImageTint.GetColor(SplashLogo) here because
            // DynamicResource bindings on MAUI elements are resolved lazily during
            // the first layout pass, which has not yet happened when OnAppearing fires.
            // Application.Current.Resources is fully populated by ThemeService.ApplyScheme
            // which runs in App() before InitializeComponent().
            Microsoft.Maui.Graphics.Color? tint = null;
            if (Application.Current?.Resources.TryGetValue("CyberCyan", out var raw) == true
                && raw is Microsoft.Maui.Graphics.Color c)
            {
                tint = c;
            }

            AeonLog.Info(LogCat, nameof(RunStartupAsync),
                $"tint={tint?.ToArgbHex() ?? "NULL"}");

            if (tint is null)
            {
                AeonLog.Warn(LogCat, nameof(RunStartupAsync),
                    "CyberCyan not found in Resources - skipping warm, landmark may be black");
            }
            else
            {
                var tWarmStart = DateTime.Now;
                AeonLog.Info(LogCat, nameof(RunStartupAsync),
                    $"WARM_START  wall={tWarmStart:HH:mm:ss.fff}  ms_since_appear={(tWarmStart - t0).TotalMilliseconds:F0}");

                await MauiProgram.WarmAllTintCachesAsync(tint);

                var tWarmEnd = DateTime.Now;
                AeonLog.Info(LogCat, nameof(RunStartupAsync),
                    $"WARM_END  wall={tWarmEnd:HH:mm:ss.fff}  ms_warm={(tWarmEnd - tWarmStart).TotalMilliseconds:F0}  ms_since_appear={(tWarmEnd - t0).TotalMilliseconds:F0}");
#if DEBUG
                MemSnapshot.Emit("POST_WARM");
#endif
            }
#endif

            var tNav = DateTime.Now;
            AeonLog.Info(LogCat, nameof(RunStartupAsync),
                $"NAVIGATE  wall={tNav:HH:mm:ss.fff}  ms_splash_visible={(tNav - t0).TotalMilliseconds:F0}");
            Application.Current!.MainPage = new MainPage();
        }
    }
}
