using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateAlienAnniversaries"/>.
    ///
    /// Constants: Mars year = 686.98 Earth days, Venus year = 224.7 Earth days.
    /// </summary>
    public class CalculateAlienAnniversariesTests
    {
        private readonly CalculationService _svc;

        public CalculateAlienAnniversariesTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void AfterExactlyOneMarsYear_ShowsApproximately1_00MarsYears()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = baseDate.AddDays(686.98);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "Test", "2000-01-01", now);

            Assert.Contains("1.00", result.BriefText);
            Assert.Contains("Mars", result.BriefText);
        }

        [Fact]
        public void AfterExactlyOneVenusYear_ShowsApproximately1_00VenusYears()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = baseDate.AddDays(224.7);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "Test", "2000-01-01", now);

            Assert.Contains("1.00", result.BriefText);
            Assert.Contains("Venus", result.BriefText);
        }

        [Fact]
        public void After365EarthDays_MarsYearsIsCorrect()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = baseDate.AddDays(365);
            // 365 / 686.98 = 0.53 Mars years
            double expectedMars = Math.Round(365.0 / 686.98, 2);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "Test", "2000-01-01", now);

            Assert.Contains(expectedMars.ToString("F2"), result.BriefText);
        }

        [Fact]
        public void ZeroElapsedTime_ReturnsNonNullResult()
        {
            var baseDate = new DateTime(2000, 1, 1);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "Test", "2000-01-01", baseDate);

            Assert.NotNull(result);
            Assert.NotNull(result.BriefText);
        }
    }
}
