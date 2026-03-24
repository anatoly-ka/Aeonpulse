using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateCosmicStretch"/>.
    ///
    /// Methodology: expansion rate = 3,300,000 km/s (Hubble-Lemaitre Law applied to
    /// the ~46.5 billion light-year observable universe radius with H0 ~ 70 km/s/Mpc).
    /// Core formula: kmExpanded = (now - baseDate).TotalSeconds * 3,300,000.
    /// Distance is displayed in million km (increments by ~3 per second - visibly live).
    /// </summary>
    public class CalculateCosmicStretchTests
    {
        private readonly CalculationService _svc;

        public CalculateCosmicStretchTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void ExactlyOneSecond_ReturnsCorrectKmExpanded()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(1);

            var result = _svc.CalculateCosmicStretch(baseDate, "2000-01-01", useMetric: true, now: now);

            Assert.Equal(3_300_000.0, result.KmExpanded, precision: 0);
        }

        [Fact]
        public void HappyPath_MetricContainsMillionKm()
        {
            // 60 years produces ~6.25e15 km = ~6,248,404,800 million km
            var baseDate = new DateTime(1965, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCosmicStretch(baseDate, "1965-01-01", useMetric: true, now: now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
            Assert.Contains("million km", result.Distance);
        }

        [Fact]
        public void HappyPath_ImperialContainsMillionMiles()
        {
            var baseDate = new DateTime(1965, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCosmicStretch(baseDate, "1965-01-01", useMetric: false, now: now);

            Assert.Contains("million miles", result.Distance);
        }

        [Fact]
        public void MetricKmExpandedMatchesFormula()
        {
            // 1 hour = 3600 seconds
            var baseDate = new DateTime(2000, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(3600);

            var result = _svc.CalculateCosmicStretch(baseDate, "2000-06-15", useMetric: true, now: now);

            double expected = 3600.0 * 3_300_000.0;
            Assert.Equal(expected, result.KmExpanded, precision: 0);
        }

        [Fact]
        public void DistanceIncrementsEachSecond_Metric()
        {
            // At 3,300,000 km/s: 1s adds 3.3 million km, so integer million-km
            // increments by 3 each second (floor division). Verify two consecutive
            // seconds differ by exactly 3 million km.
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now1     = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            var now2     = now1.AddSeconds(1);

            var r1 = _svc.CalculateCosmicStretch(baseDate, "2000-01-01", useMetric: true, now: now1);
            var r2 = _svc.CalculateCosmicStretch(baseDate, "2000-01-01", useMetric: true, now: now2);

            long millionKm1 = (long)(r1.KmExpanded / 1_000_000);
            long millionKm2 = (long)(r2.KmExpanded / 1_000_000);
            Assert.Equal(3L, millionKm2 - millionKm1);
        }

        [Fact]
        public void ZeroElapsedTime_ReturnsNonNullNonEmpty()
        {
            var baseDate = new DateTime(2000, 6, 15, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCosmicStretch(baseDate, "2000-06-15", useMetric: true, now: baseDate);

            Assert.NotNull(result);
            Assert.NotNull(result.BriefText);
            Assert.NotNull(result.FullText);
        }

        [Fact]
        public void VeryOldDate_DoesNotThrow()
        {
            var baseDate = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Record.Exception(() =>
                _svc.CalculateCosmicStretch(baseDate, "1800-01-01", useMetric: true, now: now));

            Assert.Null(ex);
        }

        [Fact]
        public void BriefTextContainsDistanceToken()
        {
            var baseDate = new DateTime(1990, 3, 20, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 3, 20, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCosmicStretch(baseDate, "1990-03-20", useMetric: true, now: now);

            string stripped = result.BriefText.Replace("<b>", "").Replace("</b>", "");
            Assert.Contains("million km", stripped);
        }

        [Fact]
        public void FullTextContainsBaseDateFormattedString()
        {
            var baseDate = new DateTime(1985, 7, 4, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 7, 4, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCosmicStretch(baseDate, "1985-07-04", useMetric: true, now: now);

            Assert.Contains(baseDate.ToString("d"), result.FullText);
        }

        [Fact]
        public void UseMetricFalse_KmExpandedIsIndependentOfUnitSystem()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(86400);

            var metric   = _svc.CalculateCosmicStretch(baseDate, "2000-01-01", useMetric: true, now: now);
            var imperial = _svc.CalculateCosmicStretch(baseDate, "2000-01-01", useMetric: false, now: now);

            Assert.Equal(metric.KmExpanded, imperial.KmExpanded, precision: 0);
        }

        [Fact]
        public void FullTextDoesNotContainFullDistanceToken()
        {
            // The {fullDistance} token is intentionally absent from the template
            // because raw km is unreadable at this scale.
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(3600);

            var result = _svc.CalculateCosmicStretch(baseDate, "2000-01-01", useMetric: true, now: now);

            Assert.DoesNotContain("{fullDistance}", result.FullText);
        }
    }
}
