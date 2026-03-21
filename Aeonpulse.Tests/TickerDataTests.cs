using Aeonpulse.Models;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Tests for <see cref="TickerData"/> and its ten typed result subclasses
    /// defined in <c>TickerResults.cs</c>.
    ///
    /// <para>
    /// Scope: property-change notification on <see cref="TickerData"/>, and
    /// correct population of the raw computed fields on each typed subclass.
    /// These are pure .NET types with no MAUI dependency, so they run in the
    /// existing <c>net9.0</c> test project without a device.
    /// </para>
    /// </summary>
    public class TickerDataTests
    {
        // ---------------------------------------------------------------
        // TickerData - INotifyPropertyChanged
        // ---------------------------------------------------------------

        [Fact]
        public void BriefText_SetNewValue_FiresPropertyChanged()
        {
            var data = new TickerData();
            string? captured = null;
            data.PropertyChanged += (_, e) => captured = e.PropertyName;

            data.BriefText = "hello";

            Assert.Equal(nameof(TickerData.BriefText), captured);
        }

        [Fact]
        public void FullText_SetNewValue_FiresPropertyChanged()
        {
            var data = new TickerData();
            string? captured = null;
            data.PropertyChanged += (_, e) => captured = e.PropertyName;

            data.FullText = "world";

            Assert.Equal(nameof(TickerData.FullText), captured);
        }

        [Fact]
        public void BriefText_SetTwice_FiresPropertyChangedBothTimes()
        {
            var data = new TickerData();
            int fireCount = 0;
            data.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TickerData.BriefText))
                    fireCount++;
            };

            data.BriefText = "first";
            data.BriefText = "second";

            Assert.Equal(2, fireCount);
        }

        [Fact]
        public void BriefText_SetToSameValue_StillFiresPropertyChanged()
        {
            // TickerData does not guard against same-value re-sets -
            // intentional: live tickers regenerate strings every second.
            const string value = "same";
            var data = new TickerData { BriefText = value };
            int fireCount = 0;
            data.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(TickerData.BriefText))
                    fireCount++;
            };

            data.BriefText = value;

            Assert.Equal(1, fireCount);
        }

        [Fact]
        public void NewTickerData_DefaultsToEmptyStrings()
        {
            var data = new TickerData();

            Assert.Equal(string.Empty, data.BriefText);
            Assert.Equal(string.Empty, data.FullText);
        }

        [Fact]
        public void FullText_SetNewValue_ValueIsRetained()
        {
            var data = new TickerData();

            data.FullText = "expanded content";

            Assert.Equal("expanded content", data.FullText);
        }

        // ---------------------------------------------------------------
        // CountdownResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void CountdownResult_RawFields_AreCorrectlyPopulated()
        {
            var anniversary = new DateTime(2027, 6, 15);
            var result = new CountdownResult
            {
                BriefText       = "brief",
                FullText        = "full",
                TotalSeconds    = 90_061L,
                Days            = 1L,
                Hours           = 1L,
                Minutes         = 1L,
                Secs            = 1L,
                AnniversaryDate = anniversary,
            };

            Assert.Equal(90_061L,    result.TotalSeconds);
            Assert.Equal(1L,         result.Days);
            Assert.Equal(1L,         result.Hours);
            Assert.Equal(1L,         result.Minutes);
            Assert.Equal(1L,         result.Secs);
            Assert.Equal(anniversary, result.AnniversaryDate);
            Assert.Equal("brief",    result.BriefText);
        }

        [Fact]
        public void CountdownResult_InheritsInpc_FromTickerData()
        {
            var result = new CountdownResult();
            string? captured = null;
            result.PropertyChanged += (_, e) => captured = e.PropertyName;

            result.BriefText = "updated";

            Assert.Equal(nameof(TickerData.BriefText), captured);
        }

        // ---------------------------------------------------------------
        // LifeOdometerResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void LifeOdometerResult_RawFields_AreCorrectlyPopulated()
        {
            var result = new LifeOdometerResult
            {
                Heartbeats = 4_200_000L,
                Breaths    =   960_000L,
            };

            Assert.Equal(4_200_000L, result.Heartbeats);
            Assert.Equal(  960_000L, result.Breaths);
        }

        [Fact]
        public void LifeOdometerResult_InheritsInpc_FromTickerData()
        {
            var result = new LifeOdometerResult();
            string? captured = null;
            result.PropertyChanged += (_, e) => captured = e.PropertyName;

            result.FullText = "updated";

            Assert.Equal(nameof(TickerData.FullText), captured);
        }

        // ---------------------------------------------------------------
        // AlienAnniversariesResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void AlienAnniversariesResult_RawFields_ReflectInputValues()
        {
            var result = new AlienAnniversariesResult
            {
                MarsYears  = 0.53,
                VenusYears = 1.62,
            };

            Assert.Equal(0.53, result.MarsYears,  precision: 10);
            Assert.Equal(1.62, result.VenusYears, precision: 10);
        }

        // ---------------------------------------------------------------
        // GalacticCommuteResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void GalacticCommuteResult_RawFields_AreCorrectlyPopulated()
        {
            var result = new GalacticCommuteResult
            {
                KmTraveled = 1_234_567.89,
                Distance   = "1.23 million km",
                UseMetric  = true,
            };

            Assert.Equal(1_234_567.89, result.KmTraveled, precision: 2);
            Assert.Equal("1.23 million km", result.Distance);
            Assert.True(result.UseMetric);
        }

        [Fact]
        public void GalacticCommuteResult_ImperialFlag_StoredCorrectly()
        {
            var result = new GalacticCommuteResult { UseMetric = false };

            Assert.False(result.UseMetric);
        }

        // ---------------------------------------------------------------
        // PhotonPathResult - raw fields and PhotonPhase enum
        // ---------------------------------------------------------------

        [Fact]
        public void PhotonPathResult_RawFields_AreCorrectlyPopulated()
        {
            var result = new PhotonPathResult
            {
                KmTraveled = 9.461e12,
                LightYears = 1.0,
                Phase      = PhotonPhase.Interstellar,
                StarName   = "Proxima Centauri",
                StarLy     = 4.24,
                UseMetric  = true,
            };

            Assert.Equal(PhotonPhase.Interstellar, result.Phase);
            Assert.Equal("Proxima Centauri", result.StarName);
            Assert.Equal(4.24, result.StarLy, precision: 10);
            Assert.True(result.UseMetric);
        }

        [Fact]
        public void PhotonPathResult_StarName_IsNullableAndDefaultsToNull()
        {
            var result = new PhotonPathResult();

            Assert.Null(result.StarName);
        }

        [Theory]
        [InlineData(PhotonPhase.SolarSystem)]
        [InlineData(PhotonPhase.Heliopause)]
        [InlineData(PhotonPhase.OortCloud)]
        [InlineData(PhotonPhase.Interstellar)]
        [InlineData(PhotonPhase.PastStar)]
        public void PhotonPhase_AllEnumValuesAreDefined(PhotonPhase phase)
        {
            // Confirms enum members were not accidentally removed or renamed.
            Assert.True(Enum.IsDefined(typeof(PhotonPhase), phase));
        }

        // ---------------------------------------------------------------
        // HumanBirthRankResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void HumanBirthRankResult_RawFields_AreCorrectlyPopulated()
        {
            var result = new HumanBirthRankResult
            {
                EstimatedRank         = 3_800_000_000.0,
                IsPreTwentiethCentury = false,
            };

            Assert.Equal(3_800_000_000.0, result.EstimatedRank, precision: 0);
            Assert.False(result.IsPreTwentiethCentury);
        }

        [Fact]
        public void HumanBirthRankResult_PreTwentiethCentury_FlagIsTrue()
        {
            var result = new HumanBirthRankResult
            {
                EstimatedRank         = 0,
                IsPreTwentiethCentury = true,
            };

            Assert.Equal(0, result.EstimatedRank, precision: 0);
            Assert.True(result.IsPreTwentiethCentury);
        }

        // ---------------------------------------------------------------
        // BirthRuneResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void BirthRuneResult_RawFields_AreCorrectlyPopulated()
        {
            var result = new BirthRuneResult
            {
                RuneName   = "Fehu",
                RuneSymbol = "\u16A0",
                RuneBrief  = "Wealth and luck.",
                RuneFull   = "Fehu represents mobile wealth and primal energy.",
            };

            Assert.Equal("Fehu",   result.RuneName);
            Assert.Equal("\u16A0", result.RuneSymbol);
            Assert.NotEmpty(result.RuneBrief);
            Assert.NotEmpty(result.RuneFull);
        }

        [Fact]
        public void BirthRuneResult_DefaultsToEmptyStrings()
        {
            var result = new BirthRuneResult();

            Assert.Equal(string.Empty, result.RuneName);
            Assert.Equal(string.Empty, result.RuneSymbol);
            Assert.Equal(string.Empty, result.RuneBrief);
            Assert.Equal(string.Empty, result.RuneFull);
        }

        // ---------------------------------------------------------------
        // PersonalYearResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void PersonalYearResult_RawFields_AreCorrectlyPopulated()
        {
            var result = new PersonalYearResult
            {
                PersonalYearNumber = 5,
                CurrentYear        = 2026,
            };

            Assert.Equal(5,    result.PersonalYearNumber);
            Assert.Equal(2026, result.CurrentYear);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(9)]
        public void PersonalYearResult_NumberIsInValidRange(int number)
        {
            var result = new PersonalYearResult { PersonalYearNumber = number };

            Assert.InRange(result.PersonalYearNumber, 1, 9);
        }

        // ---------------------------------------------------------------
        // GlobalExhaleResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void GlobalExhaleResult_RawFields_AreCorrectlyPopulated()
        {
            var result = new GlobalExhaleResult
            {
                TotalCO2BillionTonnes = 42.5,
                FormattedAmount       = "42.50 billion tonnes",
                UseMetric             = true,
                IsPreTwentiethCentury = false,
            };

            Assert.Equal(42.5, result.TotalCO2BillionTonnes, precision: 10);
            Assert.Equal("42.50 billion tonnes", result.FormattedAmount);
            Assert.True(result.UseMetric);
            Assert.False(result.IsPreTwentiethCentury);
        }

        [Fact]
        public void GlobalExhaleResult_ImperialAndPre1900Flags_StoredCorrectly()
        {
            var result = new GlobalExhaleResult
            {
                UseMetric             = false,
                IsPreTwentiethCentury = true,
            };

            Assert.False(result.UseMetric);
            Assert.True(result.IsPreTwentiethCentury);
        }

        // ---------------------------------------------------------------
        // TimeJubileesResult - raw fields
        // ---------------------------------------------------------------

        [Fact]
        public void TimeJubileesResult_RawFields_AreCorrectlyPopulated()
        {
            var jubileeDate = new DateTime(2027, 1, 1);
            var result = new TimeJubileesResult
            {
                JubileeValue = 25_000L,
                JubileeUnit  = "days",
                JubileeDate  = jubileeDate,
                DaysUntil    = 285L,
            };

            Assert.Equal(25_000L,     result.JubileeValue);
            Assert.Equal("days",      result.JubileeUnit);
            Assert.Equal(jubileeDate, result.JubileeDate);
            Assert.Equal(285L,        result.DaysUntil);
        }
    }
}
