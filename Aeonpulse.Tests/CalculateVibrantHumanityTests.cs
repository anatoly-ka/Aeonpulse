using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateVibrantHumanity"/>.
    ///
    /// Methodology: 3-epoch piecewise linear model anchored to 1900-01-01 UTC.
    /// Births delta = HumanBirthRankbyDate(now) - HumanBirthRankbyDate(baseDate).
    /// Deaths delta = TotalDeathsByDate(now) - TotalDeathsByDate(baseDate).
    /// Sub-ratios: twins = births * 0.024, heart = deaths * 0.27, cancer = deaths * 0.18.
    /// All values formatted N0 in UI strings.
    /// </summary>
    public class CalculateVibrantHumanityTests
    {
        private readonly CalculationService _svc;

        public CalculateVibrantHumanityTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        // ------------------------------------------------------------------ //
        // Happy path                                                           //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_ResultIsNotNullAndTextsAreNonEmpty()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-06-15", now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void HappyPath_BirthsGreaterThanZeroForPastBaseDate()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-06-15", now);

            Assert.True(result.BornBetweenDates > 0,
                "Births since 1990 should be positive.");
        }

        [Fact]
        public void HappyPath_DeathsGreaterThanZeroForPastBaseDate()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-06-15", now);

            Assert.True(result.DiedBetweenDates > 0,
                "Deaths since 1990 should be positive.");
        }

        [Fact]
        public void HappyPath_BirthsExceedDeathsForRecentModernDate()
        {
            // Global population is still growing in 2025, so births should exceed deaths.
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Y2K", "2000-01-01", now);

            Assert.True(result.BornBetweenDates > result.DiedBetweenDates,
                "In a growing population era births should exceed deaths.");
        }

        // ------------------------------------------------------------------ //
        // Sub-statistic ratio tests                                            //
        // ------------------------------------------------------------------ //

        [Fact]
        public void TwinsBorn_IsApproximately2Point4PercentOfBirths()
        {
            var baseDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-01-01", now);

            double expectedTwins = result.BornBetweenDates * 0.024;
            Assert.Equal(expectedTwins, result.TwinsBorn, precision: 0);
        }

        [Fact]
        public void HeartDeaths_IsApproximately27PercentOfDeaths()
        {
            var baseDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-01-01", now);

            double expectedHeart = result.DiedBetweenDates * 0.27;
            Assert.Equal(expectedHeart, result.HeartDeaths, precision: 0);
        }

        [Fact]
        public void CancerDeaths_IsApproximately18PercentOfDeaths()
        {
            var baseDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-01-01", now);

            double expectedCancer = result.DiedBetweenDates * 0.18;
            Assert.Equal(expectedCancer, result.CancerDeaths, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // Zero elapsed time                                                    //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ZeroElapsedTime_ReturnsBirthsAndDeathsOfZero()
        {
            var baseDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate;

            var result = _svc.CalculateVibrantHumanity(baseDate, "Today", "2025-01-01", now);

            Assert.Equal(0, result.BornBetweenDates, precision: 0);
            Assert.Equal(0, result.DiedBetweenDates, precision: 0);
        }

        [Fact]
        public void ZeroElapsedTime_ReturnsNonNullNonEmptyTexts()
        {
            var baseDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate;

            var result = _svc.CalculateVibrantHumanity(baseDate, "Today", "2025-01-01", now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        // ------------------------------------------------------------------ //
        // Very old base date                                                   //
        // ------------------------------------------------------------------ //

        [Fact]
        public void VeryOldBaseDate_DoesNotThrow()
        {
            var baseDate = new DateTime(1900, 1, 2, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var exception = Record.Exception(() =>
                _svc.CalculateVibrantHumanity(baseDate, "Ancestor", "1900-01-02", now));

            Assert.Null(exception);
        }

        [Fact]
        public void VeryOldBaseDate_YieldsLargerCountsThanRecentBase()
        {
            var oldBase  = new DateTime(1925, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var recentBase = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var oldResult    = _svc.CalculateVibrantHumanity(oldBase,    "Old",    "1925-01-01", now);
            var recentResult = _svc.CalculateVibrantHumanity(recentBase, "Recent", "2000-01-01", now);

            Assert.True(oldResult.BornBetweenDates > recentResult.BornBetweenDates,
                "A 100-year window should produce more births than a 25-year window.");
        }

        // ------------------------------------------------------------------ //
        // N0 formatting in display strings                                     //
        // ------------------------------------------------------------------ //

        [Fact]
        public void BriefText_ContainsHtmlBoldTags()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-06-15", now);

            Assert.Contains("<b>", result.BriefText);
        }

        [Fact]
        public void FullText_DoesNotContainUnreplacedTokens()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-06-15", now);

            Assert.DoesNotContain("{births}",  result.FullText);
            Assert.DoesNotContain("{deaths}",  result.FullText);
            Assert.DoesNotContain("{twins}",   result.FullText);
            Assert.DoesNotContain("{heart}",   result.FullText);
            Assert.DoesNotContain("{cancer}",  result.FullText);
            Assert.DoesNotContain("{baseDate:d}", result.FullText);
            Assert.DoesNotContain("{baseDateName}", result.FullText);
        }

        // ------------------------------------------------------------------ //
        // Consistency with HumanBirthRank and GlobalCrowd helpers             //
        // ------------------------------------------------------------------ //

        [Fact]
        public void BirthRankHelpers_ConsistentWithVibrantHumanityBirths()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-06-15", now);

            double expectedBirths = Math.Max(0,
                _svc.HumanBirthRankbyDate(now) - _svc.HumanBirthRankbyDate(baseDate.ToUniversalTime()));

            Assert.Equal(expectedBirths, result.BornBetweenDates, precision: 0);
        }

        [Fact]
        public void PopulationHelper_ConsistentWithVibrantHumanityDeaths()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantHumanity(baseDate, "Birthday", "1990-06-15", now);

            double expectedDeaths = Math.Max(0,
                _svc.TotalDeathsByDate(now) - _svc.TotalDeathsByDate(baseDate.ToUniversalTime()));

            Assert.Equal(expectedDeaths, result.DiedBetweenDates, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // Proportional growth test                                             //
        // ------------------------------------------------------------------ //

        [Fact]
        public void DoubledElapsedTime_RoughlyDoublesBirths()
        {
            var baseDate  = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now10yr   = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now20yr   = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result10 = _svc.CalculateVibrantHumanity(baseDate, "Y2K", "2000-01-01", now10yr);
            var result20 = _svc.CalculateVibrantHumanity(baseDate, "Y2K", "2000-01-01", now20yr);

            // Within a single piecewise segment (post-2000) births scale linearly.
            // Ratio should be close to 2.0 (within 5% tolerance).
            double ratio = result20.BornBetweenDates / result10.BornBetweenDates;
            Assert.InRange(ratio, 1.9, 2.1);
        }
    }
}
