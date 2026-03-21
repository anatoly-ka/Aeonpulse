using System.Globalization;
using Aeonpulse.Resources;

namespace Aeonpulse.Tests.Helpers
{
    /// <summary>
    /// Shared test fixture. Pins AppResources.Culture to English before every
    /// test class so that all string assertions are language-stable regardless
    /// of the OS locale of the machine running the tests.
    /// </summary>
    public static class TestFixture
    {
        public static void InitEnglish()
        {
            var en = new CultureInfo("en");
            CultureInfo.DefaultThreadCurrentCulture   = en;
            CultureInfo.DefaultThreadCurrentUICulture = en;
            AppResources.Culture = en;
        }
    }
}
