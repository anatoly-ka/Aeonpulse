using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateAlienAnniversaries"/>.
    ///
    /// Orbital periods (Earth days): Mercury=87.97, Venus=224.70, Earth=365.25, Mars=686.98, Jupiter=4332.59.
    /// </summary>
    public class CalculateAlienAnniversariesTests
    {
        private readonly CalculationService _svc;

        public CalculateAlienAnniversariesTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void AfterExactlyOneMarsYear_ShowsApproximately1_00MarsYears()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = baseDate.AddDays(686.98);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "Test", "2000-01-01", now);

            Assert.Contains("1.00", result.BriefText);
            Assert.Contains("Mars", result.BriefText);
        }

        [Fact]
        public void AfterExactlyOneVenusYear_ShowsApproximately1_00VenusYears()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = baseDate.AddDays(224.7);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "Test", "2000-01-01", now);

            Assert.Contains("1.00", result.BriefText);
            Assert.Contains("Venus", result.BriefText);
        }

        [Fact]
        public void After365EarthDays_MarsYearsIsCorrect()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now      = baseDate.AddDays(365);
            // 365 / 686.98 = 0.53 Mars years
            double expectedMars = Math.Round(365.0 / 686.98, 2);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "Test", "2000-01-01", now);

            Assert.Contains(expectedMars.ToString("F2"), result.BriefText);
        }

        [Fact]
        public void ZeroElapsedTime_ReturnsNonNullResult()
        {
            var baseDate = new DateTime(2000, 1, 1);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "Test", "2000-01-01", baseDate);

            Assert.NotNull(result);
            Assert.NotNull(result.BriefText);
        }

        // --- New five-planet year fields ---

        [Fact]
        public void MercuryYears_MatchFormula()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now = baseDate.AddDays(365.0);
            double expected = 365.0 / 87.97;

            var result = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", now);

            Assert.Equal(expected, result.MercuryYears, precision: 10);
        }

        [Fact]
        public void EarthYears_MatchFormula()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now = baseDate.AddDays(365.25);
            double expected = 365.25 / 365.25;

            var result = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", now);

            Assert.Equal(expected, result.EarthYears, precision: 10);
        }

        [Fact]
        public void JupiterYears_MatchFormula()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now = baseDate.AddDays(4332.59);
            double expected = 4332.59 / 4332.59;

            var result = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", now);

            Assert.Equal(expected, result.JupiterYears, precision: 10);
        }

        [Fact]
        public void PlanetYears_OrderedCorrectly_MercuryGreatestVenusNext()
        {
            // For a fixed elapsed time, Mercury years > Venus years > Earth years > Mars years > Jupiter years.
            var baseDate = new DateTime(1990, 1, 1);
            var now = baseDate.AddDays(10000);

            var r = _svc.CalculateAlienAnniversaries(baseDate, "T", "1990-01-01", now);

            Assert.True(r.MercuryYears > r.VenusYears);
            Assert.True(r.VenusYears  > r.EarthYears);
            Assert.True(r.EarthYears  > r.MarsYears);
            Assert.True(r.MarsYears   > r.JupiterYears);
        }

        // --- Fractional progress tests ---

        [Fact]
        public void MercuryFraction_AfterExactlyOnePeriod_IsZeroOrOne()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now = baseDate.AddDays(87.97);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", now);

            // Due to floating-point arithmetic, MercuryYears may be 1.0 - epsilon,
            // giving fraction ~0.9999 rather than exactly 0.0. Both values represent
            // a complete orbit, so check the fraction is within 1e-5 of 0.0 or 1.0.
            double f = result.MercuryFraction;
            bool nearZeroOrOne = f < 1e-5 || f > (1.0 - 1e-5);
            Assert.True(nearZeroOrOne, $"Expected fraction near 0 or 1, got {f}");
        }

        [Fact]
        public void MarsFraction_AfterHalfOrbit_IsApproximatelyHalf()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now = baseDate.AddDays(686.98 * 0.5);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", now);

            Assert.Equal(0.5, result.MarsFraction, precision: 6);
        }

        [Fact]
        public void AllFractions_AreInZeroToOneRange()
        {
            var baseDate = new DateTime(1985, 6, 15);
            var now = baseDate.AddDays(14000);

            var r = _svc.CalculateAlienAnniversaries(baseDate, "T", "1985-06-15", now);

            Assert.InRange(r.MercuryFraction,  0.0, 1.0);
            Assert.InRange(r.VenusFraction,    0.0, 1.0);
            Assert.InRange(r.EarthFraction,    0.0, 1.0);
            Assert.InRange(r.MarsFraction,     0.0, 1.0);
            Assert.InRange(r.JupiterFraction,  0.0, 1.0);
        }

        [Fact]
        public void ZeroElapsedTime_AllFractionsAreZero()
        {
            var baseDate = new DateTime(2000, 1, 1);

            var r = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", baseDate);

            Assert.Equal(0.0, r.MercuryFraction,  precision: 10);
            Assert.Equal(0.0, r.VenusFraction,    precision: 10);
            Assert.Equal(0.0, r.EarthFraction,    precision: 10);
            Assert.Equal(0.0, r.MarsFraction,     precision: 10);
            Assert.Equal(0.0, r.JupiterFraction,  precision: 10);
        }

        [Fact]
        public void VenusFraction_AfterOneAndQuarterOrbits_IsApproximatelyQuarter()
        {
            var baseDate = new DateTime(2000, 1, 1);
            var now = baseDate.AddDays(224.70 * 1.25);

            var r = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", now);

            Assert.Equal(0.25, r.VenusFraction, precision: 6);
        }
    }
}
