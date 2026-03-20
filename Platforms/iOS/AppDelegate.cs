using Aeonpulse.Attributes;
using Foundation;

namespace Aeonpulse
{
    /// <summary>
    /// iOS application delegate — the platform entry point that bootstraps the MAUI host.
    /// The <c>[Register]</c> attribute makes this class visible to the Objective-C runtime
    /// so UIKit can instantiate it as the app delegate.
    /// </summary>
    [AIContext("PlatformEntryPoint")]
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        /// <inheritdoc />
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
