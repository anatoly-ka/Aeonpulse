using Aeonpulse.Attributes;

namespace Aeonpulse.Models
{
    /// <summary>
    /// Typed result for <c>CalculationService.CalculateTimeJubilees</c>.
    /// Carries the nearest jubilee milestone data in addition to the
    /// formatted <see cref="TickerData.BriefText"/> and <see cref="TickerData.FullText"/>.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class TimeJubileesResult : TickerData
    {
        /// <summary>Numeric value of the nearest upcoming jubilee (e.g. 20000).</summary>
        public long JubileeValue { get; init; }

        /// <summary>Localised unit label of the jubilee (e.g. "days", "months").</summary>
        public string JubileeUnit { get; init; } = string.Empty;

        /// <summary>Calendar date on which the jubilee falls.</summary>
        public DateTime JubileeDate { get; init; }

        /// <summary>Whole days remaining until the jubilee date.</summary>
        public long DaysUntil { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateCountdown</c>.
    /// Carries the decomposed time components of the countdown to the next anniversary.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class CountdownResult : TickerData
    {
        /// <summary>Total whole seconds remaining until the next anniversary.</summary>
        public long TotalSeconds { get; init; }

        /// <summary>Whole days component of the remaining time.</summary>
        public long Days { get; init; }

        /// <summary>Hours component (0-23) of the remaining time.</summary>
        public long Hours { get; init; }

        /// <summary>Minutes component (0-59) of the remaining time.</summary>
        public long Minutes { get; init; }

        /// <summary>Seconds component (0-59) of the remaining time.</summary>
        public long Secs { get; init; }

        /// <summary>The upcoming anniversary date being counted down to.</summary>
        public DateTime AnniversaryDate { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateLifeOdometer</c>.
    /// Carries the raw physiological totals so callers (e.g. tease-text generation)
    /// can use numeric values directly without parsing formatted strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class LifeOdometerResult : TickerData
    {
        /// <summary>Total estimated heartbeats since the base date (70 bpm average).</summary>
        public long Heartbeats { get; init; }

        /// <summary>Total estimated breaths since the base date (16 /min average).</summary>
        public long Breaths { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateAlienAnniversaries</c>.
    /// Carries equivalent planetary ages in Mars and Venus years.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class AlienAnniversariesResult : TickerData
    {
        /// <summary>Age in Martian years (1 Mars year = 686.98 Earth days).</summary>
        public double MarsYears { get; init; }

        /// <summary>Age in Venusian years (1 Venus year = 224.7 Earth days).</summary>
        public double VenusYears { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateGalacticCommute</c>.
    /// Carries the raw distance travelled through the Milky Way and the
    /// active unit system so consumers do not need to re-derive it.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class GalacticCommuteResult : TickerData
    {
        /// <summary>Raw distance travelled in kilometres (before any unit conversion).</summary>
        public double KmTraveled { get; init; }

        /// <summary>
        /// Formatted, scaled distance string including unit label
        /// (e.g. "3.45 million km" or "2.14 million miles").
        /// </summary>
        public string Distance { get; init; } = string.Empty;

        /// <summary><c>true</c> when the result was computed in metric units.</summary>
        public bool UseMetric { get; init; }
    }

    /// <summary>
    /// Phase of the photon's cosmic journey, used by <see cref="PhotonPathResult"/>.
    /// </summary>
    public enum PhotonPhase
    {
        /// <summary>Still within the Solar System.</summary>
        SolarSystem,
        /// <summary>Approaching or within the Heliopause boundary.</summary>
        Heliopause,
        /// <summary>Within the Oort Cloud (less than 1.5 light-years).</summary>
        OortCloud,
        /// <summary>Interstellar space beyond the Oort Cloud but before the nearest star.</summary>
        Interstellar,
        /// <summary>Past one or more named stars in the catalogue.</summary>
        PastStar
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculatePhotonPath</c>.
    /// Carries the raw travel data and the named star most recently passed (if any),
    /// enabling tease-text and cross-ticker logic to reference cosmic milestones
    /// without re-parsing the formatted strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class PhotonPathResult : TickerData
    {
        /// <summary>Distance in kilometres that a photon has travelled since the base date.</summary>
        public double KmTraveled { get; init; }

        /// <summary>Distance converted to light-years.</summary>
        public double LightYears { get; init; }

        /// <summary>Current phase of the photon's journey.</summary>
        public PhotonPhase Phase { get; init; }

        /// <summary>
        /// Name of the most recently passed named star, or <see langword="null"/>
        /// when the photon has not yet reached <c>Proxima Centauri</c>.
        /// </summary>
        public string? StarName { get; init; }

        /// <summary>Distance of the most recently passed star in light-years, or 0.</summary>
        public double StarLy { get; init; }

        /// <summary><c>true</c> when the result was computed in metric units.</summary>
        public bool UseMetric { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateHumanBirthRank</c>.
    /// Carries the estimated ordinal birth rank as a numeric value so callers
    /// can perform comparisons or format it independently of the display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class HumanBirthRankResult : TickerData
    {
        /// <summary>
        /// Estimated birth rank (N-th human ever born), or 0 for pre-1900 dates
        /// where only a fixed-text response is produced.
        /// </summary>
        public double EstimatedRank { get; init; }

        /// <summary><c>true</c> when the base date is before 1900-01-01.</summary>
        public bool IsPreTwentiethCentury { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateBirthRune</c>.
    /// Carries the matched Elder Futhark rune data so callers can reference
    /// the rune name, symbol, and interpretations without parsing display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class BirthRuneResult : TickerData
    {
        /// <summary>Localised rune name (e.g. "Fehu").</summary>
        public string RuneName { get; init; } = string.Empty;

        /// <summary>Unicode rune glyph (e.g. "&#x16A0;").</summary>
        public string RuneSymbol { get; init; } = string.Empty;

        /// <summary>Short thematic interpretation from AppResources.</summary>
        public string RuneBrief { get; init; } = string.Empty;

        /// <summary>Full thematic interpretation from AppResources.</summary>
        public string RuneFull { get; init; } = string.Empty;
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculatePersonalYear</c>.
    /// Carries the computed numerology number and the current calendar year
    /// so downstream logic can reuse them without re-computing.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class PersonalYearResult : TickerData
    {
        /// <summary>Numerological personal year number in the range 1-9.</summary>
        public int PersonalYearNumber { get; init; }

        /// <summary>The calendar year for which the personal year was computed.</summary>
        public int CurrentYear { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateGlobalExhale</c>.
    /// Carries the raw CO2 figure in billion metric tonnes so downstream logic
    /// (tease text, cross-ticker comparisons) can use the numeric value directly.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class GlobalExhaleResult : TickerData
    {
        /// <summary>
        /// Total CO2 emitted since the base date in billion metric tonnes.
        /// 0 for pre-1900 base dates (only a fixed total is shown).
        /// </summary>
        public double TotalCO2BillionTonnes { get; init; }

        /// <summary>
        /// Formatted CO2 amount string including unit label, identical to the
        /// token substituted into <see cref="TickerData.BriefText"/>
        /// (e.g. "3.45 billion tonnes" or "3.40 billion tons").
        /// Available directly so tease-text generation avoids re-computing it.
        /// </summary>
        public string FormattedAmount { get; init; } = string.Empty;

        /// <summary><c>true</c> when the result was computed in metric units.</summary>
        public bool UseMetric { get; init; }

        /// <summary><c>true</c> when the base date is before 1900-01-01.</summary>
        public bool IsPreTwentiethCentury { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateCosmicStretch</c>.
    /// Carries the raw expansion distance in kilometres so callers can use
    /// the numeric value directly without parsing the formatted display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class CosmicStretchResult : TickerData
    {
        /// <summary>Raw expansion distance in kilometres since the base date.</summary>
        public double KmExpanded { get; init; }

        /// <summary>
        /// Formatted, scaled primary distance string including unit label
        /// (e.g. "3.45 million km" or "2.14 million miles").
        /// </summary>
        public string Distance { get; init; } = string.Empty;

        /// <summary><c>true</c> when the result was computed in metric units.</summary>
        public bool UseMetric { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateYourBreath</c>.
    /// Carries the raw breath count, air volume, and CO2 mass so callers can
    /// use the numeric values directly without parsing the formatted display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class YourBreathResult : TickerData
    {
        /// <summary>Total estimated breaths taken since the base date (14 breaths/min average).</summary>
        public double BreathCount { get; init; }

        /// <summary>Total air volume processed in litres since the base date (0.5 L per breath).</summary>
        public double AirLiters { get; init; }

        /// <summary>Total CO2 mass exhaled in kilograms since the base date (1.04 kg/day).</summary>
        public double Co2Kg { get; init; }

        /// <summary><c>true</c> when the result was computed in metric units (CO2 mass display only).</summary>
        public bool UseMetric { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateCellularRefresh</c>.
    /// Carries the raw skin cycle count and red blood cell count so callers can
    /// use the numeric values directly without parsing the formatted display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class CellularRefreshResult : TickerData
    {
        /// <summary>
        /// Number of complete (and partial) outer skin layer replacement cycles since
        /// the base date. The epidermis renews itself approximately every 27 days.
        /// Stored as double to capture fractional in-progress cycles (formatted as N2).
        /// </summary>
        public double SkinCycles { get; init; }

        /// <summary>
        /// Total estimated red blood cells generated since the base date.
        /// Based on a production rate of 2,000,000 new RBCs per second.
        /// </summary>
        public double TotalRbcsCreated { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateVibrantCosmos</c>.
    /// Carries the raw astronomical counts so callers can use the numeric values
    /// directly without parsing the formatted display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class VibrantCosmosResult : TickerData
    {
        /// <summary>Total stars born since the base date (4,800 per second).</summary>
        public double StarsBorn { get; init; }

        /// <summary>Total supernovas since the base date (30 per second).</summary>
        public double Supernovas { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateGlobalCrowd</c>.
    /// Carries the estimated human population at the base date and at the
    /// current moment so callers can use the numeric values directly without
    /// parsing the formatted display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class GlobalCrowdResult : TickerData
    {
        /// <summary>Estimated global population at the user's base date.</summary>
        public double BasePopulation { get; init; }

        /// <summary>Estimated current global population at calculation time.</summary>
        public double CurrentPopulation { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateLifeLog</c>.
    /// Carries the raw computed total hours per activity and the two activity
    /// names/hours chosen for the brief view so callers can re-randomise without
    /// re-running the full calculation.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class LifeLogResult : TickerData
    {
        /// <summary>Total elapsed days since the base date used for all activity calculations.</summary>
        public double TotalDays { get; init; }

        /// <summary>
        /// Dictionary mapping localised activity name to total hours accumulated since the base date.
        /// </summary>
        public Dictionary<string, double> ActivityHours { get; init; } = new();

        /// <summary>Name of the first randomly selected activity shown in the brief view.</summary>
        public string Activity1Name { get; init; } = string.Empty;

        /// <summary>Total hours for the first randomly selected activity (raw, unformatted).</summary>
        public double Activity1Hours { get; init; }

        /// <summary>Name of the second randomly selected activity shown in the brief view.</summary>
        public string Activity2Name { get; init; } = string.Empty;

        /// <summary>Total hours for the second randomly selected activity (raw, unformatted).</summary>
        public double Activity2Hours { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateSpaceWait</c>.
    /// Carries the name of the next planet, the ordinal age milestone, and
    /// the countdown TimeSpan so callers can use the values directly without
    /// parsing the formatted display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class SpaceWaitResult : TickerData
    {
        /// <summary>Localised name of the planet whose orbital birthday comes next.</summary>
        public string NextPlanet { get; init; } = string.Empty;

        /// <summary>The ordinal age (birthday number) that will be reached on that planet.</summary>
        public int NextAge { get; init; }

        /// <summary>Time remaining until the next planetary orbital birthday.</summary>
        public TimeSpan Countdown { get; init; }
    }

    /// <summary>
    /// Typed result for <c>CalculationService.CalculateVibrantHumanity</c>.
    /// Carries the raw demographic counts between the base date and now
    /// so callers can use the numeric values directly without parsing the
    /// formatted display strings.
    /// </summary>
    [AIContext("DataTransferObject")]
    public class VibrantHumanityResult : TickerData
    {
        /// <summary>Estimated number of people born globally since the base date.</summary>
        public double BornBetweenDates { get; init; }

        /// <summary>Estimated number of people who died globally since the base date.</summary>
        public double DiedBetweenDates { get; init; }

        /// <summary>Estimated pairs of twins born since the base date (approx. 2.4% of births).</summary>
        public double TwinsBorn { get; init; }

        /// <summary>Estimated deaths from heart disease or stroke since the base date (approx. 27% of deaths).</summary>
        public double HeartDeaths { get; init; }

        /// <summary>Estimated deaths from cancer since the base date (approx. 18% of deaths).</summary>
        public double CancerDeaths { get; init; }
    }
}
