using Aeonpulse.Attributes;
using ObjCRuntime;
using UIKit;

namespace Aeonpulse
{
    /// <summary>
    /// The static entry point for the iOS application.
    /// Delegates immediately to <see cref="UIApplication.Main"/> with
    /// <see cref="AppDelegate"/> as the UIKit application delegate class.
    /// </summary>
    [AIContext("PlatformEntryPoint")]
    public class Program
    {
        /// <summary>
        /// iOS application entry point. Called by the iOS runtime after the process launches.
        /// </summary>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}
