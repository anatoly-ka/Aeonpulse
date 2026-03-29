using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateVibrantNature"/>.
    ///
    /// Methodology: 3-epoch piecewise linear daily-rate model anchored to 1900-01-01 UTC.
    /// Discovery rates: 1900-1950 ~27.4/day; 1950-2000 ~41.1/day; 2000-present ~49.3/day.
    /// Extinction rates: 1900-1950 ~10/day; 1950-2000 ~50/day; 2000-present ~150/day.
    /// Taxonomic proportions: insects 55% of discoveries / 60% of extinctions,
    /// plants 15% of discoveries, vertebrates 2% of both.
    /// </summary>
    public class CalculateVibrantNatureTests
    {
        private readonly CalculationService _svc;

        public CalculateVibrantNatureTests()
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

            var result = _svc.CalculateVibrantNature(baseDate, now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void HappyPath_DiscoveredSpeciesGreaterThanZeroForPastBaseDate()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            Assert.True(result.DiscoveredSince > 0,
                "Discovered species count since 1990 should be positive.");
        }

        [Fact]
        public void HappyPath_ExtinctSpeciesGreaterThanZeroForPastBaseDate()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            Assert.True(result.ExtinctSince > 0,
                "Extinct species count since 1990 should be positive.");
        }

        // ------------------------------------------------------------------ //
        // N0 formatting in BriefText                                          //
        // ------------------------------------------------------------------ //

        [Fact]
        public void BriefText_ContainsFormattedDiscoveredCount()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            // Exact N0 value of discovered species (49.3/day * 3652.5 days ~= 180,068)
            string expectedDiscovered = ((long)result.DiscoveredSince).ToString("N0");
            Assert.Contains(expectedDiscovered, result.BriefText);
        }

        [Fact]
        public void BriefText_ContainsFormattedExtinctCount()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            string expectedExtinct = ((long)result.ExtinctSince).ToString("N0");
            Assert.Contains(expectedExtinct, result.BriefText);
        }

        // ------------------------------------------------------------------ //
        // Zero elapsed time                                                   //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ZeroElapsedTime_DiscoveredIsZero()
        {
            var baseDate = new DateTime(2020, 3, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate;

            var result = _svc.CalculateVibrantNature(baseDate, now);

            Assert.Equal(0.0, result.DiscoveredSince, precision: 0);
        }

        [Fact]
        public void ZeroElapsedTime_ExtinctIsZero()
        {
            var baseDate = new DateTime(2020, 3, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate;

            var result = _svc.CalculateVibrantNature(baseDate, now);

            Assert.Equal(0.0, result.ExtinctSince, precision: 0);
        }

        [Fact]
        public void ZeroElapsedTime_TextsAreNonEmpty()
        {
            var baseDate = new DateTime(2020, 3, 15, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, baseDate);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        // ------------------------------------------------------------------ //
        // Very old base date                                                  //
        // ------------------------------------------------------------------ //

        [Fact]
        public void VeryOldBaseDate_DoesNotThrow()
        {
            var baseDate = new DateTime(1901, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Exception? caught = null;
            try { _svc.CalculateVibrantNature(baseDate, now); }
            catch (Exception ex) { caught = ex; }

            Assert.Null(caught);
        }

        [Fact]
        public void VeryOldBaseDate_DiscoveredIsPositive()
        {
            var baseDate = new DateTime(1901, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            Assert.True(result.DiscoveredSince > 0);
        }

        // ------------------------------------------------------------------ //
        // Piecewise epoch boundaries                                          //
        // ------------------------------------------------------------------ //

        [Fact]
        public void PreEpoch1950_DiscoveryRateIsCorrect()
        {
            // 365 days entirely within the 1900-1950 epoch
            var baseDate = new DateTime(1910, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(1911, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = 365.0 * 27.4;
            Assert.Equal(expected, result.DiscoveredSince, precision: 0);
        }

        [Fact]
        public void PreEpoch1950_ExtinctionRateIsCorrect()
        {
            // 365 days entirely within the 1900-1950 epoch (use a non-leap year)
            var baseDate = new DateTime(1921, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(1922, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = 365.0 * 10.0;
            Assert.Equal(expected, result.ExtinctSince, precision: 0);
        }

        [Fact]
        public void PostEpoch2000_DiscoveryRateIsCorrect()
        {
            // 365 days entirely within the post-2000 epoch
            var baseDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = 365.0 * 49.3;
            Assert.Equal(expected, result.DiscoveredSince, precision: 0);
        }

        [Fact]
        public void PostEpoch2000_ExtinctionRateIsCorrect()
        {
            // 365 days entirely within the post-2000 epoch
            var baseDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = 365.0 * 150.0;
            Assert.Equal(expected, result.ExtinctSince, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // Taxonomic sub-statistics                                            //
        // ------------------------------------------------------------------ //

        [Fact]
        public void InsectsDiscovered_Is55PercentOfTotal()
        {
            var baseDate = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = result.DiscoveredSince * 0.55;
            Assert.Equal(expected, result.InsectsDiscovered, precision: 5);
        }

        [Fact]
        public void PlantsDiscovered_Is15PercentOfTotal()
        {
            var baseDate = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = result.DiscoveredSince * 0.15;
            Assert.Equal(expected, result.PlantsDiscovered, precision: 5);
        }

        [Fact]
        public void VertebratesDiscovered_Is2PercentOfTotal()
        {
            var baseDate = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = result.DiscoveredSince * 0.02;
            Assert.Equal(expected, result.VertebratesDiscovered, precision: 5);
        }

        [Fact]
        public void InsectsExtinct_Is60PercentOfTotal()
        {
            var baseDate = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = result.ExtinctSince * 0.60;
            Assert.Equal(expected, result.InsectsExtinct, precision: 5);
        }

        [Fact]
        public void VertebratesExtinct_Is2PercentOfTotal()
        {
            var baseDate = new DateTime(2005, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            double expected = result.ExtinctSince * 0.02;
            Assert.Equal(expected, result.VertebratesExtinct, precision: 5);
        }

        // ------------------------------------------------------------------ //
        // Proportional growth                                                 //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ProportionalGrowth_DoubleElapsedTimeDoublesDiscoveries()
        {
            var baseDate   = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now1Year   = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now2Year   = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result1 = _svc.CalculateVibrantNature(baseDate, now1Year);
            var result2 = _svc.CalculateVibrantNature(baseDate, now2Year);

            // In the post-2000 epoch, rate is constant so 2x time = 2x discoveries
            Assert.InRange(result2.DiscoveredSince / result1.DiscoveredSince, 1.99, 2.01);
        }

        [Fact]
        public void ProportionalGrowth_DoubleElapsedTimeDoublesExtinctions()
        {
            var baseDate   = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now1Year   = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now2Year   = new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result1 = _svc.CalculateVibrantNature(baseDate, now1Year);
            var result2 = _svc.CalculateVibrantNature(baseDate, now2Year);

            Assert.InRange(result2.ExtinctSince / result1.ExtinctSince, 1.99, 2.01);
        }

        // ------------------------------------------------------------------ //
        // Typed result field round-trip                                       //
        // ------------------------------------------------------------------ //

        [Fact]
        public void TypedResult_AllRawFieldsArePopulated()
        {
            var baseDate = new DateTime(1985, 3, 10, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            Assert.True(result.DiscoveredSince > 0,        "DiscoveredSince");
            Assert.True(result.ExtinctSince > 0,           "ExtinctSince");
            Assert.True(result.InsectsDiscovered > 0,      "InsectsDiscovered");
            Assert.True(result.PlantsDiscovered > 0,       "PlantsDiscovered");
            Assert.True(result.VertebratesDiscovered > 0,  "VertebratesDiscovered");
            Assert.True(result.InsectsExtinct > 0,         "InsectsExtinct");
            Assert.True(result.VertebratesExtinct > 0,     "VertebratesExtinct");
        }

        [Fact]
        public void FullText_ContainsInsectsDiscoveredToken()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantNature(baseDate, now);

            string expectedInsects = ((long)result.InsectsDiscovered).ToString("N0");
            Assert.Contains(expectedInsects, result.FullText);
        }
    }
}
