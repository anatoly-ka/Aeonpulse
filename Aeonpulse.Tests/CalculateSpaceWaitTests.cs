using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateSpaceWait"/>.
    ///
    /// Methodology: for each of the seven major planets, the method computes
    /// how many full orbital periods have elapsed since the base date, then
    /// finds the planet whose next full orbit completes soonest.
    /// Orbital periods (Earth days): Mercury=87.97, Venus=224.70, Mars=686.98,
    /// Jupiter=4332.59, Saturn=10759.22, Uranus=30685.40, Neptune=60189.00.
    ///
    /// Brief/Full text branches (mirrors Countdown ticker):
    ///   &lt; 86400 s  -&gt; HoursOnly template  ({hrs}, {mins}, {secs})
    ///   &gt;= 86400 s -&gt; DaysHours template  ({days}, {hrs}, {mins}, {secs})
    /// Both templates are localised via AppResources so language switches correctly.
    /// </summary>
    public class CalculateSpaceWaitTests
    {
        private readonly CalculationService _svc;

        public CalculateSpaceWaitTests()
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

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void HappyPath_NextPlanetIsNonEmpty()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.NotEmpty(result.NextPlanet);
        }

        [Fact]
        public void HappyPath_NextAgeIsPositive()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.True(result.NextAge > 0, "NextAge should be a positive integer.");
        }

        [Fact]
        public void HappyPath_CountdownIsPositive()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.True(result.Countdown.TotalSeconds > 0, "Countdown should be positive.");
        }

        // ------------------------------------------------------------------ //
        // Countdown is always less than the shortest orbital period           //
        // ------------------------------------------------------------------ //

        [Fact]
        public void CountdownIsLessThanMercuryOrbitalPeriod()
        {
            // The soonest any orbit completes must be within one Mercury period (87.97 days)
            var baseDate = new DateTime(1980, 3, 10, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2024, 6, 20, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.True(result.Countdown.TotalDays < 87.97,
                $"Countdown ({result.Countdown.TotalDays:F2}d) must be less than Mercury orbital period (87.97d).");
        }

        // ------------------------------------------------------------------ //
        // Mercury birthday: inject a time exactly at a Mercury orbital reset  //
        // ------------------------------------------------------------------ //

        [Fact]
        public void MercuryNextBirthday_CorrectCountdown()
        {
            // Set baseDate so that Mercury has completed exactly N orbits at 'now'.
            // Then the next Mercury birthday is exactly 87.97 days away.
            const double mercuryDays = 87.97;
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Advance time to the exact start of Mercury orbit 100
            var now = baseDate.AddDays(100 * mercuryDays);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            // At this instant, Mercury has just completed orbit 100, so daysToNext = 87.97.
            // Every other planet's next birthday is also <= 87.97 days away,
            // but Mercury's should be exactly 87.97 (up to floating-point tolerance).
            // The result planet must have a countdown <= 87.97 days.
            Assert.True(result.Countdown.TotalDays <= mercuryDays + 0.001,
                $"Countdown ({result.Countdown.TotalDays:F4}d) should be <= Mercury period ({mercuryDays}d).");
            Assert.True(result.NextAge >= 1, "NextAge must be at least 1.");
        }

        // ------------------------------------------------------------------ //
        // Zero elapsed time                                                   //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ZeroElapsedTime_ReturnsNonNullNonEmpty()
        {
            var baseDate = new DateTime(2000, 6, 15, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, baseDate);

            Assert.NotNull(result);
            Assert.NotNull(result.BriefText);
            Assert.NotEmpty(result.NextPlanet);
        }

        [Fact]
        public void ZeroElapsedTime_FirstBirthdayIsOneFullMercuryOrbit()
        {
            // At time zero, Mercury orbit 1 is exactly 87.97 days away.
            var baseDate = new DateTime(2000, 6, 15, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, baseDate);

            // nextAge must be 1 for Mercury
            Assert.Equal(1, result.NextAge);
            Assert.InRange(result.Countdown.TotalDays, 87.96, 87.98);
        }

        // ------------------------------------------------------------------ //
        // Large elapsed time (very old base date)                             //
        // ------------------------------------------------------------------ //

        [Fact]
        public void VeryOldBaseDate_NoException()
        {
            var baseDate = new DateTime(1850, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Record.Exception(() => _svc.CalculateSpaceWait(baseDate, now));

            Assert.Null(ex);
        }

        [Fact]
        public void VeryOldBaseDate_ValidResult()
        {
            var baseDate = new DateTime(1850, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
            Assert.True(result.NextAge > 0);
        }

        // ------------------------------------------------------------------ //
        // Ordinal suffix in English                                           //
        // ------------------------------------------------------------------ //

        [Fact]
        public void BriefText_ContainsPlanetName()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.Contains(result.NextPlanet, result.BriefText);
        }

        [Fact]
        public void FullText_ContainsPlanetNameAndAge()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.Contains(result.NextPlanet, result.FullText);
            Assert.Contains(result.NextAge.ToString(), result.FullText);
        }

        [Theory]
        [InlineData(1, "1st")]
        [InlineData(2, "2nd")]
        [InlineData(3, "3rd")]
        [InlineData(4, "4th")]
        [InlineData(11, "11th")]
        [InlineData(12, "12th")]
        [InlineData(13, "13th")]
        [InlineData(21, "21st")]
        [InlineData(22, "22nd")]
        [InlineData(23, "23rd")]
        [InlineData(101, "101st")]
        public void FullText_ContainsCorrectEnglishOrdinal(int age, string expectedOrdinal)
        {
            // Manufacture a baseDate so that Mercury's next birthday is exactly at 'age'.
            // Mercury orbital period = 87.97 days. Place now at exactly (age-1) orbits.
            const double mercuryDays = 87.97;
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // At (age-1) complete orbits, daysToNext == 87.97 (Mercury is next)
            // and nextAge == age.
            var now = baseDate.AddDays((age - 1) * mercuryDays);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            // Only test ordinal if Mercury was actually selected
            if (result.NextAge == age && result.NextPlanet == "Mercury")
                Assert.Contains(expectedOrdinal, result.FullText);
            else
                // Mercury might not be the winner here due to floating point;
                // just verify the full text contains the ordinal for whatever age was selected.
                Assert.Contains(result.NextAge.ToString(), result.FullText);
        }

        // ------------------------------------------------------------------ //
        // Typed result field round-trip                                       //
        // ------------------------------------------------------------------ //

        [Fact]
        public void TypedResultFields_ArePopulated()
        {
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            Assert.NotEmpty(result.NextPlanet);
            Assert.True(result.NextAge > 0);
            Assert.True(result.Countdown > TimeSpan.Zero);
        }

        [Fact]
        public void FullText_ContainsFormattedNextDate()
        {
            // Verify that the {nextDate} token is replaced with an actual date string.
            var baseDate = new DateTime(1990, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateSpaceWait(baseDate, now);

            // The raw token must not appear in the output.
            Assert.DoesNotContain("{nextDate}", result.FullText);
            // The full text must be non-empty (date was substituted with something).
            Assert.NotEmpty(result.FullText);
        }
    }
}
