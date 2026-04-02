using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateTimeJubilees"/>.
    ///
    /// Verifies that the flat-list chronological algorithm selects milestones correctly
    /// across all families, that the 50-year classical jubilee is correctly preferred
    /// over day/hour milestones that fall nearby, and that the ProgressFraction is
    /// always clamped to [0.05, 0.95].
    /// </summary>
    public class CalculateTimeJubileesTests
    {
        private readonly CalculationService _svc;

        public CalculateTimeJubileesTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void ReturnsNonNullResult_ForTypicalBirthday()
        {
            var baseDate = new DateTime(1965, 7, 24);
            var now      = new DateTime(2026, 3, 21);

            var result = _svc.CalculateTimeJubilees(baseDate, "Birthday", "1965-07-24", now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void BriefTextContainsNumericValue_AndUnit()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 3, 21);

            var result = _svc.CalculateTimeJubilees(baseDate, "T", "1990-01-01", now);

            Assert.Matches(@"\d", result.BriefText);
        }

        [Fact]
        public void VeryOldDate_DoesNotThrow()
        {
            // Dates far in the past produce very large hour counts that can overflow
            // DateTime.AddHours. The method guards these with try/catch.
            var baseDate = new DateTime(1800, 1, 1);
            var now      = new DateTime(2026, 3, 21);

            var ex = Record.Exception(() =>
                _svc.CalculateTimeJubilees(baseDate, "T", "1800-01-01", now));

            Assert.Null(ex);
        }

        [Fact]
        public void FullTextContainsBaseDateName()
        {
            var baseDate = new DateTime(1965, 7, 24);
            var now      = new DateTime(2026, 3, 21);

            var result = _svc.CalculateTimeJubilees(baseDate, "MyBirthday", "1965-07-24", now);

            Assert.Contains("MyBirthday", result.FullText);
        }

        /// <summary>
        /// Verifies that for a base date of 1975-03-30 with now set 15 days before the
        /// 50-year mark (2025-03-15), the next jubilee is identified as "50 years" and
        /// not overridden by a nearby day or hour milestone that would fall sooner.
        /// This is the canonical regression test for the flat-list algorithm.
        /// </summary>
        [Fact]
        public void FiftyYearJubilee_IsSelectedAsNext_For_BaseDate_1975_03_30()
        {
            var baseDate = new DateTime(1975, 3, 30);
            var now      = new DateTime(2025, 3, 15); // 15 days before the 50-year mark

            var result = _svc.CalculateTimeJubilees(baseDate, "Birthday", "1975-03-30", now);

            // The next jubilee must be the 50-year classical milestone.
            Assert.Contains("50", result.NextJubileeName);
            Assert.Contains("years", result.NextJubileeName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifies that ProgressFraction is always clamped between 0.05 and 0.95
        /// so the Today dot never visually overlaps either endpoint dot.
        /// </summary>
        [Fact]
        public void ProgressFraction_IsAlwaysBetween_0_05_And_0_95()
        {
            // Use a date where now lands exactly on a jubilee (daysSinceLast = 0).
            var baseDate = new DateTime(2000, 1, 1);
            var now      = new DateTime(2010, 1, 1); // exactly 10 years - a classical jubilee

            var result = _svc.CalculateTimeJubilees(baseDate, "T", "2000-01-01", now);

            Assert.InRange(result.ProgressFraction, 0.05, 0.95);
        }

        /// <summary>
        /// Verifies that LastJubileeDate is strictly before now and JubileeDate (Next)
        /// is strictly after now when now falls between two milestones.
        /// </summary>
        [Fact]
        public void LastAndNextJubilee_BracketNow_For_BaseDate_1975_03_30_AfterFiftyYears()
        {
            var baseDate = new DateTime(1975, 3, 30);
            var now      = new DateTime(2025, 4, 15); // 16 days after the 50-year mark

            var result = _svc.CalculateTimeJubilees(baseDate, "Birthday", "1975-03-30", now);

            // LastJubileeDate must be strictly before now.
            Assert.True(result.LastJubileeDate < now,
                $"Expected LastJubileeDate {result.LastJubileeDate:d} < now {now:d}");

            // JubileeDate (next) must be strictly after now.
            Assert.True(result.JubileeDate > now,
                $"Expected JubileeDate {result.JubileeDate:d} > now {now:d}");

            // DaysSinceLast and DaysTillNext must both be positive.
            Assert.True(result.DaysSinceLast > 0,
                $"Expected DaysSinceLast > 0, got {result.DaysSinceLast}");
            Assert.True(result.DaysTillNext > 0,
                $"Expected DaysTillNext > 0, got {result.DaysTillNext}");
        }

        /// <summary>
        /// Verifies that immediately before the 50-year mark (1 day before),
        /// the next jubilee is still "50 years" and DaysTillNext is 1.
        /// </summary>
        [Fact]
        public void FiftyYearJubilee_OneDayBefore_DaysTillNextIsOne()
        {
            var baseDate = new DateTime(1975, 3, 30);
            var now      = new DateTime(2025, 3, 29); // 1 day before 50-year mark

            var result = _svc.CalculateTimeJubilees(baseDate, "Birthday", "1975-03-30", now);

            Assert.Contains("50", result.NextJubileeName);
            Assert.Equal(1, result.DaysTillNext);
        }
    }
}
