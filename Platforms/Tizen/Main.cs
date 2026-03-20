using Aeonpulse.Attributes;
using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Aeonpulse;

/// <summary>
/// Entry point for the Tizen platform target.
/// Inherits from <see cref="MauiApplication"/> which adapts the MAUI host
/// to the Tizen application lifecycle.
/// </summary>
[AIContext("PlatformEntryPoint")]
class Program : MauiApplication
{
    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    /// <summary>Tizen process entry point.</summary>
    static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}
