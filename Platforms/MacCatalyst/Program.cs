using Aeonpulse.Attributes;
using ObjCRuntime;
using UIKit;

namespace Aeonpulse;

/// <summary>
/// Static entry point for the Mac Catalyst target.
/// Structurally identical to <c>Platforms\iOS\Program.cs</c> because Mac Catalyst
/// uses the same UIKit-based UIApplication bootstrap path.
/// </summary>
[AIContext("PlatformEntryPoint")]
public class Program
{
    /// <summary>Mac Catalyst process entry point.</summary>
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
