using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateCellularRefresh"/>.
    ///
    /// Core formulas:
    ///   skinCycles          = totalDays / 27.0       (displayed as N0 - whole number)
    ///   totalRbcsCreated    = totalSeconds * 2,000,000
    ///   totalRbcsBillions   = totalRbcsCreated / 1,000,000,000  (displayed as N2)
    ///
    /// No unit-system dependency: skin cycles and RBC counts are pure biological
    /// counts requiring no metric/imperial conversion.
    /// </summary>
    public class CalculateCellularRefreshTests
    {
        private readonly CalculationService _svc;

        public CalculateCellularRefreshTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        // ------------------------------------------------------------------ //
        // 1. Happy path - skin cycle count formula                           //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_SkinCycles_OneCycle()
        {
            // Exactly 27 days = 1 complete skin cycle
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(27);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now);

            Assert.Equal(1.0, result.SkinCycles, precision: 6);
        }

        [Fact]
        public void HappyPath_SkinCycles_TwoCycles()
        {
            // 54 days = 2 complete skin cycles
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(54);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now);

            Assert.Equal(2.0, result.SkinCycles, precision: 6);
        }

        // ------------------------------------------------------------------ //
        // 2. Happy path - RBC count formula (raw field, then billions)       //
        // ------------------------------------------------------------------ //

        [Fact]
        public void HappyPath_TotalRbcs_OneSecond()
        {
            // 1 second: 1 * 2,000,000 = 2,000,000 raw
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(1);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now);

            Assert.Equal(2_000_000.0, result.TotalRbcsCreated, precision: 0);
        }

        [Fact]
        public void HappyPath_TotalRbcs_OneDay_RawField()
        {
            // 86400 s * 2,000,000 = 172,800,000,000 raw
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(86400);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now);

            Assert.Equal(172_800_000_000.0, result.TotalRbcsCreated, precision: 0);
        }

        [Fact]
        public void HappyPath_TotalRbcs_OneBillionSeconds()
        {
            // 1,000,000,000 s * 2,000,000 / 1e9 = 2,000,000 billion -> 2,000,000.00
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddSeconds(1_000_000_000);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now);

            double expectedBillions = 1_000_000_000.0 * 2_000_000.0 / 1_000_000_000.0;
            Assert.Equal(expectedBillions, result.TotalRbcsCreated / 1_000_000_000.0, precision: 2);
        }

        // ------------------------------------------------------------------ //
        // 3. Output strings                                                  //
        // ------------------------------------------------------------------ //

        [Fact]
        public void BriefText_IsNotEmpty()
        {
            var baseDate = new DateTime(1965, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "1965-07-24", now: now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
        }

        [Fact]
        public void FullText_IsNotEmpty()
        {
            var baseDate = new DateTime(1965, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "1965-07-24", now: now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void BriefText_ContainsSkinCycleValue_N0Format()
        {
            // 540 days / 27 = 20 skin cycles -> N0 = "20"
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(540);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now);

            // N0 formatting -> "20" (locale-stable because TestFixture.InitEnglish())
            Assert.Contains(">20<", result.BriefText);
        }

        [Fact]
        public void BriefText_ContainsBillionRbcUnit()
        {
            var baseDate = new DateTime(1965, 7, 24, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "1965-07-24", now: now);

            Assert.Contains("billion RBCs", result.BriefText);
        }

        // ------------------------------------------------------------------ //
        // 4. Zero elapsed time                                               //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ZeroElapsed_SkinCyclesIsZero()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: baseDate);

            Assert.Equal(0.0, result.SkinCycles, precision: 6);
        }

        [Fact]
        public void ZeroElapsed_TotalRbcsIsZero()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: baseDate);

            Assert.Equal(0.0, result.TotalRbcsCreated, precision: 0);
        }

        [Fact]
        public void ZeroElapsed_StringsAreNonNull()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: baseDate);

            Assert.NotNull(result.BriefText);
            Assert.NotNull(result.FullText);
        }

        // ------------------------------------------------------------------ //
        // 5. Large elapsed time - no exception thrown                        //
        // ------------------------------------------------------------------ //

        [Fact]
        public void VeryOldBaseDate_DoesNotThrow()
        {
            var baseDate = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var exception = Record.Exception(() =>
                _svc.CalculateCellularRefresh(baseDate, "My Birthday", "1800-01-01", now: now));

            Assert.Null(exception);
        }

        [Fact]
        public void VeryOldBaseDate_ValuesArePositive()
        {
            var baseDate = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "1800-01-01", now: now);

            Assert.True(result.SkinCycles > 0);
            Assert.True(result.TotalRbcsCreated > 0);
        }

        // ------------------------------------------------------------------ //
        // 6. Proportional growth                                             //
        // ------------------------------------------------------------------ //

        [Fact]
        public void ProportionalGrowth_DoublePeriod_DoublesSkinCycles()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now1     = baseDate.AddDays(100);
            var now2     = baseDate.AddDays(200);

            var result1 = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now1);
            var result2 = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now2);

            Assert.Equal(result1.SkinCycles * 2, result2.SkinCycles, precision: 6);
        }

        [Fact]
        public void ProportionalGrowth_DoublePeriod_DoublesRbcs()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now1     = baseDate.AddSeconds(3600);
            var now2     = baseDate.AddSeconds(7200);

            var result1 = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now1);
            var result2 = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now2);

            Assert.Equal(result1.TotalRbcsCreated * 2, result2.TotalRbcsCreated, precision: 0);
        }

        // ------------------------------------------------------------------ //
        // 7. Raw field round-trip                                            //
        // ------------------------------------------------------------------ //

        [Fact]
        public void RawFields_TenSkinCycles_RbcsConsistent()
        {
            // 270 days = 10 skin cycles exactly
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var now      = baseDate.AddDays(270);

            var result = _svc.CalculateCellularRefresh(baseDate, "My Birthday", "2000-01-01", now: now);

            Assert.Equal(10.0, result.SkinCycles, precision: 6);

            // 270 days * 86400 s/day * 2,000,000 RBC/s
            double expectedRbcs = 270.0 * 86400.0 * 2_000_000.0;
            Assert.Equal(expectedRbcs, result.TotalRbcsCreated, precision: 0);
        }
    }
}
