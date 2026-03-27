using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateLifeLog"/>.
    ///
    /// Methodology: each activity total = dailyAverage * totalDays.
    /// Average daily hours: Sleeping 8.8, Leisure 5.2, Working 3.6,
    /// Household Chores 1.8, Eating 1.2, Commuting 1.1, Personal Care 0.8.
    /// Brief view shows 2 randomly selected activities formatted as N0 hours.
    /// Full view lists all 7 activities with time converted to readable units.
    /// </summary>
    public class CalculateLifeLogTests
    {
        private readonly CalculationService _svc;

        public CalculateLifeLogTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        // ------------------------------------------------------------------ //
        // Happy path                                                          //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_ResultIsNotNullAndTextsAreNonEmpty()
        {
            var baseDate = new DateTime(1990, 6, 15);
            var now      = new DateTime(2025, 1, 1);

            var result = _svc.CalculateLifeLog(baseDate, "My Birthday", "1990-06-15", now: now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void HappyPath_TotalDaysMatchesElapsed()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = new DateTime(2001, 1, 1);   // exactly 366 days (leap year 2000)

            var result = _svc.CalculateLifeLog(baseDate, "Test", "2000-01-01", now: now);

            Assert.Equal(366.0, result.TotalDays, precision: 0);
        }

        [Fact]
        public void HappyPath_ActivityHoursContainsSevenEntries()
        {
            var baseDate = new DateTime(1980, 3, 1);
            var now      = new DateTime(2025, 3, 1);

            var result = _svc.CalculateLifeLog(baseDate, "Test", "1980-03-01", now: now);

            Assert.Equal(7, result.ActivityHours.Count);
        }

        // ------------------------------------------------------------------ //
        // Sleeping formula (8.8 h/day)                                        //
        // ------------------------------------------------------------------ //

        [Fact]
        public void SleepingHours_MatchFormula()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = new DateTime(2001, 1, 1);   // 366 days
            double expectedHours = 8.8 * 366.0;

            var result = _svc.CalculateLifeLog(baseDate, "Test", "2000-01-01", now: now);

            Assert.True(result.ActivityHours.ContainsKey("Sleeping"),
                "ActivityHours should contain the 'Sleeping' key (English locale).");
            Assert.Equal(expectedHours, result.ActivityHours["Sleeping"], precision: 1);
        }

        // ------------------------------------------------------------------ //
        // Brief text                                                          //
        // ------------------------------------------------------------------ //

        [Fact]
        public void BriefText_ContainsTwoActivityNames()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2025, 1, 1);

            var result = _svc.CalculateLifeLog(baseDate, "Test", "1990-01-01", now: now);

            // Brief text format: "{activity_1}: {hours_1} hrs, {activity_2}: {hours_2} hrs"
            // Activity names come from AppResources; at minimum two colon-separated segments
            int colonCount = result.BriefText.Count(c => c == ':');
            Assert.True(colonCount >= 2, $"Expected at least 2 colons in BriefText, got: '{result.BriefText}'");
        }

        [Fact]
        public void BriefText_DoesNotContainRawTokens()
        {
            var baseDate = new DateTime(1985, 7, 4);
            var now      = new DateTime(2025, 7, 4);

            var result = _svc.CalculateLifeLog(baseDate, "Test", "1985-07-04", now: now);

            Assert.DoesNotContain("{activity_1}", result.BriefText);
            Assert.DoesNotContain("{activity_2}", result.BriefText);
            Assert.DoesNotContain("{hours_1}",    result.BriefText);
            Assert.DoesNotContain("{hours_2}",    result.BriefText);
        }

        [Fact]
        public void BriefText_Activity1And2MatchResultFields()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2025, 1, 1);
            var rand     = new Random(42);   // fixed seed for determinism

            var result = _svc.CalculateLifeLog(baseDate, "Test", "1990-01-01", rand: rand, now: now);

            Assert.NotEmpty(result.Activity1Name);
            Assert.NotEmpty(result.Activity2Name);
            Assert.NotEqual(result.Activity1Name, result.Activity2Name);
            Assert.True(result.Activity1Hours > 0);
            Assert.True(result.Activity2Hours > 0);
        }

        // ------------------------------------------------------------------ //
        // Full text                                                           //
        // ------------------------------------------------------------------ //

        [Fact]
        public void FullText_DoesNotContainRawDateToken()
        {
            var baseDate = new DateTime(1990, 6, 15);
            var now      = new DateTime(2025, 1, 1);

            var result = _svc.CalculateLifeLog(baseDate, "My Birthday", "1990-06-15", now: now);

            Assert.DoesNotContain("{baseDate:d}",          result.FullText);
            Assert.DoesNotContain("{baseDateName}",        result.FullText);
            Assert.DoesNotContain("{all_activities_list}", result.FullText);
        }

        [Fact]
        public void FullText_ContainsBaseDateName()
        {
            var baseDate = new DateTime(1990, 6, 15);
            var now      = new DateTime(2025, 1, 1);

            var result = _svc.CalculateLifeLog(baseDate, "My Birthday", "1990-06-15", now: now);

            Assert.Contains("My Birthday", result.FullText);
        }

        [Fact]
        public void FullText_ContainsAllSevenActivities()
        {
            var baseDate = new DateTime(1980, 1, 1);
            var now      = new DateTime(2025, 1, 1);

            var result = _svc.CalculateLifeLog(baseDate, "Test", "1980-01-01", now: now);

            Assert.Contains("Sleeping",        result.FullText);
            Assert.Contains("Working",         result.FullText);
            Assert.Contains("Commuting",       result.FullText);
            Assert.Contains("Personal Care",   result.FullText);
            // Activity names with '&' must appear HTML-escaped so the MAUI HTML
            // renderer does not break the surrounding &bull; and <br> tags.
            Assert.Contains("Leisure &amp; Screen Time", result.FullText);
            Assert.Contains("Eating &amp; Drinking",     result.FullText);
        }

        [Fact]
        public void FullText_ActivityNamesWithAmpersandAreHtmlEscaped()
        {
            // Regression test: bare '&' in English activity names ("Leisure & Screen Time",
            // "Eating & Drinking") must be escaped to '&amp;' in the HTML full-text so that
            // the MAUI Label TextType="Html" renderer correctly displays &bull; and <br> tags.
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2025, 1, 1);

            var result = _svc.CalculateLifeLog(baseDate, "Test", "1990-01-01", now: now);

            // Must NOT contain a bare '&' that is not part of an HTML entity
            // (i.e. '&' not followed by 'amp;', 'bull;', 'lt;', 'gt;', 'nbsp;', '#')
            var bareAmpersand = System.Text.RegularExpressions.Regex.IsMatch(
                result.FullText, @"&(?!amp;|bull;|lt;|gt;|nbsp;|#)");
            Assert.False(bareAmpersand,
                $"FullText contains a bare '&' that is not a valid HTML entity: {result.FullText}");
        }

        // ------------------------------------------------------------------ //
        // Zero elapsed time                                                   //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ZeroElapsedTime_ReturnsNonNullResult()
        {
            var baseDate = new DateTime(2025, 1, 1);
            var now      = new DateTime(2025, 1, 1);

            var result = _svc.CalculateLifeLog(baseDate, "Test", "2025-01-01", now: now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
            Assert.Equal(0.0, result.TotalDays, precision: 1);
        }

        // ------------------------------------------------------------------ //
        // Very old base date                                                  //
        // ------------------------------------------------------------------ //

        [Fact]
        public void VeryOldBaseDate_DoesNotThrow()
        {
            var baseDate = new DateTime(1800, 1, 1);
            var now      = new DateTime(2025, 1, 1);

            var result = _svc.CalculateLifeLog(baseDate, "Old Date", "1800-01-01", now: now);

            Assert.NotNull(result);
            Assert.True(result.TotalDays > 0);
        }

        // ------------------------------------------------------------------ //
        // Proportional growth                                                 //
        // ------------------------------------------------------------------ //

        [Fact]
        public void DoubleDays_DoublesSleepingHours()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now1     = new DateTime(2010, 1, 1);
            var now2     = new DateTime(2020, 1, 1);   // roughly double the days from 2000

            var result1 = _svc.CalculateLifeLog(baseDate, "T", "2000-01-01", now: now1);
            var result2 = _svc.CalculateLifeLog(baseDate, "T", "2000-01-01", now: now2);

            double ratio = result2.ActivityHours["Sleeping"] / result1.ActivityHours["Sleeping"];
            // ~2x (allow 5% tolerance for leap years)
            Assert.True(ratio > 1.9 && ratio < 2.1,
                $"Expected sleeping hours to roughly double; ratio was {ratio:F3}");
        }

        // ------------------------------------------------------------------ //
        // Typed result field round-trip                                       //
        // ------------------------------------------------------------------ //

        [Fact]
        public void TypedFields_ActivityHoursStoredCorrectly()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = new DateTime(2001, 1, 1);  // 366 days

            var result = _svc.CalculateLifeLog(baseDate, "T", "2000-01-01", now: now);

            // Sleeping = 8.8 * 366 = 3220.8 h
            Assert.Equal(3220.8, result.ActivityHours["Sleeping"], precision: 0);
            // Working = 3.6 * 366 = 1317.6 h
            Assert.Equal(1317.6, result.ActivityHours["Working"], precision: 0);
        }

        [Fact]
        public void Activity1And2Hours_MatchActivityHoursDictionary()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2025, 1, 1);
            var rand     = new Random(99);

            var result = _svc.CalculateLifeLog(baseDate, "T", "1990-01-01", rand: rand, now: now);

            Assert.True(result.ActivityHours.ContainsKey(result.Activity1Name));
            Assert.True(result.ActivityHours.ContainsKey(result.Activity2Name));
            Assert.Equal(result.ActivityHours[result.Activity1Name], result.Activity1Hours, precision: 1);
            Assert.Equal(result.ActivityHours[result.Activity2Name], result.Activity2Hours, precision: 1);
        }
    }
}
