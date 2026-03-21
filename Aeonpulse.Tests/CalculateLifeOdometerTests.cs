using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateLifeOdometer"/>.
    ///
    /// Rates: 70 heartbeats/min, 16 breaths/min.
    /// Formula: heartbeats = totalSeconds * 70 / 60, breaths = totalSeconds * 16 / 60.
    /// </summary>
    public class CalculateLifeOdometerTests
    {
        private readonly CalculationService _svc;

        public CalculateLifeOdometerTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void ExactlyOneMinute_Returns70HeartbeatsAnd16Breaths()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(60);

            var result = _svc.CalculateLifeOdometer(baseDate, "Test", "2000-01-01", now);

            Assert.Contains("70",  result.BriefText);
            Assert.Contains("16",  result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void ExactlyOneHour_Returns4200HeartbeatsAnd960Breaths()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(3600);

            var result = _svc.CalculateLifeOdometer(baseDate, "Test", "2000-01-01", now);

            // 3600 * 70 / 60 = 4200; 3600 * 16 / 60 = 960
            Assert.Contains("4,200", result.BriefText);
            Assert.Contains("960",   result.BriefText);
        }

        [Fact]
        public void ZeroElapsedTime_ReturnsNonNullNonEmpty()
        {
            var baseDate = new DateTime(2000, 6, 15);

            var result = _svc.CalculateLifeOdometer(baseDate, "Test", "2000-06-15", baseDate);

            Assert.NotNull(result);
            Assert.NotNull(result.BriefText);
            Assert.NotNull(result.FullText);
        }

        [Fact]
        public void VeryOldDate_DoesNotThrow()
        {
            var baseDate = new DateTime(1800, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var ex = Record.Exception(() =>
                _svc.CalculateLifeOdometer(baseDate, "Test", "1800-01-01", now));

            Assert.Null(ex);
        }
    }
}
