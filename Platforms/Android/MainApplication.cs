using Android.App;
using Android.Runtime;
using Aeonpulse.Attributes;

namespace Aeonpulse;

/// <summary>
/// The Android <see cref="Application"/> subclass that bootstraps the MAUI host.
/// Instantiated by the Android runtime before <see cref="MainActivity"/>, making it
/// the earliest possible hook for cross-cutting initialisation.
///
/// <para>
/// Delegates <see cref="CreateMauiApp"/> to <see cref="MauiProgram.CreateMauiApp"/>,
/// which registers fonts, handler mappers, and the DI container.
/// </para>
/// </summary>
[AIContext("PlatformEntryPoint")]
[Application]
public class MainApplication : MauiApplication
{
    /// <param name="handle">JNI handle provided by the Android runtime.</param>
    /// <param name="ownership">JNI handle ownership semantics.</param>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
