using Aeonpulse.Attributes;
using Foundation;

namespace Aeonpulse;

/// <summary>
/// Mac Catalyst application delegate — structurally identical to the iOS delegate
/// since Mac Catalyst runs the iOS UIKit stack on macOS.
/// </summary>
[AIContext("PlatformEntryPoint")]
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
