using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateGlobalExhale"/>.
    ///
    /// Three branches: before 1900 (constant), 1900-present (polynomial).
    /// Also validates metric/imperial toggle.
    /// </summary>
    public class CalculateGlobalExhaleTests
    {
        private readonly CalculationService _svc;

        public CalculateGlobalExhaleTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void DateBefore1900_ReturnsPre1900Message()
        {
            var result = _svc.CalculateGlobalExhale(
                new DateTime(1850, 1, 1), "Test", "1850-01-01", useMetric: true,
                now: new DateTime(2026, 1, 1));

            Assert.Contains("1900", result.BriefText);
        }

        [Fact]
        public void DateAfter1900_BriefTextContainsCO2Amount()
        {
            var baseDate = new DateTime(1965, 7, 24);
            var now      = new DateTime(2026, 1, 1);

            var result = _svc.CalculateGlobalExhale(baseDate, "Test", "1965-07-24", useMetric: true, now);

            // Brief text must mention CO2 (written as "CO2" or "CO2" with subscript unicode)
            Assert.Contains("CO", result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void MetricResult_ContainsBTonnes()
        {
            var baseDate = new DateTime(1980, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var metric   = _svc.CalculateGlobalExhale(baseDate, "T", "1980-01-01", useMetric: true,  now);
            var imperial = _svc.CalculateGlobalExhale(baseDate, "T", "1980-01-01", useMetric: false, now);

            Assert.Contains("tonnes", metric.BriefText,   StringComparison.OrdinalIgnoreCase);
            Assert.Contains("tons",   imperial.BriefText,  StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void LargerDateRange_ProducesMoreCO2ThanSmallerRange()
        {
            var baseDate  = new DateTime(1960, 1, 1);
            var baseDate2 = new DateTime(1990, 1, 1);
            var now       = new DateTime(2026, 1, 1);

            // Extract numeric portion to compare
            // Both return "X.XX billion tonnes of CO2 emitted"
            // Longer window should produce more CO2
            var long_ = _svc.CalculateGlobalExhale(baseDate,  "T", "1960-01-01", true, now);
            var short_ = _svc.CalculateGlobalExhale(baseDate2, "T", "1990-01-01", true, now);

            // Parse first number from BriefText (strip HTML tags first)
            double ParseAmount(string text)
            {
                var plain = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", "");
                var token = plain.Split(' ').FirstOrDefault(t => double.TryParse(t, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _));
                return token != null ? double.Parse(token, System.Globalization.CultureInfo.InvariantCulture) : 0;
            }

            Assert.True(ParseAmount(long_.BriefText) > ParseAmount(short_.BriefText));
        }
    }
}
