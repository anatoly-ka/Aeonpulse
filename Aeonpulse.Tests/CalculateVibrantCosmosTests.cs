using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateVibrantCosmos"/>.
    ///
    /// Core formulas:
    ///   starsBorn  = totalSeconds * 4,800   (stars born per second in the observable universe)
    ///   supernovas = totalSeconds * 30      (supernovas per second in the observable universe)
    ///
    /// No unit-system dependency: both values are pure astronomical counts.
    /// Values are formatted with N0 for readability on mobile screens.
    /// </summary>
    public class CalculateVibrantCosmosTests
    {
        private readonly CalculationService _svc;

        public CalculateVibrantCosmosTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        // ------------------------------------------------------------------ //
        // 1. Happy path - stars born formula                                  //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_StarsBorn_OneSecond()
        {
            // Exactly 1 second elapsed: 4,800 stars should have been born.
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(1);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.Equal(4800.0, result.StarsBorn, precision: 0);
        }

        [Fact]
        public void HappyPath_StarsBorn_OneHour()
        {
            // 3,600 seconds elapsed: 3,600 * 4,800 = 17,280,000 stars.
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(3600);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.Equal(3600.0 * 4800.0, result.StarsBorn, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // 2. Happy path - supernovas formula                                  //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_Supernovas_OneSecond()
        {
            // Exactly 1 second elapsed: 30 supernovas.
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(1);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.Equal(30.0, result.Supernovas, precision: 0);
        }

        [Fact]
        public void HappyPath_Supernovas_OneHour()
        {
            // 3,600 seconds: 3,600 * 30 = 108,000 supernovas.
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(3600);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.Equal(3600.0 * 30.0, result.Supernovas, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // 3. BriefText and FullText are non-empty                             //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_BriefText_IsNotEmpty()
        {
            var baseDate = new DateTime(1965, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
        }

        [Fact]
        public void HappyPath_FullText_IsNotEmpty()
        {
            var baseDate = new DateTime(1965, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.NotEmpty(result.FullText);
        }

        // ------------------------------------------------------------------ //
        // 4. Zero elapsed time                                                //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ZeroElapsedTime_StarsBornIsZero()
        {
            var baseDate = new DateTime(2020, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            var now      = baseDate; // same instant

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.Equal(0.0, result.StarsBorn, precision: 0);
            Assert.Equal(0.0, result.Supernovas, precision: 0);
        }

        [Fact]
        public void ZeroElapsedTime_ResultIsNotNull()
        {
            var baseDate = new DateTime(2020, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            var now      = baseDate;

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.NotNull(result);
            Assert.NotNull(result.BriefText);
            Assert.NotNull(result.FullText);
        }

        // ------------------------------------------------------------------ //
        // 5. Very old base date - no exception                                //
        // ------------------------------------------------------------------ //

        [Fact]
        public void VeryOldDate_NoException()
        {
            var baseDate = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Record.Exception(() => _svc.CalculateVibrantCosmos(baseDate, now: now));

            Assert.Null(ex);
        }

        [Fact]
        public void VeryOldDate_StarsBornIsLargePositiveNumber()
        {
            var baseDate = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.True(result.StarsBorn > 1_000_000_000.0, "Stars born over 225 years should exceed 1 billion.");
        }

        // ------------------------------------------------------------------ //
        // 6. Proportional growth - double the elapsed time doubles the counts //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ProportionalGrowth_DoubleTime_DoublesCounts()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now1     = baseDate.AddSeconds(1000);
            var now2     = baseDate.AddSeconds(2000);

            var result1 = _svc.CalculateVibrantCosmos(baseDate, now: now1);
            var result2 = _svc.CalculateVibrantCosmos(baseDate, now: now2);

            Assert.Equal(result1.StarsBorn * 2, result2.StarsBorn, precision: 0);
            Assert.Equal(result1.Supernovas * 2, result2.Supernovas, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // 7. Ratio of stars to supernovas is always 160:1                    //
        // ------------------------------------------------------------------ //

        [Fact]
        public void StarToSupernovaRatio_IsAlways160To1()
        {
            // 4800 / 30 = 160
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(5000);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            if (result.Supernovas > 0)
                Assert.Equal(160.0, result.StarsBorn / result.Supernovas, precision: 6);
        }

        // ------------------------------------------------------------------ //
        // 8. Typed result fields round-trip                                   //
        // ------------------------------------------------------------------ //

        [Fact]
        public void TypedResult_StarsBornAndSupernovasFieldsRoundTrip()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(100);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.Equal(100.0 * 4800.0, result.StarsBorn, precision: 0);
            Assert.Equal(100.0 * 30.0,   result.Supernovas, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // 9. BriefText contains formatted numbers (N0 format check)          //
        // ------------------------------------------------------------------ //

        [Fact]
        public void BriefText_ContainsFormattedStarCount()
        {
            // 1 second = exactly 4,800 stars - formatted as "4,800" in N0
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(1);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.Contains("4,800", result.BriefText);
        }

        [Fact]
        public void BriefText_ContainsFormattedSupernovaCount()
        {
            // 1 second = exactly 30 supernovas - formatted as "30" in N0
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(1);

            var result = _svc.CalculateVibrantCosmos(baseDate, now: now);

            Assert.Contains("30", result.BriefText);
        }
    }
}
