using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateGlobalCrowd"/>.
    ///
    /// Methodology: piecewise linear population model with three segments:
    /// - Before 1900: base 978,000,000 at 1800-01-01 UTC, rate 18,398/day.
    /// - 1900 to 1950: base 1,650,000,000 at 1900-01-01 UTC, rate 47,919/day.
    /// - 1950 onward: base 2,525,149,000 at 1950-01-01 UTC, rate 203,206/day (~2.35/s).
    /// Both base-date and current populations are formatted with N0 (thousand separators).
    /// </summary>
    public class CalculateGlobalCrowdTests
    {
        private readonly CalculationService _svc;

        public CalculateGlobalCrowdTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        // ------------------------------------------------------------------ //
        // Happy path - modern dates (post-1950 segment)                       //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_ResultIsNotNullAndTextsAreNonEmpty()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1990-06-15", now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void HappyPath_CurrentPopulationIsGreaterThanBasePopulation()
        {
            var baseDate = new DateTime(1965, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1965-07-24", now);

            Assert.True(result.CurrentPopulation > result.BasePopulation,
                "Current population should be larger than base-date population for a past base date.");
        }

        [Fact]
        public void HappyPath_BriefTextContainsPopulationFigures()
        {
            var baseDate = new DateTime(1965, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1965-07-24", now);

            // The brief text uses N0 formatting, so it should contain a comma-separated number
            // e.g. "3,326,140,000" for 1965-era population
            Assert.Contains(",", result.BriefText);
        }

        [Fact]
        public void HappyPath_FullTextContainsBothPopulationFigures()
        {
            var baseDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1990-01-01", now);

            // Both tokens replaced; full text is longer than brief
            Assert.True(result.FullText.Length > result.BriefText.Length);
        }

        [Fact]
        public void FullText_DoesNotContainRawDateToken()
        {
            // Verifies the {baseDate:d} token was replaced and does not appear verbatim.
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1990-06-15", now);

            Assert.DoesNotContain("{baseDate:d}", result.FullText);
        }

        // ------------------------------------------------------------------ //
        // Post-1950 formula accuracy                                          //
        // ------------------------------------------------------------------ //

        [Fact]
        public void Post1950_BasePopulationMatchesFormula()
        {
            // Exactly 365 days after epoch1950 (1951-01-01 UTC)
            var baseDate = new DateTime(1951, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(1);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1951-01-01", now);

            // Expected: 2,525,149,000 + 365 * 203,206 = 2,599,319,190
            double expectedBase = 2525149000.0 + 365.0 * 203206.0;
            Assert.Equal(expectedBase, result.BasePopulation, precision: 0);
        }

        [Fact]
        public void Post1950_AtEpochAnchor_MatchesAnchorValue()
        {
            var epoch1950 = new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now       = epoch1950.AddSeconds(1);

            var result = _svc.CalculateGlobalCrowd(epoch1950, "1950-01-01", now);

            Assert.Equal(2525149000.0, result.BasePopulation, precision: 0);
        }

        [Fact]
        public void Post1950_LiveUpdate_PopulationIncreasesPerSecond()
        {
            var baseDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now1     = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var now2     = now1.AddSeconds(1);

            var result1 = _svc.CalculateGlobalCrowd(baseDate, "1990-01-01", now1);
            var result2 = _svc.CalculateGlobalCrowd(baseDate, "1990-01-01", now2);

            Assert.True(result2.CurrentPopulation > result1.CurrentPopulation,
                "Population should increase each second (post-1950 segment rate ~2.35/s).");
        }

        // ------------------------------------------------------------------ //
        // Pre-1900 segment                                                    //
        // ------------------------------------------------------------------ //

        [Fact]
        public void Pre1900_BasePopulationMatchesFormula()
        {
            // Exactly 365 days after epoch1800 (1801-01-01 UTC)
            var baseDate = new DateTime(1801, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1801-01-01", now);

            // Expected base: 978,000,000 + 365 * 18,398 = 984,715,270
            double expectedBase = 978000000.0 + 365.0 * 18398.0;
            Assert.Equal(expectedBase, result.BasePopulation, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // 1900-1950 segment                                                   //
        // ------------------------------------------------------------------ //

        [Fact]
        public void Between1900And1950_BasePopulationMatchesFormula()
        {
            // Exactly 365 days after epoch1900 (1901-01-01 UTC)
            var baseDate = new DateTime(1901, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1901-01-01", now);

            // Expected base: 1,650,000,000 + 365 * 47,919 = 1,667,490,435
            double expectedBase = 1650000000.0 + 365.0 * 47919.0;
            Assert.Equal(expectedBase, result.BasePopulation, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // Zero elapsed time                                                   //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ZeroElapsedTime_BaseAndCurrentPopulationsAreEqual()
        {
            var moment = new DateTime(2000, 6, 15, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(moment, "2000-06-15", moment);

            // When baseDate == now, both populations should be equal
            Assert.Equal(result.BasePopulation, result.CurrentPopulation, precision: 0);
        }

        [Fact]
        public void ZeroElapsedTime_BriefTextIsNonEmpty()
        {
            var moment = new DateTime(2000, 6, 15, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(moment, "2000-06-15", moment);

            Assert.NotEmpty(result.BriefText);
        }

        // ------------------------------------------------------------------ //
        // Very old date - no exception                                        //
        // ------------------------------------------------------------------ //

        [Fact]
        public void VeryOldDate_DoesNotThrow()
        {
            var baseDate = new DateTime(1850, 3, 21, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var exception = Record.Exception(() => _svc.CalculateGlobalCrowd(baseDate, "1850-03-21", now));

            Assert.Null(exception);
        }

        [Fact]
        public void VeryOldDate_PopulationIsPositive()
        {
            var baseDate = new DateTime(1820, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1820-01-01", now);

            Assert.True(result.BasePopulation > 0);
            Assert.True(result.CurrentPopulation > 0);
        }

        // ------------------------------------------------------------------ //
        // Typed result field round-trip                                       //
        // ------------------------------------------------------------------ //

        [Fact]
        public void TypedResult_BaseAndCurrentPopulationFieldsArePopulated()
        {
            var baseDate = new DateTime(1975, 4, 10, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 4, 10, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1975-04-10", now);

            Assert.True(result.BasePopulation > 0, "BasePopulation must be populated.");
            Assert.True(result.CurrentPopulation > 0, "CurrentPopulation must be populated.");
        }

        // ------------------------------------------------------------------ //
        // N0 formatting - no decimal point in brief text                      //
        // ------------------------------------------------------------------ //

        [Fact]
        public void BriefText_DoesNotContainDecimalPoint()
        {
            var baseDate = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateGlobalCrowd(baseDate, "1990-01-01", now);

            // N0 formatting must not produce a decimal separator
            Assert.DoesNotContain(".", result.BriefText);
        }
    }
}
