using Aeonpulse.Models;
using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Verifies that <see cref="CalculationService"/> methods correctly populate
    /// the raw computed fields on each typed result subclass - the properties that
    /// existing string-assertion tests do not cover.
    ///
    /// <para>
    /// All tests inject a deterministic <c>now</c> value so results are
    /// locale-stable and reproducible on any CI machine.
    /// </para>
    /// </summary>
    public class TypedResultFieldTests
    {
        private readonly CalculationService _svc;

        public TypedResultFieldTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        // ---------------------------------------------------------------
        // CountdownResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculateCountdown_TotalSeconds_MatchesExpectedDuration()
        {
            var baseDate = new DateTime(2000, 6, 15);
            // 2h 30m 45s before the next anniversary (Jun 15 2026)
            var now      = new DateTime(2026, 6, 14, 21, 29, 15);
            long expected = (long)new DateTime(2026, 6, 15).Subtract(now).TotalSeconds;

            var result = _svc.CalculateCountdown(baseDate, now);

            Assert.Equal(expected, result.TotalSeconds);
        }

        [Fact]
        public void CalculateCountdown_AnniversaryDate_IsInTheFuture()
        {
            var baseDate = new DateTime(1990, 3, 10);
            var now      = new DateTime(2026, 1, 1);

            var result = _svc.CalculateCountdown(baseDate, now);

            Assert.True(result.AnniversaryDate > now);
        }

        [Fact]
        public void CalculateCountdown_DecomposedComponents_SumToTotalSeconds()
        {
            var baseDate = new DateTime(2000, 6, 15);
            var now      = new DateTime(2026, 6, 14, 21, 29, 15);

            var r = _svc.CalculateCountdown(baseDate, now);

            long recomposed = r.Days * 86_400 + r.Hours * 3_600 + r.Minutes * 60 + r.Secs;
            Assert.Equal(r.TotalSeconds, recomposed);
        }

        [Fact]
        public void CalculateCountdown_HoursComponent_IsInRange0To23()
        {
            var baseDate = new DateTime(2000, 6, 15);
            var now      = new DateTime(2026, 3, 1, 10, 0, 0);

            var result = _svc.CalculateCountdown(baseDate, now);

            Assert.InRange(result.Hours, 0L, 23L);
        }

        [Fact]
        public void CalculateCountdown_SecsComponent_IsInRange0To59()
        {
            var baseDate = new DateTime(2000, 6, 15);
            var now      = new DateTime(2026, 3, 1, 10, 15, 37);

            var result = _svc.CalculateCountdown(baseDate, now);

            Assert.InRange(result.Secs, 0L, 59L);
        }

        // ---------------------------------------------------------------
        // LifeOdometerResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculateLifeOdometer_Heartbeats_EqualsExpectedFormula()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(3600); // exactly 1 hour

            var result = _svc.CalculateLifeOdometer(baseDate, "T", "2000-01-01", now);

            // 3600 s * 70 bpm / 60 s/min = 4200
            Assert.Equal(4_200L, result.Heartbeats);
        }

        [Fact]
        public void CalculateLifeOdometer_Breaths_EqualsExpectedFormula()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(3600);

            var result = _svc.CalculateLifeOdometer(baseDate, "T", "2000-01-01", now);

            // (3600 / 60.0) * 14 = 840
            Assert.Equal(840L, result.Breaths);
        }

        [Fact]
        public void CalculateLifeOdometer_Heartbeats_AreAlwaysGreaterThanBreaths()
        {
            var baseDate = new DateTime(1990, 5, 1);
            var now      = new DateTime(2026, 1, 1);

            var result = _svc.CalculateLifeOdometer(baseDate, "T", "1990-05-01", now);

            Assert.True(result.Heartbeats > result.Breaths);
        }

        [Fact]
        public void CalculateLifeOdometer_ExactlyOneMinute_Returns70HeartbeatsAnd14Breaths()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(60);

            var result = _svc.CalculateLifeOdometer(baseDate, "T", "2000-01-01", now);

            Assert.Equal(70L, result.Heartbeats);
            Assert.Equal(14L, result.Breaths);
        }

        // ---------------------------------------------------------------
        // AlienAnniversariesResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculateAlienAnniversaries_MarsYears_MatchFormula()
        {
            var baseDate     = new DateTime(2000, 1, 1);
            var now          = baseDate.AddDays(365.0);
            double expected  = 365.0 / 686.98;

            var result = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", now);

            Assert.Equal(expected, result.MarsYears, precision: 10);
        }

        [Fact]
        public void CalculateAlienAnniversaries_VenusYears_MatchFormula()
        {
            var baseDate    = new DateTime(2000, 1, 1);
            var now         = baseDate.AddDays(365.0);
            double expected = 365.0 / 224.7;

            var result = _svc.CalculateAlienAnniversaries(baseDate, "T", "2000-01-01", now);

            Assert.Equal(expected, result.VenusYears, precision: 10);
        }

        [Fact]
        public void CalculateAlienAnniversaries_VenusYears_AlwaysGreaterThanMarsYears()
        {
            // Venus year is shorter than Mars year, so for any elapsed time
            // the Venus year count is always higher than the Mars year count.
            var baseDate = new DateTime(1980, 3, 15);
            var now      = new DateTime(2026, 1, 1);

            var result = _svc.CalculateAlienAnniversaries(baseDate, "T", "1980-03-15", now);

            Assert.True(result.VenusYears > result.MarsYears);
        }

        // ---------------------------------------------------------------
        // GalacticCommuteResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculateGalacticCommute_KmTraveled_EqualsSecondsTimesSolarVelocity()
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(100);
            // Solar system velocity: 225 km/s
            double expected = 100 * 225.0;

            var result = _svc.CalculateGalacticCommute(baseDate, "2000-01-01", useMetric: true, now);

            Assert.Equal(expected, result.KmTraveled, precision: 6);
        }

        [Fact]
        public void CalculateGalacticCommute_UseMetricFlag_StoredOnResult()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var metric   = _svc.CalculateGalacticCommute(baseDate, "1990-01-01", useMetric: true,  now);
            var imperial = _svc.CalculateGalacticCommute(baseDate, "1990-01-01", useMetric: false, now);

            Assert.True(metric.UseMetric);
            Assert.False(imperial.UseMetric);
        }

        [Fact]
        public void CalculateGalacticCommute_BothUnits_SameKmTraveled()
        {
            // KmTraveled is always the raw km value regardless of the unit toggle.
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var metric   = _svc.CalculateGalacticCommute(baseDate, "1990-01-01", useMetric: true,  now);
            var imperial = _svc.CalculateGalacticCommute(baseDate, "1990-01-01", useMetric: false, now);

            Assert.Equal(metric.KmTraveled, imperial.KmTraveled, precision: 0);
        }

        // ---------------------------------------------------------------
        // PhotonPathResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculatePhotonPath_UseMetricFlag_StoredOnResult()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var metric   = _svc.CalculatePhotonPath(baseDate, "1990-01-01", useMetric: true,  now);
            var imperial = _svc.CalculatePhotonPath(baseDate, "1990-01-01", useMetric: false, now);

            Assert.True(metric.UseMetric);
            Assert.False(imperial.UseMetric);
        }

        [Fact]
        public void CalculatePhotonPath_LightYears_IsPositiveForNonZeroElapsed()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var result = _svc.CalculatePhotonPath(baseDate, "1990-01-01", useMetric: true, now);

            Assert.True(result.LightYears > 0);
        }

        [Fact]
        public void CalculatePhotonPath_KmTraveled_EqualsLightYearsTimesKmPerLy()
        {
            var baseDate        = new DateTime(2000, 1, 1, 0, 0, 0);
            // 1 light-year = 9,460,730,472,580.8 km; speed of light = 299,792.458 km/s
            var now             = baseDate.AddSeconds(1);
            double expectedKm   = 299_792.458;

            var result = _svc.CalculatePhotonPath(baseDate, "2000-01-01", useMetric: true, now);

            Assert.Equal(expectedKm, result.KmTraveled, precision: 2);
        }

        [Fact]
        public void CalculatePhotonPath_PhaseIsSolarSystem_ForVeryRecentDate()
        {
            // A baseDate only seconds ago means the photon is still in the Solar System.
            var baseDate = new DateTime(2026, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(10);

            var result = _svc.CalculatePhotonPath(baseDate, "2026-01-01", useMetric: true, now);

            Assert.Equal(PhotonPhase.SolarSystem, result.Phase);
        }

        [Fact]
        public void CalculatePhotonPath_BothUnits_SameKmTraveled()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var metric   = _svc.CalculatePhotonPath(baseDate, "1990-01-01", useMetric: true,  now);
            var imperial = _svc.CalculatePhotonPath(baseDate, "1990-01-01", useMetric: false, now);

            Assert.Equal(metric.KmTraveled, imperial.KmTraveled, precision: 0);
        }

        [Fact]
        public void CalculatePhotonPath_Interstellar_NextStarIsProximaCentauri()
        {
            // Travel 2 light-years (~2 years elapsed) puts the photon in interstellar space
            // (between the Oort Cloud boundary 1.5 LY and Proxima Centauri at 4.246 LY).
            double twoLySeconds = 2.0 * 9_460_730_472_580.8 / 299_792.458;
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(twoLySeconds);

            var result = _svc.CalculatePhotonPath(baseDate, "2000-01-01", useMetric: true, now);

            Assert.Equal(PhotonPhase.Interstellar, result.Phase);
            Assert.False(string.IsNullOrEmpty(result.NextStarName));
            Assert.Equal(4.246, result.NextStarDistance, precision: 2);
            Assert.True(result.ProgressFraction > 0d && result.ProgressFraction < 1d);
            Assert.True(result.DistanceLeft > 0d);
            Assert.False(string.IsNullOrEmpty(result.NextStopText));
        }

        [Fact]
        public void CalculatePhotonPath_ProgressFraction_IsClamped()
        {
            // A very old base date (200 years) puts the photon way past the last star.
            // ProgressFraction should always be in [0, 1].
            var baseDate = new DateTime(1800, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var result = _svc.CalculatePhotonPath(baseDate, "1800-01-01", useMetric: true, now);

            Assert.True(result.ProgressFraction >= 0d);
            Assert.True(result.ProgressFraction <= 1d);
        }

        [Fact]
        public void CalculatePhotonPath_SolarSystem_NextStarIsProximaCentauri()
        {
            // A very recent base date - photon still in Solar System. Next star is Proxima.
            var baseDate = new DateTime(2026, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(100);

            var result = _svc.CalculatePhotonPath(baseDate, "2026-01-01", useMetric: true, now);

            Assert.Equal(PhotonPhase.SolarSystem, result.Phase);
            Assert.Equal(4.246, result.NextStarDistance, precision: 2);
            Assert.True(result.ProgressFraction >= 0d && result.ProgressFraction <= 1d);
        }

        [Fact]
        public void CalculatePhotonPath_HalfwayToProxima_ProgressFractionNearHalf()
        {
            // Position photon at exactly halfway to Proxima Centauri (2.123 light-years).
            // Proxima is at 4.246 ly; halfway = 2.123 ly.
            // Time to travel 2.123 ly = 2.123 * 9460730472580.8 / 299792.458 seconds.
            double halfwayLy   = 4.246 / 2.0;
            double kmPerLy     = 9_460_730_472_580.8;
            double kmPerSecond = 299_792.458;
            double secondsNeeded = halfwayLy * kmPerLy / kmPerSecond;
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0);
            var now      = baseDate.AddSeconds(secondsNeeded);

            var result = _svc.CalculatePhotonPath(baseDate, "2000-01-01", useMetric: true, now);

            // Should be Interstellar phase with ProgressFraction close to 0.5
            Assert.Equal(PhotonPhase.Interstellar, result.Phase);
            Assert.InRange(result.ProgressFraction, 0.48, 0.52);
        }

        // ---------------------------------------------------------------
        // HumanBirthRankResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculateHumanBirthRank_PostTwentiethCentury_RankIsPositive()
        {
            var result = _svc.CalculateHumanBirthRank(new DateTime(1965, 7, 24), "T");

            Assert.True(result.EstimatedRank > 0);
        }

        // ---------------------------------------------------------------
        // PersonalYearResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculatePersonalYear_PersonalYearNumber_IsInRange1To9()
        {
            var baseDate = new DateTime(1965, 7, 24);
            var now      = new DateTime(2026, 6, 1);

            var result = _svc.CalculatePersonalYear(baseDate, "1965-07-24", now);

            Assert.InRange(result.PersonalYearNumber, 1, 9);
        }

        [Fact]
        public void CalculatePersonalYear_CurrentYear_MatchesInjectedNow()
        {
            var baseDate = new DateTime(1980, 3, 15);
            var now      = new DateTime(2026, 6, 1);

            var result = _svc.CalculatePersonalYear(baseDate, "1980-03-15", now);

            Assert.Equal(2026, result.CurrentYear);
        }

        [Fact]
        public void CalculatePersonalYear_PersonalYearNumber_NeverZero()
        {
            // Inputs whose raw sum reduces to 0 must be substituted with 9.
            var baseDate = new DateTime(1999, 9, 9);
            var now      = new DateTime(2016, 1, 1); // year root=9, month=9, day=9

            var result = _svc.CalculatePersonalYear(baseDate, "1999-09-09", now);

            Assert.NotEqual(0, result.PersonalYearNumber);
        }

        // ---------------------------------------------------------------
        // GlobalExhaleResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculateGlobalExhale_DateAt1900_CO2IsPositive()
        {
            var result = _svc.CalculateGlobalExhale(
                new DateTime(1900, 1, 1), "T", "1900-01-01",
                useMetric: true, now: new DateTime(2026, 1, 1));

            Assert.True(result.TotalCO2BillionTonnes > 0);
        }

        [Fact]
        public void CalculateGlobalExhale_Post1900_CO2IsPositive()
        {
            var result = _svc.CalculateGlobalExhale(
                new DateTime(1965, 7, 24), "T", "1965-07-24",
                useMetric: true, now: new DateTime(2026, 1, 1));

            Assert.True(result.TotalCO2BillionTonnes > 0);
        }

        [Fact]
        public void CalculateGlobalExhale_MetricAndImperial_FormattedAmountsAreDifferent()
        {
            var baseDate = new DateTime(1980, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var metric   = _svc.CalculateGlobalExhale(baseDate, "T", "1980-01-01", useMetric: true,  now);
            var imperial = _svc.CalculateGlobalExhale(baseDate, "T", "1980-01-01", useMetric: false, now);

            // The unit suffix must differ (tonnes vs tons).
            Assert.NotEqual(metric.FormattedAmount, imperial.FormattedAmount);
        }

        [Fact]
        public void CalculateGlobalExhale_UseMetricFlag_StoredOnResult()
        {
            var baseDate = new DateTime(1980, 1, 1);
            var now      = new DateTime(2026, 1, 1);

            var metric   = _svc.CalculateGlobalExhale(baseDate, "T", "1980-01-01", useMetric: true,  now);
            var imperial = _svc.CalculateGlobalExhale(baseDate, "T", "1980-01-01", useMetric: false, now);

            Assert.True(metric.UseMetric);
            Assert.False(imperial.UseMetric);
        }

        // ---------------------------------------------------------------
        // TimeJubileesResult - raw field values
        // ---------------------------------------------------------------

        [Fact]
        public void CalculateTimeJubilees_DaysUntil_IsPositive()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 3, 22);

            var result = _svc.CalculateTimeJubilees(baseDate, "T", "1990-01-01", now);

            Assert.True(result.DaysUntil > 0);
        }

        [Fact]
        public void CalculateTimeJubilees_JubileeDate_IsAfterNow()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 3, 22);

            var result = _svc.CalculateTimeJubilees(baseDate, "T", "1990-01-01", now);

            Assert.True(result.JubileeDate > now);
        }

        [Fact]
        public void CalculateTimeJubilees_JubileeValue_IsPositive()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 3, 22);

            var result = _svc.CalculateTimeJubilees(baseDate, "T", "1990-01-01", now);

            Assert.True(result.JubileeValue > 0);
        }

        [Fact]
        public void CalculateTimeJubilees_JubileeUnit_IsNotEmpty()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 3, 22);

            var result = _svc.CalculateTimeJubilees(baseDate, "T", "1990-01-01", now);

            // JubileeUnit is intentionally empty in the flat-list algorithm; the unit
            // is embedded in NextJubileeName (e.g., "50 years" or "10,000 days").
            Assert.NotEmpty(result.NextJubileeName);
        }

        [Fact]
        public void CalculateTimeJubilees_DaysUntil_MatchesDifferenceBetweenJubileeDateAndNow()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 3, 22);

            var result   = _svc.CalculateTimeJubilees(baseDate, "T", "1990-01-01", now);
            long expected = (long)(result.JubileeDate - now).TotalDays;

            Assert.Equal(expected, result.DaysUntil);
        }
    }
}
