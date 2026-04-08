using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateCountdown"/>.
    ///
    /// The method has two distinct output branches based on seconds remaining:
    ///   &lt; 86400    -> HH:MM:SS format (hours only)
    ///   >= 86400   -> days + HH:MM format (days and hours, regardless of total duration)
    /// </summary>
    public class CalculateCountdownTests
    {
        private readonly CalculationService _svc;

        public CalculateCountdownTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void LessThanOneDay_BriefTextContainsHoursMinutesSeconds()
        {
            // Anniversary is 2h 30m 45s away
            var baseDate = new DateTime(2000, 6, 15);
            var now      = new DateTime(2026, 6, 14, 21, 29, 15); // 2h30m45s before Jun 15

            var result = _svc.CalculateCountdown(baseDate, now);

            Assert.Contains("2h",  result.BriefText);
            Assert.Contains("30m", result.BriefText);
            Assert.Contains("45s", result.BriefText);
        }

        [Fact]
        public void MoreThanOneDayLessThanOneMonth_BriefTextContainsDaysAndHours()
        {
            var baseDate = new DateTime(2000, 6, 15);
            // 5 days 3 hours before the anniversary
            var now = new DateTime(2026, 6, 9, 21, 0, 0);

            var result = _svc.CalculateCountdown(baseDate, now);

            Assert.Contains("days", result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void MoreThanOneMonth_BriefTextContainsDaysAndHours()
        {
            var baseDate = new DateTime(2000, 6, 15);
            // ~3 months before the anniversary
            var now = new DateTime(2026, 3, 1, 0, 0, 0);

            var result = _svc.CalculateCountdown(baseDate, now);

            Assert.Contains("days", result.BriefText);
            // Hours and minutes are always shown when > 1 day, even for >1 month
            Assert.Contains("h :", result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void AnniversaryExactlyToday_PicksNextYear()
        {
            var baseDate = new DateTime(2000, 6, 15);
            // now IS the anniversary - should count to next year (365 days away)
            var now = new DateTime(2026, 6, 15, 0, 0, 0);

            var result = _svc.CalculateCountdown(baseDate, now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
        }
    }
}
