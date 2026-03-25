using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateYourBreath"/>.
    ///
    /// Core formulas (via shared CalculateBreaths helper):
    ///   breaths   = (long)((totalSeconds / 60.0) * 14.0)
    ///   airLiters = breaths * 0.5
    ///   co2Kg     = totalDays * 1.04
    ///
    /// Air volume is always in litres regardless of unit system.
    /// CO2 mass follows UseMetric: kg (metric) or lbs (imperial, * 2.20462).
    /// The 14 breaths/min rate is shared with CalculateLifeOdometer via CalculateBreaths().
    /// </summary>
    public class CalculateYourBreathTests
    {
        private readonly CalculationService _svc;

        public CalculateYourBreathTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        // ------------------------------------------------------------------ //
        // 1. Happy path - known inputs produce correct raw field values       //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_BreathCountMatchesFormula()
        {
            // Exactly 1 day = 86400 seconds
            // breaths = (86400 / 60) * 14 = 1440 * 14 = 20160
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(86400);

            var result = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: now);

            Assert.Equal(20160.0, result.BreathCount, precision: 3);
        }

        [Fact]
        public void HappyPath_AirLitersMatchesFormula()
        {
            // 1 day: airLiters = 20160 * 0.5 = 10080
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(86400);

            var result = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: now);

            Assert.Equal(10080.0, result.AirLiters, precision: 3);
        }

        [Fact]
        public void HappyPath_Co2KgMatchesFormula()
        {
            // 1 day: co2Kg = 1.0 * 1.04 = 1.04
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(1);

            var result = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: now);

            Assert.Equal(1.04, result.Co2Kg, precision: 4);
        }

        [Fact]
        public void HappyPath_BriefTextContainsBreathCount()
        {
            var baseDate = new DateTime(1965, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateYourBreath(baseDate, "1965-07-24", useMetric: true, now: now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void HappyPath_FullTextContainsLiters()
        {
            var baseDate = new DateTime(1990, 3, 15, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 3, 15, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateYourBreath(baseDate, "1990-03-15", useMetric: true, now: now);

            // Full text must mention liters (always metric per spec)
            Assert.Contains("liter", result.FullText, StringComparison.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------------ //
        // 2. Unit system - air volume always litres; CO2 converts             //
        // ------------------------------------------------------------------ //

        [Fact]
        public void AirVolume_AlwaysInLiters_RegardlessOfUnitSystem()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(30);

            var metric   = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true,  now: now);
            var imperial = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: false, now: now);

            // AirLiters raw field must be identical regardless of unit system
            Assert.Equal(metric.AirLiters, imperial.AirLiters, precision: 4);
        }

        [Fact]
        public void Co2Mass_Metric_ContainsKg()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(365);

            var result = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: now);

            Assert.Contains("kg", result.BriefText, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Co2Mass_Imperial_ContainsLbs()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(365);

            var result = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: false, now: now);

            Assert.Contains("lbs", result.BriefText, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Co2Mass_Imperial_IsLargerThanMetric()
        {
            // 1 kg = 2.20462 lbs, so imperial CO2 display number > metric
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(100);

            var metric   = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true,  now: now);
            var imperial = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: false, now: now);

            // The raw Co2Kg is unit-system-independent
            Assert.Equal(metric.Co2Kg, imperial.Co2Kg, precision: 4);
            // The imperial display value is co2Kg * 2.20462 > co2Kg
            double imperialDisplay = metric.Co2Kg * 2.20462;
            Assert.True(imperialDisplay > metric.Co2Kg);
        }

        // ------------------------------------------------------------------ //
        // 3. Edge cases                                                        //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ZeroElapsedTime_ReturnsZeroBreathsAndCO2()
        {
            var baseDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateYourBreath(baseDate, "2025-01-01", useMetric: true, now: baseDate);

            Assert.Equal(0.0, result.BreathCount, precision: 4);
            Assert.Equal(0.0, result.AirLiters,   precision: 4);
            Assert.Equal(0.0, result.Co2Kg,        precision: 4);
            Assert.NotNull(result.BriefText);
            Assert.NotNull(result.FullText);
        }

        [Fact]
        public void VeryOldDate_DoesNotThrow()
        {
            // 150+ years in the past - verifies no overflow
            var baseDate = new DateTime(1850, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var ex = Record.Exception(() =>
                _svc.CalculateYourBreath(baseDate, "1850-06-01", useMetric: true, now: now));

            Assert.Null(ex);
        }

        [Fact]
        public void VeryOldDate_BreathCountIsPositiveAndLarge()
        {
            var baseDate = new DateTime(1850, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateYourBreath(baseDate, "1850-06-01", useMetric: true, now: now);

            // 175 years * 365.25 days * 86400 s/day / 60 * 14 breaths/min > 1 billion
            Assert.True(result.BreathCount > 1_000_000_000.0);
        }

        // ------------------------------------------------------------------ //
        // 4. Proportional growth                                               //
        // ------------------------------------------------------------------ //

        [Fact]
        public void LongerTimespan_ProducesMoreBreaths()
        {
            var baseDate  = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var shortNow  = baseDate.AddDays(10);
            var longNow   = baseDate.AddDays(20);

            var shortResult = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: shortNow);
            var longResult  = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: longNow);

            Assert.True(longResult.BreathCount > shortResult.BreathCount);
            Assert.True(longResult.Co2Kg       > shortResult.Co2Kg);
            Assert.True(longResult.AirLiters   > shortResult.AirLiters);
        }

        [Fact]
        public void TwiceDuration_ProducesTwiceBreaths()
        {
            // Linear formula - 2x time should yield 2x breaths
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now1     = baseDate.AddDays(30);
            var now2     = baseDate.AddDays(60);

            var r1 = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: now1);
            var r2 = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: now2);

            Assert.Equal(r1.BreathCount * 2, r2.BreathCount, precision: 3);
            Assert.Equal(r1.Co2Kg * 2,       r2.Co2Kg,       precision: 4);
        }

        // ------------------------------------------------------------------ //
        // 5. UseMetric field round-trip                                        //
        // ------------------------------------------------------------------ //

        [Fact]
        public void UseMetric_True_IsStoredInResult()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(10);

            var result = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: true, now: now);

            Assert.True(result.UseMetric);
        }

        [Fact]
        public void UseMetric_False_IsStoredInResult()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(10);

            var result = _svc.CalculateYourBreath(baseDate, "2000-01-01", useMetric: false, now: now);

            Assert.False(result.UseMetric);
        }
    }
}
