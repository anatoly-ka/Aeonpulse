using Android.App;
using Android.Content.PM;
using Android.OS;
using Aeonpulse.Attributes;

namespace Aeonpulse
{
    /// <summary>
    /// The single Android Activity that hosts the entire MAUI application.
    /// Marked as <c>MainLauncher = true</c> so Android starts it on app launch.
    ///
    /// <para>
    /// <b>ConfigurationChanges:</b> all common configuration change types are listed
    /// to prevent Activity destruction/recreation on orientation, dark-mode, or
    /// density changes — MAUI handles these internally via its layout pipeline.
    /// </para>
    /// </summary>
    [AIContext("PlatformEntryPoint")]
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation |
                               ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                               ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
