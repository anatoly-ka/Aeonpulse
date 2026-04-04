using Aeonpulse.Attributes;
using Aeonpulse.Models;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Aeonpulse.Resources;

[assembly: InternalsVisibleTo("Aeonpulse.Tests")]

namespace Aeonpulse.Services
{
    /// <summary>
    /// Core domain-logic engine that converts a user-supplied base date into
    /// a collection of richly-formatted <see cref="TickerData"/> objects,
    /// each representing a distinct temporal or cosmological perspective.
    ///
    /// <para>
    /// <b>Hidden dependencies / side effects:</b>
    /// <list type="bullet">
    ///   <item><description>
    ///     All text output is pulled from <see cref="AppResources"/> at call time,
    ///     so the strings automatically reflect whichever culture
    ///     <see cref="ViewModels.MainViewModel.ApplyLanguage"/> has applied to
    ///     <c>AppResources.Culture</c>.
    ///   </description></item>
    ///   <item><description>
    ///     Every method reads <see cref="DateTime.Now"/> internally; they are
    ///     therefore not pure functions and will produce different results on
    ///     every invocation - intentional for live-update scenarios.
    ///   </description></item>
    ///   <item><description>
    ///     No global state is written; this service is stateless and safe to
    ///     call from any thread (the 1-second timer in
    ///     <see cref="ViewModels.MainViewModel"/> marshals calls back to the
    ///     main thread via <c>MainThread.BeginInvokeOnMainThread</c>).
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    [AIContext("CoreCalculationEngine")]
    public class CalculationService
    {
        private const string LogCat = "CALC";

        #region Helper Methods

        /// <summary>
        /// Finds the nearest "jubilee" (a round, memorable milestone) that is
        /// strictly greater than <paramref name="diff"/>.
        ///
        /// <para>
        /// The algorithm searches four jubilee families in order, then returns
        /// the smallest candidate that beats <paramref name="diff"/>:
        /// <list type="number">
        ///   <item><description>Major power-of-10 (10, 100, 1000 …)</description></item>
        ///   <item><description>Minor leading-digit multiple (5, 20, 300 …)</description></item>
        ///   <item><description>Quarter fractions (25, 250, 750 …)</description></item>
        ///   <item><description>Repeating-digit "nice" numbers (111, 2222 …)</description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="diff">The elapsed count (days, weeks, months, etc.) since the base date.</param>
        /// <returns>The smallest jubilee value greater than <paramref name="diff"/>.</returns>
        [AIContext("JubileeSelectionAlgorithm")]
        internal static long FindNearestJubilee(long diff)
        {
            int numOfDigits = diff.ToString().Length;
            long nearestJubilee = long.MaxValue;

            // Find the next major jubilee (10, 100, 1000, etc.)
            long majorJubilee = (long)Math.Pow(10, numOfDigits);
            if (nearestJubilee > majorJubilee)
                nearestJubilee = majorJubilee;

            // Find the next minor jubilee (5, 20, 300, etc.)
            if (diff > 1)
            {
                long minorJubilee = (long)Math.Ceiling((diff + 0.5) / Math.Pow(10, numOfDigits - 1)) * (long)Math.Pow(10, numOfDigits - 1);
                if (nearestJubilee > minorJubilee)
                    nearestJubilee = minorJubilee;
            }

            // Find the next quarter jubilee (25, 750, 5000, etc.)
            if (diff > 10)
            {
                long quarterJubilee = long.MaxValue;
                if (diff < majorJubilee / 4)
                    quarterJubilee = majorJubilee / 4;
                else if (diff < majorJubilee / 2)
                    quarterJubilee = majorJubilee / 2;
                else if (diff < majorJubilee * 3 / 4)
                    quarterJubilee = majorJubilee * 3 / 4;

                if (nearestJubilee > quarterJubilee)
                    nearestJubilee = quarterJubilee;
            }

            // Find the next "nice" jubilee with same digits (111, 2222, etc.)
            if (diff > 10)
            {
                long baseNumber = (long)Math.Ceiling(diff / Math.Pow(10, numOfDigits - 1));
                string repeatedDigits = baseNumber.ToString();
                string niceJubileeStr = string.Concat(Enumerable.Repeat(repeatedDigits, numOfDigits));
                if (long.TryParse(niceJubileeStr, out long niceJubilee))
                {
                    if (nearestJubilee > niceJubilee)
                        nearestJubilee = niceJubilee;
                }
            }

            return nearestJubilee;
        }

        /// <summary>
        /// Finds the largest "jubilee" milestone that is strictly less than or equal to
        /// <paramref name="current"/>, using the same four jubilee families as
        /// <see cref="FindNearestJubilee"/>.
        ///
        /// <para>
        /// Used by <see cref="CalculateTimeJubilees"/> to identify the last milestone
        /// the user has already passed, anchoring the left end of the timeline graphic.
        /// </para>
        /// </summary>
        /// <param name="current">The elapsed count since the base date.</param>
        /// <returns>The largest jubilee value less than or equal to <paramref name="current"/>, or 0 when none exists.</returns>
        [AIContext("JubileeSelectionAlgorithm")]
        internal static long FindPreviousJubilee(long current)
        {
            if (current <= 0)
                return 0;

            long best = 0;
            int numDigits = current.ToString().Length;

            for (int mag = 1; mag <= numDigits + 1; mag++)
            {
                long scale = (long)System.Math.Pow(10, mag - 1);
                long pow10 = scale * 10;

                // Power-of-10 candidates
                if (pow10 <= current && pow10 > best)
                    best = pow10;
                if (scale <= current && scale > best)
                    best = scale;

                // Minor multiples of scale
                for (long mult = 2; mult <= 9; mult++)
                {
                    long candidate = mult * scale;
                    if (candidate <= current && candidate > best)
                        best = candidate;
                }

                // Quarter fractions of the next power of 10
                long p10 = (long)System.Math.Pow(10, mag);
                long q1 = p10 / 4, q2 = p10 / 2, q3 = 3 * p10 / 4;
                if (q1 > 0 && q1 <= current && q1 > best) best = q1;
                if (q2 > 0 && q2 <= current && q2 > best) best = q2;
                if (q3 > 0 && q3 <= current && q3 > best) best = q3;

                // Repeating-digit candidates (1, 11, 111, ...; 2, 22, ...)
                for (int digit = 1; digit <= 9; digit++)
                {
                    string repeated = new string((char)('0' + digit), mag);
                    if (long.TryParse(repeated, out long niceVal) && niceVal <= current && niceVal > best)
                        best = niceVal;
                }
            }

            return best;
        }

        /// <summary>
        /// Repeatedly sums the decimal digits of <paramref name="num"/> until the result
        /// is a single digit (1–9). This is the standard numerology "digital root" operation.
        /// </summary>
        /// <param name="num">A non-negative integer.</param>
        /// <returns>A single digit in the range 1–9.</returns>
        internal static int ReduceToSingleDigit(int num)
        {
            while (num > 9)
            {
                num = num.ToString().Sum(c => c - '0');
            }
            return num;
        }

        /// <summary>
        /// Calculates the total number of breaths taken over <paramref name="totalSeconds"/>
        /// using the NCBI-sourced average rate of 14 breaths per minute
        /// (midpoint of the normal adult resting range of 12-16 breaths/min).
        ///
        /// <para>
        /// This helper is the single source of truth for the breath rate used by both
        /// <see cref="CalculateLifeOdometer"/> and <see cref="CalculateYourBreath"/>,
        /// ensuring the two tickers always agree.
        /// </para>
        /// </summary>
        /// <param name="totalSeconds">Elapsed time in seconds.</param>
        /// <returns>Total breath count as a whole number.</returns>
        private static long CalculateBreaths(double totalSeconds)
            => (long)((totalSeconds / 60.0) * 14.0);

        /// <summary>
        /// Returns the estimated global human population on <paramref name="date"/> using
        /// a 3-epoch piecewise linear model anchored to UN demographic reference points.
        ///
        /// <para>
        /// <b>Epochs (all anchored to 1900-01-01 UTC):</b>
        /// <list type="bullet">
        ///   <item><description>Before 1950 (days less than 18262): linear from 1,656,000,000 to 2,499,000,000.</description></item>
        ///   <item><description>1950-2000 (days 18262-36524): linear from 2,499,000,000 to 6,149,000,000.</description></item>
        ///   <item><description>After 2000 (days greater than 36525): linear from 6,149,000,000 at 8,036-day rate to 7,963,500,000.</description></item>
        /// </list>
        /// Used by both <see cref="CalculateGlobalCrowd"/> and <see cref="CalculateVibrantHumanity"/>
        /// to ensure mathematical consistency.
        /// </para>
        /// </summary>
        /// <param name="date">The date for which to estimate the global population (UTC recommended).</param>
        /// <returns>Estimated population as a non-negative double.</returns>
        public double HumanPopulationByDate(DateTime date)
        {
            double days = (double)(date - new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds / 86400.0;
            double estimatedPopulation;

            if (days < 18262) // before 1950
                estimatedPopulation = days * (2499000000.0 - 1656000000.0) / 18262.0 + 1656000000.0;
            else if (days < 36525) // before 2000
                estimatedPopulation = (days - 18262) * (6149000000.0 - 2499000000.0) / 18263.0 + 2499000000.0;
            else // after 2000
                estimatedPopulation = (days - 36525) * (7963500000.0 - 6149000000.0) / 8036.0 + 6149000000.0;

            return estimatedPopulation;
        }

        /// <summary>
        /// Returns the estimated cumulative number of humans ever born up to <paramref name="date"/>
        /// using a 3-epoch piecewise linear model anchored to PRB demographic reference points.
        ///
        /// <para>
        /// <b>Epochs (all anchored to 1900-01-01 UTC):</b>
        /// <list type="bullet">
        ///   <item><description>Before 1950 (days less than 18262): linear from rank 104,510,976,956 to 107,901,175,171.</description></item>
        ///   <item><description>1950-2000 (days 18262-36524): linear from 107,901,175,171 to 113,966,170,055.</description></item>
        ///   <item><description>After 2000 (days greater than 36525): linear from 113,966,170,055 at 8,036-day rate to 117,020,448,575.</description></item>
        /// </list>
        /// Used by both <see cref="CalculateHumanBirthRank"/> and <see cref="CalculateVibrantHumanity"/>
        /// to ensure mathematical consistency across both tickers.
        /// </para>
        /// </summary>
        /// <param name="date">The date for which to estimate the cumulative birth count (UTC recommended).</param>
        /// <returns>Estimated cumulative births as a non-negative double.</returns>
        public double HumanBirthRankbyDate(DateTime date)
        {
            double days = (double)(date - new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds / 86400.0;
            double estimatedRank;

            if (days < 18262) // before 1950
                estimatedRank = days * (107901175171.0 - 104510976956.0) / 18262.0 + 104510976956.0;
            else if (days < 36525) // before 2000
                estimatedRank = (days - 18262) * (113966170055.0 - 107901175171.0) / 18263.0 + 107901175171.0;
            else // after 2000
                estimatedRank = (days - 36525) * (117020448575.0 - 113966170055.0) / 8036.0 + 113966170055.0;

            return estimatedRank;
        }

        /// <summary>
        /// Returns the estimated cumulative number of humans who have died up to <paramref name="date"/>,
        /// calculated as the difference between cumulative births and current population.
        ///
        /// <para>
        /// <b>Formula:</b> TotalDeaths = <see cref="HumanBirthRankbyDate"/> - <see cref="HumanPopulationByDate"/>.
        /// Used by <see cref="CalculateVibrantHumanity"/> for the deaths-between-dates statistic.
        /// </para>
        /// </summary>
        /// <param name="date">The date for which to estimate cumulative deaths (UTC recommended).</param>
        /// <returns>Estimated cumulative deaths as a non-negative double.</returns>
        public double TotalDeathsByDate(DateTime date)
            => HumanBirthRankbyDate(date) - HumanPopulationByDate(date);

        /// <summary>
        /// Returns the raw historical (Year, EverBorn) data points for the
        /// cumulative-births history polyline, plus the interpolated birth year
        /// for the user marker.
        ///
        /// <para>
        /// Raw year/EverBorn values are returned so the drawable can apply any
        /// axis mapping (currently linear X in [-5000, 2050], linear Y in
        /// [0, 125,000,000,000]) without re-computing from proportional values.
        /// </para>
        /// <para>
        /// <b>Marker year</b> is found by linearly interpolating between the two
        /// bracketing historical data points that straddle
        /// <paramref name="estimatedRank"/>.
        /// </para>
        /// </summary>
        /// <param name="estimatedRank">User estimated birth rank from the piecewise model.</param>
        /// <returns>
        /// Tuple of: the raw data point list (Year AD, EverBorn) and the
        /// interpolated <c>MarkerYear</c> (AD). MarkerYear is NaN when rank is 0.
        /// </returns>
        internal static (IReadOnlyList<(double Year, double EverBorn)> Points, double MarkerYear)
            BirthRankChartPoints(double estimatedRank)
        {
            // Historical data: (Year AD, cumulative humans ever born).
            // Source: PRB "How Many People Have Ever Lived on Earth?" (2024 revision).
            // Negative years = BC. Same dataset as CalculateHumanBirthRank comment block.
            var data = new (double Year, double EverBorn)[]
            {
                (-190000,            0),
                ( -50000,  7_856_100_002),
                (  -8000,  8_993_889_771),
                (      1, 55_019_222_125),
                (   1200, 81_610_565_125),
                (   1650, 94_392_567_578),
                (   1750, 97_564_499_091),
                (   1850, 101_610_739_100),
                (   1900, 104_510_976_956),
                (   1950, 107_901_175_171),
                (   2000, 113_966_170_055),
                (   2010, 115_330_173_460),
                (   2022, 117_020_448_575),
                (   2035, 118_779_027_464),
                (   2050, 120_847_437_072),
            };

            if (estimatedRank <= 0)
                return (data, double.NaN);

            // Find bracketing data points for marker year interpolation.
            double markerYear = double.NaN;
            for (int i = 1; i < data.Length; i++)
            {
                if (estimatedRank <= data[i].EverBorn)
                {
                    double span = data[i].EverBorn - data[i - 1].EverBorn;
                    double t = span > 0
                        ? (estimatedRank - data[i - 1].EverBorn) / span
                        : 0;
                    markerYear = data[i - 1].Year + t * (data[i].Year - data[i - 1].Year);
                    break;
                }
            }
            // If rank exceeds all data points, clamp to the last point year.
            if (double.IsNaN(markerYear))
                markerYear = data[data.Length - 1].Year;

            return (data, markerYear);
        }

        #endregion

        #region Time Jubilees

        /// <summary>
        /// Calculates the nearest time jubilee milestone by building a comprehensive
        /// flat list of all candidate milestone <see cref="DateTime"/> values across
        /// three milestone families (Classical Years, Power-of-Ten Days/Hours, Patterned
        /// Numerals for Days/Hours), sorting them chronologically, then selecting the
        /// single date strictly before <paramref name="now"/> (Last Jubilee) and the
        /// single date strictly after (Next Jubilee).
        ///
        /// <para>
        /// <b>Algorithm:</b>
        /// <list type="number">
        ///   <item><description>
        ///     Build candidate list: Classical Year milestones (5, 10, 15, 20, 25, 30,
        ///     40, 50, 60, 70, 75, 80, 90, 100, 110, 120, 125, 150, 175, 200 years);
        ///     Power-of-Ten Days (100, 1000, 5000, 10000, ...); Repeating-digit Days
        ///     (1111, 2222, ..., 11111, 22222, ...); Power-of-Ten Hours (1000, 10000,
        ///     100000, 500000, 1000000); Repeating-digit Hours (11111, 22222, ...,
        ///     111111, 222222, ...).
        ///   </description></item>
        ///   <item><description>
        ///     Convert every candidate count to an absolute <see cref="DateTime"/> using
        ///     the appropriate <c>baseDate.AddXxx</c> call. Guard against
        ///     <see cref="ArgumentOutOfRangeException"/> for very large values.
        ///   </description></item>
        ///   <item><description>
        ///     Sort the resulting <c>(date, name)</c> list chronologically.
        ///   </description></item>
        ///   <item><description>
        ///     Scan forward to find the last date strictly before <paramref name="now"/>
        ///     and the first date strictly after <paramref name="now"/>.
        ///   </description></item>
        ///   <item><description>
        ///     Compute <c>ProgressFraction</c> and clamp to <c>[0.05, 0.95]</c> so the
        ///     Today dot never visually overlaps the endpoint dots.
        ///   </description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <b>Side effect:</b> reads <see cref="AppResources"/> for every unit label,
        /// so output language follows <c>AppResources.Culture</c>.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The user-selected origin date (e.g., birthday).</param>
        /// <param name="baseDateName">Human-readable label for <paramref name="baseDate"/> (e.g., "My Birthday").</param>
        /// <param name="baseDateValue">ISO-8601 string representation of <paramref name="baseDate"/>, used in formatted output.</param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>A <see cref="TimeJubileesResult"/> with brief and full descriptions of the nearest jubilee.</returns>
        [AIContext("CoreCalculation")]
        public TimeJubileesResult CalculateTimeJubilees(DateTime baseDate, string baseDateName, string baseDateValue, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            int bYear  = baseDate.Year;
            int bMonth = baseDate.Month;
            int bDay   = baseDate.Day;

            double totalDays    = (now_ - baseDate).TotalDays;
            double totalHours   = (now_ - baseDate).TotalHours;
            AeonLog.Debug(LogCat, nameof(CalculateTimeJubilees), $"baseDate={baseDate:d} totalDays={totalDays:F1}");

            // Build the comprehensive candidate list: (absolute DateTime, display name).
            var candidates = new System.Collections.Generic.List<(DateTime Date, string Name)>();

            // --- Classical Year milestones -------------------------------------------
            int[] classicalYears = { 5, 10, 15, 20, 25, 30, 40, 50, 60, 70, 75, 80, 90,
                                     100, 110, 120, 125, 150, 175, 200 };
            foreach (int y in classicalYears)
            {
                try
                {
                    var d = new DateTime(bYear + y, bMonth, bDay,
                                        baseDate.Hour, baseDate.Minute, baseDate.Second);
                    candidates.Add((d, $"{y:N0} {AppResources.Unit_Years}"));
                    AeonLog.Debug(LogCat, nameof(CalculateTimeJubilees),
                        $"candidate year={y} date={d:d}", "UNIT_SCAN");
                }
                catch (ArgumentOutOfRangeException) { /* skip invalid calendar dates */ }
            }

            // --- Power-of-Ten Day milestones -----------------------------------------
            long[] powDays = { 100, 500, 1000, 2000, 3000, 4000, 5000, 6000, 7000,
                                8000, 9000, 10000, 20000, 25000, 30000, 50000, 100000 };
            foreach (long d in powDays)
            {
                try
                {
                    var dt = baseDate.AddDays(d);
                    candidates.Add((dt, $"{d:N0} {AppResources.Unit_Days}"));
                    AeonLog.Debug(LogCat, nameof(CalculateTimeJubilees),
                        $"candidate days={d} date={dt:d}", "UNIT_SCAN");
                }
                catch (ArgumentOutOfRangeException) { }
            }

            // --- Repeating-digit Day milestones (1111, 2222 ... 9999, 11111 ... 99999) ---
            for (int digits = 4; digits <= 5; digits++)
            {
                for (int digit = 1; digit <= 9; digit++)
                {
                    string rep = new string((char)('0' + digit), digits);
                    if (long.TryParse(rep, out long repVal))
                    {
                        try
                        {
                            var dt = baseDate.AddDays(repVal);
                            candidates.Add((dt, $"{repVal:N0} {AppResources.Unit_Days}"));
                            AeonLog.Debug(LogCat, nameof(CalculateTimeJubilees),
                                $"candidate repdays={repVal} date={dt:d}", "UNIT_SCAN");
                        }
                        catch (ArgumentOutOfRangeException) { }
                    }
                }
            }

            // --- Power-of-Ten Hour milestones ----------------------------------------
            long[] powHours = { 1000, 5000, 10000, 50000, 100000, 500000, 1000000 };
            foreach (long h in powHours)
            {
                try
                {
                    var dt = baseDate.AddHours(h);
                    candidates.Add((dt, $"{h:N0} {AppResources.Unit_Hours}"));
                    AeonLog.Debug(LogCat, nameof(CalculateTimeJubilees),
                        $"candidate hours={h} date={dt:d}", "UNIT_SCAN");
                }
                catch (ArgumentOutOfRangeException) { }
            }

            // --- Repeating-digit Hour milestones (11111, 22222 ... 99999, 111111 ...) ---
            for (int digits = 5; digits <= 6; digits++)
            {
                for (int digit = 1; digit <= 9; digit++)
                {
                    string rep = new string((char)('0' + digit), digits);
                    if (long.TryParse(rep, out long repVal))
                    {
                        try
                        {
                            var dt = baseDate.AddHours(repVal);
                            candidates.Add((dt, $"{repVal:N0} {AppResources.Unit_Hours}"));
                            AeonLog.Debug(LogCat, nameof(CalculateTimeJubilees),
                                $"candidate rephours={repVal} date={dt:d}", "UNIT_SCAN");
                        }
                        catch (ArgumentOutOfRangeException) { }
                    }
                }
            }

            // Remove exact duplicates on date, sort chronologically.
            candidates.Sort((a, b) => a.Date.CompareTo(b.Date));

            // Find Last (strictly before now_) and Next (strictly after now_).
            (DateTime Date, string Name) lastEntry  = (baseDate, $"0 {AppResources.Unit_Years}");
            (DateTime Date, string Name) nextEntry  = (DateTime.MaxValue, string.Empty);
            bool foundLast = false;
            bool foundNext = false;

            foreach (var (date, name) in candidates)
            {
                if (date < now_)
                {
                    lastEntry = (date, name);
                    foundLast = true;
                }
                else if (date > now_ && !foundNext)
                {
                    nextEntry = (date, name);
                    foundNext = true;
                    break;
                }
            }

            // Fallback: if no future milestone found use the last candidate.
            if (!foundNext && candidates.Count > 0)
                nextEntry = candidates[candidates.Count - 1];

            string lastJubileeName = lastEntry.Name;
            string nextJubileeName = nextEntry.Name;
            DateTime lastJubileeDate = lastEntry.Date;
            DateTime nearestJubileeDate = nextEntry.Date;

            int daysSinceLast = foundLast
                ? Math.Max(0, (int)(now_ - lastJubileeDate).TotalDays)
                : (int)Math.Max(0, totalDays);
            long daysTillNext = foundNext
                ? Math.Max(0, (long)(nearestJubileeDate - now_).TotalDays)
                : 0L;

            int totalSpan = daysSinceLast + (int)daysTillNext;
            double progressFraction = totalSpan > 0
                ? Math.Clamp((double)daysSinceLast / totalSpan, 0.05, 0.95)
                : 0.5;

            // Derive a jubilee value and unit from the winning next entry name
            // (needed for BriefText and FullText token replacement).
            string nextJubilee = nextJubileeName;

            AeonLog.Debug(LogCat, nameof(CalculateTimeJubilees),
                $"last={lastJubileeName} next={nextJubileeName} daysSinceLast={daysSinceLast} daysTillNext={daysTillNext} progress={progressFraction:F3}", "WINNER");

            return new TimeJubileesResult
            {
                JubileeValue        = daysTillNext,
                JubileeUnit         = string.Empty,
                JubileeDate         = nearestJubileeDate,
                DaysUntil           = daysTillNext,
                IllustrationSource  = "img_timejubilees.png",
                LastJubileeValue    = daysSinceLast,
                LastJubileeUnit     = string.Empty,
                LastJubileeDate     = lastJubileeDate,
                LastJubileeName     = lastJubileeName,
                NextJubileeName     = nextJubileeName,
                DaysSinceLast       = daysSinceLast,
                DaysTillNext        = (int)daysTillNext,
                ProgressFraction    = progressFraction,
                BriefText = AppResources.Ticker_TimeJubileesBrief
                    .Replace("{nextJubilee}", nextJubilee)
                    .Replace("{nearestJubileeDate:d}", nearestJubileeDate.ToString("d")),
                FullText = AppResources.Ticker_TimeJubileesFull
                    .Replace("{baseDateName}", baseDateName)
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{nextJubilee}", nextJubilee)
                    .Replace("{nearestJubileeDate:d}", nearestJubileeDate.ToString("d"))
            };
        }

        #endregion

        #region Countdown

        /// <summary>
        /// Computes a countdown to the next calendar anniversary of <paramref name="baseDate"/>
        /// in the current year (or the following year if the anniversary has already passed).
        ///
        /// <para>
        /// The display format adapts to the remaining time:
        /// <list type="bullet">
        ///   <item><description>Less than 1 day -> HH:MM:SS only</description></item>
        ///   <item><description>1 day – 1 month -> days + HH:MM</description></item>
        ///   <item><description>More than 1 month -> days only</description></item>
        /// </list>
        /// This is a <b>live ticker</b> - called every second by the
        /// <see cref="ViewModels.MainViewModel"/> timer.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The origin date whose annual anniversary is being counted down to.</param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>A <see cref="TickerData"/> with the appropriately scaled countdown strings.</returns>
        [AIContext("LiveTicker")]
        public CountdownResult CalculateCountdown(DateTime baseDate, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            AeonLog.Debug(LogCat, nameof(CalculateCountdown), $"baseDate={baseDate:d}");
            int bYear = baseDate.Year;
            int bMonth = baseDate.Month;
            int bDay = baseDate.Day;
            int nYear = now_.Year;

            // Find next year jubilee for countdown
            DateTime nearest = new DateTime(nYear, bMonth, bDay);
            if (nearest < now_)
                nearest = nearest.AddYears(1);

            long seconds = (long)(nearest - now_).TotalSeconds;
            long days = seconds / 86400;
            long hrs = (seconds - days * 86400) / 3600;
            long mins = (seconds - days * 86400 - hrs * 3600) / 60;
            long secs = seconds % 60;

            string countdown;
            string countdownFull;

            if (seconds < 86400) // less than a day
            {
                countdown = AppResources.Ticker_CountdownBrief_HoursOnly
                    .Replace("{hrs}", hrs.ToString())
                    .Replace("{mins}", mins.ToString())
                    .Replace("{secs}", secs.ToString());
                countdownFull = AppResources.Ticker_CountdownFull_HoursOnly
                    .Replace("{hrs}", hrs.ToString())
                    .Replace("{mins}", mins.ToString())
                    .Replace("{secs}", secs.ToString())
                    .Replace("{nearest:d}", nearest.ToString("d"));
            }
            else // more than a day
            {
                countdownFull = AppResources.Ticker_CountdownFull_WithDays
                    .Replace("{days}", days.ToString())
                    .Replace("{hrs}", hrs.ToString())
                    .Replace("{mins}", mins.ToString())
                    .Replace("{secs}", secs.ToString())
                    .Replace("{nearest:d}", nearest.ToString("d"));
                if (seconds < 2592000) // more than a day but less than a month
                {
                    countdown = AppResources.Ticker_CountdownBrief_DaysHours
                        .Replace("{days}", days.ToString())
                        .Replace("{hrs}", hrs.ToString())
                        .Replace("{mins}", mins.ToString());
                }
                else // more than a month
                {
                    countdown = AppResources.Ticker_CountdownBrief_DaysOnly
                        .Replace("{days}", days.ToString());
                }
            }

            return new CountdownResult
            {
                TotalSeconds    = seconds,
                Days            = days,
                Hours           = hrs,
                Minutes         = mins,
                Secs            = secs,
                AnniversaryDate = nearest,
                IllustrationSource = "anim_countdown.gif",
                BriefText       = countdown,
                FullText        = countdownFull
            };
        }

        #endregion

        #region Life Odometer

        /// <summary>
        /// Estimates the total number of heartbeats and breaths accumulated since
        /// <paramref name="baseDate"/>, using population-average physiological rates:
        /// 70 bpm and 14 breaths/min (NCBI midpoint of the 12-16 resting range).
        ///
        /// <para>
        /// This is a <b>live ticker</b> - called every second by the VM timer.
        /// Results are intentionally approximate; the goal is experiential impact,
        /// not medical precision.
        /// </para>
        /// <para>
        /// <b>Side effect:</b> breath count is computed via
        /// <see cref="CalculateBreaths"/>, the same helper used by
        /// <see cref="CalculateYourBreath"/>, so both tickers always show
        /// the same breath total.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The start of the lifespan being measured.</param>
        /// <param name="baseDateName">Human-readable label for display in the full text.</param>
        /// <param name="baseDateValue">ISO-8601 string for display in the full text.</param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>
        /// A <see cref="TickerData"/> containing formatted heartbeat and breath totals.
        /// </returns>
        [AIContext("LiveTicker")]
        public LifeOdometerResult CalculateLifeOdometer(DateTime baseDate, string baseDateName, string baseDateValue, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            long seconds = (long)(now_ - baseDate).TotalSeconds;
            AeonLog.Debug(LogCat, nameof(CalculateLifeOdometer), $"baseDate={baseDate:d} seconds={seconds}");

            long heartbeats = seconds * 70 / 60;
            long breaths    = CalculateBreaths(seconds);

            return new LifeOdometerResult
            {
                Heartbeats = heartbeats,
                Breaths    = breaths,
                IllustrationSource = "heartbeat.png",
                BriefText = AppResources.Ticker_LifeOdometerBrief
                    .Replace("{heartbeats:N0}", heartbeats.ToString("N0"))
                    .Replace("{breaths:N0}", breaths.ToString("N0")),
                FullText = AppResources.Ticker_LifeOdometerFull
                    .Replace("{heartbeats:N0}", heartbeats.ToString("N0"))
                    .Replace("{breaths:N0}", breaths.ToString("N0"))
                    .Replace("{baseDateName}", baseDateName)
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
            };
        }

        #endregion

        #region Alien Anniversaries

        /// <summary>
        /// Converts the elapsed Earth days since <paramref name="baseDate"/> into
        /// equivalent years on Mercury, Venus, Earth, Mars, and Jupiter,
        /// giving users a playful cross-planetary perspective on their age.
        /// Also computes a fractional orbital progress [0.0, 1.0) for each planet
        /// (0.0 = 12 o'clock / just completed a full orbit, clockwise) used to drive
        /// the orrery visualization in the expanded card view.
        ///
        /// <para>
        /// Planetary year lengths are fixed constants based on orbital periods (NASA
        /// Goddard Planetary Fact Sheet); they do not account for leap-year variations.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The origin date (typically a birthday).</param>
        /// <param name="baseDateName">Human-readable label for display in the full text.</param>
        /// <param name="baseDateValue">ISO-8601 string for display in the full text.</param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>A <see cref="AlienAnniversariesResult"/> with all five planet year figures and orbital fractions.</returns>
        [AIContext("CoreCalculation")]
        public AlienAnniversariesResult CalculateAlienAnniversaries(DateTime baseDate, string baseDateName, string baseDateValue, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            double earthDays = (now_ - baseDate).TotalDays;
            AeonLog.Debug(LogCat, nameof(CalculateAlienAnniversaries), $"baseDate={baseDate:d} earthDays={earthDays:F2}");

            const double mercuryPeriod = 87.97;
            const double venusPeriod   = 224.70;
            const double earthPeriod   = 365.25;
            const double marsPeriod    = 686.98;
            const double jupiterPeriod = 4332.59;

            double mercuryYears  = earthDays / mercuryPeriod;
            double venusYears    = earthDays / venusPeriod;
            double earthYears    = earthDays / earthPeriod;
            double marsYears     = earthDays / marsPeriod;
            double jupiterYears  = earthDays / jupiterPeriod;

            // Fractional orbital progress: the decimal part of the total years elapsed.
            // 0.0 = start of a new orbit (12 o'clock), 0.25 = 3 o'clock, 0.5 = 6 o'clock, 0.75 = 9 o'clock.
            static double Fraction(double years) => years - Math.Floor(years);

            return new AlienAnniversariesResult
            {
                MercuryYears    = mercuryYears,
                MercuryFraction = Fraction(mercuryYears),
                VenusYears      = venusYears,
                VenusFraction   = Fraction(venusYears),
                EarthYears      = earthYears,
                EarthFraction   = Fraction(earthYears),
                MarsYears       = marsYears,
                MarsFraction    = Fraction(marsYears),
                JupiterYears    = jupiterYears,
                JupiterFraction = Fraction(jupiterYears),
                BriefText = AppResources.Ticker_AlienAnniversariesBrief
                    .Replace("{marsYears:F2}", marsYears.ToString("F2"))
                    .Replace("{venusYears:F2}", venusYears.ToString("F2")),
                FullText = AppResources.Ticker_AlienAnniversariesFull
                    .Replace("{baseDateName}", baseDateName)
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{marsYears:F2}", marsYears.ToString("F2"))
                    .Replace("{venusYears:F2}", venusYears.ToString("F2"))
            };
        }

        #endregion

        #region Galactic Commute

        /// <summary>
        /// Calculates the distance the Solar System has travelled through the Milky Way
        /// since <paramref name="baseDate"/>, based on the Sun's galactic orbital velocity
        /// of approximately 225 km/s.
        ///
        /// <para>
        /// <b>Unit toggling:</b> when <paramref name="useMetric"/> is <c>false</c>, all
        /// output distances are converted to miles. The <c>fullDistance</c> parenthetical
        /// is suppressed when the primary unit already gives the raw figure (i.e., when
        /// no scaling prefix like "million" is needed).
        /// </para>
        /// <para>
        /// This is a <b>live ticker</b> - called every second to update the VM timer.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The origin date from which galactic travel is measured.</param>
        /// <param name="baseDateValue">ISO-8601 string for display in the full text.</param>
        /// <param name="useMetric">
        /// <c>true</c> to display kilometres; <c>false</c> to display miles.
        /// Sourced from <see cref="ViewModels.MainViewModel.UseMetric"/>.
        /// </param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>A <see cref="TickerData"/> describing the galactic distance travelled.</returns>
        [AIContext("LiveTicker")]
        public GalacticCommuteResult CalculateGalacticCommute(DateTime baseDate, string baseDateValue, bool useMetric, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            long seconds = (long)(now_ - baseDate).TotalSeconds;
            AeonLog.Debug(LogCat, nameof(CalculateGalacticCommute), $"baseDate={baseDate:d} seconds={seconds} useMetric={useMetric}");

            // Solar system moves at ~220-230 km/s through the galaxy
            double kmTraveled = seconds * 225;

            string distance;
            string fullTextResource = AppResources.Ticker_GalacticCommuteFullWithRounded;
            string fullDistance = $"{kmTraveled:N0} {AppResources.UnitMetric_Km}";
            if (useMetric)
            {
                if (kmTraveled > 1000000000)
                    distance = $"{(kmTraveled / 1000000000):F2} {AppResources.UnitMetric_BKm}";
                else if (kmTraveled > 1000000)
                    distance = $"{(kmTraveled / 1000000):F2} {AppResources.UnitMetric_MKm}";
                else
                {
                    distance = $"{kmTraveled:N0} {AppResources.UnitMetric_Km}";
                    fullTextResource = AppResources.Ticker_GalacticCommuteFull; // no need to show raw km figure in parentheses when it's already in the main distance string
                }
            }
            else
            {
                double miles = kmTraveled * 0.621371;
                fullDistance = $"{miles:N0} {AppResources.UnitImperial_Miles}";
                if (miles > 1000000000)
                    distance = $"{(miles / 1000000000):F2} {AppResources.UnitImperial_BMiles}";
                else if (miles > 1000000)
                    distance = $"{(miles / 1000000):F2} {AppResources.UnitImperial_MMiles}";
                else
                {
                    distance = $"{miles:N0} {AppResources.UnitImperial_Miles}";
                    fullTextResource = AppResources.Ticker_GalacticCommuteFull; // no need to show raw km figure in parentheses when it's already in the main distance string
                }
            }

            return new GalacticCommuteResult
            {
                KmTraveled = kmTraveled,
                Distance   = distance,
                UseMetric  = useMetric,
                BriefText = AppResources.Ticker_GalacticCommuteBrief
                    .Replace("{distance}", distance),
                FullText = fullTextResource
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{distance}", distance)
                    .Replace("{fullDistance}", fullDistance)
            };
        }

        #endregion

        #region Photon Path

        /// <summary>
        /// Determines how far a photon emitted on <paramref name="baseDate"/> would have
        /// travelled by now (light travels at 299,792.458 km/s), then contextualises that
        /// distance against a curated catalogue of named stars ordered by light-year distance.
        ///
        /// <para>
        /// <b>Output narrative phases</b> (driven by <c>lightYears</c> thresholds):
        /// <list type="number">
        ///   <item><description>Still within the Solar System / approaching the Heliopause</description></item>
        ///   <item><description>Within the Oort Cloud (&lt; 1.5 ly)</description></item>
        ///   <item><description>Interstellar space (&lt; 4.246 ly - Proxima Centauri)</description></item>
        ///   <item><description>Past a named star in the catalogue (up to ~139 ly / Achernar)</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The 57-star catalogue is defined inline (anonymous-type array) to keep it
        /// co-located with the logic that consumes it. Star data (name, distance in light-years,
        /// descriptive info) is sourced entirely from <see cref="AppResources"/>.
        /// </para>
        /// <para>
        /// This is a <b>live ticker</b> - called every second by the VM timer.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The origin date from which photon travel is measured.</param>
        /// <param name="baseDateValue">ISO-8601 string for display in the full text.</param>
        /// <param name="useMetric">
        /// <c>true</c> for km-based secondary distance; <c>false</c> for miles.
        /// Sourced from <see cref="ViewModels.MainViewModel.UseMetric"/>.
        /// </param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>
        /// A <see cref="TickerData"/> whose narrative describes which cosmic region the
        /// photon has reached, or which star it has most recently passed.
        /// </returns>
        [AIContext("LiveTicker")]
        [AIContext("StarCatalogueLookup")]
        public PhotonPathResult CalculatePhotonPath(DateTime baseDate, string baseDateValue, bool useMetric, DateTime? now = null)
        {
            var stars = new[]
            {
                new { Name = AppResources.Star_ProximaCentauri_Name,  Ly =  4.246d,  Info = AppResources.Star_ProximaCentauri_Info },
                new { Name = AppResources.Star_AlphaCentauri_Name,    Ly =  4.321d,  Info = AppResources.Star_AlphaCentauri_Info },
                new { Name = AppResources.Star_BarnardsStarName,      Ly =  5.963d,  Info = AppResources.Star_BarnardsStarInfo },
                new { Name = AppResources.Star_Luhman16_Name,         Ly =  6.5d,    Info = AppResources.Star_Luhman16_Info },
                new { Name = AppResources.Star_Lalande21185_Name,     Ly =  8.29d,   Info = AppResources.Star_Lalande21185_Info },
                new { Name = AppResources.Star_Sirius_Name,           Ly =  8.71d,   Info = AppResources.Star_Sirius_Info },
                new { Name = AppResources.Star_EpsilonEridani_Name,   Ly =  10.47d,  Info = AppResources.Star_EpsilonEridani_Info },
                new { Name = AppResources.Star_Procyon_Name,          Ly =  11.46d,  Info = AppResources.Star_Procyon_Info },
                new { Name = AppResources.Star_61Cygni_Name,          Ly =  11.4d,   Info = AppResources.Star_61Cygni_Info },
                new { Name = AppResources.Star_EpsilonIndi_Name,      Ly =  11.87d,  Info = AppResources.Star_EpsilonIndi_Info },
                new { Name = AppResources.Star_TauCeti_Name,          Ly =  11.91d,  Info = AppResources.Star_TauCeti_Info },
                new { Name = AppResources.Star_Groombridge1618_Name,  Ly =  15.89d,  Info = AppResources.Star_Groombridge1618_Info },
                new { Name = AppResources.Star_Omicron2Eridani_Name,  Ly =  16.33d,  Info = AppResources.Star_Omicron2Eridani_Info },
                new { Name = AppResources.Star_70Ophiuchi_Name,       Ly =  16.71d,  Info = AppResources.Star_70Ophiuchi_Info },
                new { Name = AppResources.Star_Altair_Name,           Ly =  16.73d,  Info = AppResources.Star_Altair_Info },
                new { Name = AppResources.Star_Alsafi_Name,           Ly =  18d,     Info = AppResources.Star_InCepheus_Info },
                new { Name = AppResources.Star_EtaCassiopeiae_Name,   Ly =  19.33d,  Info = AppResources.Star_EtaCassiopeiae_Info },
                new { Name = AppResources.Star_36Ophiuchi_Name,       Ly =  19.5d,   Info = AppResources.Star_36Ophiuchi_Info },
                new { Name = AppResources.Star_DeltaPavonis_Name,     Ly =  19.89d,  Info = AppResources.Star_DeltaPavonis_Info },
                new { Name = AppResources.Star_Vega_Name,             Ly =  25d,     Info = AppResources.Star_Vega_Info },
                new { Name = AppResources.Star_Fomalhaut_Name,        Ly =  25.13d,  Info = AppResources.Star_Fomalhaut_Info },
                new { Name = AppResources.Star_Pollux_Name,           Ly =  33.78d,  Info = AppResources.Star_Pollux_Info },
                new { Name = AppResources.Star_Denebola_Name,         Ly =  35.9d,   Info = AppResources.Star_Denebola_Info },
                new { Name = AppResources.Star_Arcturus_Name,         Ly =  36.7d,   Info = AppResources.Star_Arcturus_Info },
                new { Name = AppResources.Star_Capella_Name,          Ly =  42.9d,   Info = AppResources.Star_Capella_Info },
                new { Name = AppResources.Star_Rasalhague_Name,       Ly =  47.8d,   Info = AppResources.Star_Rasalhague_Info },
                new { Name = AppResources.Star_Alderamin_Name,        Ly =  49.1d,   Info = AppResources.Star_Alderamin_Info },
                new { Name = AppResources.Star_Castor_Name,           Ly =  51.6d,   Info = AppResources.Star_Castor_Info },
                new { Name = AppResources.Star_Caph_Name,             Ly =  53.1d,   Info = AppResources.Star_Caph_Info },
                new { Name = AppResources.Star_Menkent_Name,          Ly =  58.8d,   Info = AppResources.Star_InCentaurus_Info },
                new { Name = AppResources.Star_Aldebaran_Name,        Ly =  65.1d,   Info = AppResources.Star_Aldebaran_Info },
                new { Name = AppResources.Star_Larawag_Name,          Ly =  66d,     Info = AppResources.Star_InAuriga_Info },
                new { Name = AppResources.Star_Hamal_Name,            Ly =  66.3d,   Info = AppResources.Star_Hamal_Info },
                new { Name = AppResources.Star_Aljanah_Name,          Ly =  72d,     Info = AppResources.Star_InCepheus_Info },
                new { Name = AppResources.Star_Alphecca_Name,         Ly =  75d,     Info = AppResources.Star_Alphecca_Info },
                new { Name = AppResources.Star_Ankaa_Name,            Ly =  77d,     Info = AppResources.Star_Ankaa_Info },
                new { Name = AppResources.Star_Merak_Name,            Ly =  79.1d,   Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = AppResources.Star_Regulus_Name,          Ly =  79.3d,   Info = AppResources.Star_Regulus_Info },
                new { Name = AppResources.Star_Alsephina_Name,        Ly =  80.6d,   Info = AppResources.Star_InCentaurus_Info },
                new { Name = AppResources.Star_Menkalinan_Name,       Ly =  81.1d,   Info = AppResources.Star_InAuriga_Info },
                new { Name = AppResources.Star_Alioth_Name,           Ly =  82.6d,   Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = AppResources.Star_Mizar_Name,            Ly =  83d,     Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = AppResources.Star_Phecda_Name,           Ly =  83.2d,   Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = AppResources.Star_Sabik_Name,            Ly =  88d,     Info = AppResources.Star_Sabik_Info },
                new { Name = AppResources.Star_Gacrux_Name,           Ly =  88.6d,   Info = AppResources.Star_Gacrux_Info },
                new { Name = AppResources.Star_Algol_Name,            Ly =  94d,     Info = AppResources.Star_Algol_Info },
                new { Name = AppResources.Star_Diphda_Name,           Ly =  96.3d,   Info = AppResources.Star_Diphda_Info },
                new { Name = AppResources.Star_Alpheratz_Name,        Ly =  97d,     Info = AppResources.Star_Alpheratz_Info },
                new { Name = AppResources.Star_Alnair_Name,           Ly =  101d,    Info = AppResources.Star_Alnair_Info },
                new { Name = AppResources.Star_Alkaid_Name,           Ly =  103.9d,  Info = AppResources.Star_Alkaid_Info },
                new { Name = AppResources.Star_Alhena_Name,           Ly =  109d,    Info = AppResources.Star_Alhena_Info },
                new { Name = AppResources.Star_Miaplacidus_Name,      Ly =  113.2d,  Info = AppResources.Star_Miaplacidus_Info },
                new { Name = AppResources.Star_Dubhe_Name,            Ly =  123d,    Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = AppResources.Star_Muhlifain_Name,        Ly =  130d,    Info = AppResources.Star_InCepheus_Info },
                new { Name = AppResources.Star_Algieba_Name,          Ly =  130.3d,  Info = AppResources.Star_Algieba_Info },
                new { Name = AppResources.Star_Kochab_Name,           Ly =  130.9d,  Info = AppResources.Star_Kochab_Info },
                new { Name = AppResources.Star_Elnath_Name,           Ly =  134d,    Info = AppResources.Star_Elnath_Info },
                new { Name = AppResources.Star_Achernar_Name,         Ly =  139d,    Info = AppResources.Star_Achernar_Info }
            };

            DateTime now_ = now ?? DateTime.Now;
            long seconds = (long)(now_ - baseDate).TotalSeconds;
            AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"baseDate={baseDate:d} seconds={seconds}", "INPUT");

            // Light travels at 299,792 km/s
            double kmTraveled = seconds * 299792.458;
            double lightYears = kmTraveled / 9460730472580.8;
            AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"ly={lightYears:F4} km={kmTraveled:N0}", "DISTANCE");

            string distance = $"{lightYears:F2} {AppResources.Unit_LightYears}";
            string fullDistance = useMetric ? $"{(kmTraveled / 1000000):N2} {AppResources.UnitMetric_MKm}" : $"{(kmTraveled * 0.621371 / 1000000):N2} {AppResources.UnitImperial_MMiles}";

            string bText = "";
            string fText = "";
            var phase    = PhotonPhase.SolarSystem;
            string? starName = null;
            double  starLy   = 0d;

            if (lightYears < 0.00237188)
            {
                if (kmTraveled > 11000000000)
                {
                    phase = PhotonPhase.Heliopause;
                    AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"phase=Heliopause km={kmTraveled:N0}", "PHASE_LOOKUP");
                    bText = AppResources.Ticker_PhotonPathHeliopause_Brief;
                    fText = AppResources.Ticker_PhotonPathHeliopause_Full
                        .Replace("{baseDate:d}", baseDate.ToString("d"))
                        .Replace("{fullDistance}", fullDistance);
                }
                else
                {
                    phase = PhotonPhase.SolarSystem;
                    AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"phase=SolarSystem km={kmTraveled:N0}", "PHASE_LOOKUP");
                    bText = AppResources.Ticker_PhotonPathSolarSystem_Brief;
                    fText = AppResources.Ticker_PhotonPathSolarSystem_Full
                        .Replace("{baseDate:d}", baseDate.ToString("d"))
                        .Replace("{fullDistance}", fullDistance);
                }
            }
            else if (lightYears < 1.5)
            {
                phase = PhotonPhase.OortCloud;
                AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"phase=OortCloud ly={lightYears:F4}", "PHASE_LOOKUP");
                bText = AppResources.Ticker_PhotonPathOortCloud_Brief;
                fText = AppResources.Ticker_PhotonPathOortCloud_Full
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{fullDistance}", fullDistance);
            }
            else if (lightYears < 4.246)
            {
                phase = PhotonPhase.Interstellar;
                AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"phase=Interstellar ly={lightYears:F4}", "PHASE_LOOKUP");
                bText = AppResources.Ticker_PhotonPathInterstellar_Brief;
                fText = AppResources.Ticker_PhotonPathInterstellar_Full
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{distance}", distance)
                    .Replace("{fullDistance}", fullDistance);
            }
            else
            {
                phase = PhotonPhase.PastStar;
                AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"phase=PastStar ly={lightYears:F4} scanning {stars.Length} stars", "PHASE_LOOKUP");
                foreach (var star in stars)
                {
                    if (lightYears < star.Ly)
                        break;
                    starName = star.Name;
                    starLy   = star.Ly;
                    AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"star={star.Name} starLy={star.Ly}", "STAR_MATCH");
                    bText = AppResources.Ticker_PhotonPathStar_BriefTemplate
                        .Replace("{star.Name}", star.Name);
                    fText = AppResources.Ticker_PhotonPathStar_FullTemplate
                        .Replace("{baseDate:d}", baseDate.ToString("d"))
                        .Replace("{distance}", distance)
                        .Replace("{fullDistance}", fullDistance)
                        .Replace("{star.Name}", star.Name)
                        .Replace("{star.Ly}", star.Ly.ToString("F2"))
                        .Replace("{star.Info}", star.Info);
                }
            }

            // Compute proportional track fields: find the next star ahead of current position.
            // For pre-PastStar phases the next target is always Proxima Centauri (stars[0], 4.246 ly).
            // In PastStar phase the next target is the first star whose Ly > lightYears.
            string nextStarName = string.Empty;
            double nextStarDistance = 0d;
            double totalDistancePassed = lightYears;
            double originLy = 0d;  // distance of the last-passed milestone (Sun = 0, or last star Ly)

            if (phase == PhotonPhase.Interstellar || phase == PhotonPhase.OortCloud
             || phase == PhotonPhase.Heliopause   || phase == PhotonPhase.SolarSystem)
            {
                // Next target is always Proxima Centauri
                nextStarName     = stars[0].Name;
                nextStarDistance = stars[0].Ly;
                originLy         = 0d;
            }
            else if (phase == PhotonPhase.PastStar)
            {
                // Find the next star ahead
                originLy = starLy;  // last passed star is the origin
                for (int si = 0; si < stars.Length; si++)
                {
                    if (lightYears < stars[si].Ly)
                    {
                        nextStarName     = stars[si].Name;
                        nextStarDistance = stars[si].Ly;
                        break;
                    }
                }
                // If past the last catalogued star, keep empty/zero (no next target)
            }

            double progressFraction = 0d;
            double distanceLeft = 0d;
            string nextStopText = string.Empty;

            if (nextStarDistance > 0d)
            {
                double span = nextStarDistance - originLy;
                double traveled = lightYears - originLy;
                progressFraction = span > 0d ? Math.Clamp(traveled / span, 0d, 1d) : 0d;
                distanceLeft     = Math.Max(0d, nextStarDistance - lightYears);
                nextStopText = AppResources.Ticker_PhotonPathNextStop
                    .Replace("{nextStarName}", nextStarName)
                    .Replace("{distanceLeft}", distanceLeft.ToString("F3"));
            }

            AeonLog.Debug(LogCat, nameof(CalculatePhotonPath), $"phase={phase} starName={starName ?? "null"} ly={lightYears:F4} nextStar={nextStarName} progress={progressFraction:F3}", "RESULT");
            return new PhotonPathResult
            {
                KmTraveled           = kmTraveled,
                LightYears           = lightYears,
                Phase                = phase,
                StarName             = starName,
                StarLy               = starLy,
                UseMetric            = useMetric,
                BriefText            = bText,
                FullText             = fText,
                NextStarName         = nextStarName,
                NextStarDistance     = nextStarDistance,
                TotalDistancePassed  = lightYears,
                DistanceLeft         = distanceLeft,
                ProgressFraction     = progressFraction,
                NextStopText         = nextStopText
            };
        }

        #endregion

        #region Human Birth Rank

        /// <summary>
        /// Estimates the approximate ordinal birth rank of a person born on
        /// <paramref name="baseDate"/> - i.e., roughly the N-th human to have
        /// ever been born on Earth.
        ///
        /// <para>
        /// <b>Data source:</b> "How Many People Have Ever Lived on Earth?" by
        /// Toshiko Kaneda &amp; Carl Haub (Population Reference Bureau), cross-referenced
        /// with UN World Population Prospects 2024 and the Human Mortality Database 2025.
        /// </para>
        /// <para>
        /// <b>Algorithm:</b> Three piecewise-linear interpolations are used for
        /// pre-1950, 1950–2000, and post-2000 ranges, reflecting the dramatically
        /// different birth-rate growth trajectories in each era.
        /// Returns a "pre-20th century" fallback for dates before 1900-01-01.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The birth date to rank.</param>
        /// <param name="baseDateName">Human-readable label for display in the full text.</param>
        /// <returns>
        /// A <see cref="TickerData"/> with the estimated birth rank, or a generic
        /// pre-20th-century message for very old dates.
        /// </returns>
        [AIContext("CoreCalculation")]
        [AIContext("ExternalDataModel")]
        public HumanBirthRankResult CalculateHumanBirthRank(DateTime baseDate, string baseDateName)
        {
            AeonLog.Debug(LogCat, nameof(CalculateHumanBirthRank), $"baseDate={baseDate:d}");
            /* Data from "How Many People Have Ever Lived on Earth?" by Toshiko Kaneda & Carl Haub
               from Population Reference Bureau (PRB) (https://www.prb.org/articles/how-many-people-have-ever-lived-on-earth/)
               derived from "World Fertility Data" of the United Nations (https://www.un.org/development/desa/pd/world-fertility-data),
               "Historical estimates by Human Mortality Database (2025)" (https://www.mortality.org/), and
               "World Population Prospects 2024" of the United Nations (https://population.un.org/wpp/).

               Year |    Population | Number Ever Born
            -190000 |             2 |               0
             -50000 |     2,000,000 |   7,856,100,002
              -8000 |     5,000,000 |   8,993,889,771
                  1 |   300,000,000 |  55,019,222,125
               1200 |   450,000,000 |  81,610,565,125
               1650 |   500,000,000 |  94,392,567,578
               1750 |   795,000,000 |  97,564,499,091
               1850 | 1,265,000,000 | 101,610,739,100
               1900 | 1,656,000,000 | 104,510,976,956
               1950 | 2,499,000,000 | 107,901,175,171
               2000 | 6,149,000,000 | 113,966,170,055
               2010 | 6,986,000,000 | 115,330,173,460
               2022 | 7,963,500,000 | 117,020,448,575
               2035 | 8,899,000,000 | 118,779,027,464
               2050 | 9,752,000,000 | 120,847,437,072
            */

            // Pre-1900 dates are outside the piecewise model range
            long days = (long)(baseDate - new DateTime(1900, 1, 1)).TotalDays;
            if (days < 0)
            {
                return new HumanBirthRankResult
                {
                    IsPreTwentiethCentury = true,
                    EstimatedRank = 0,
                    BriefText = AppResources.Ticker_HumanBirthRankPreXX_Brief,
                    FullText = AppResources.Ticker_HumanBirthRankPreXX_Full
                        .Replace("{baseDateName}", baseDateName)
                };
            }

            // Use the shared piecewise model for consistent results with CalculateVibrantHumanity
            double estimatedRank = HumanBirthRankbyDate(baseDate.ToUniversalTime());
            var (chartPoints, markerYear) = BirthRankChartPoints(estimatedRank);

            return new HumanBirthRankResult
            {
                IsPreTwentiethCentury = false,
                EstimatedRank = estimatedRank,
                ChartPoints   = chartPoints,
                MarkerYear    = markerYear,
                BriefText = AppResources.Ticker_HumanBirthRankPostXX_Brief
                    .Replace("{estimatedRank:N0}", estimatedRank.ToString("N0")),
                FullText = AppResources.Ticker_HumanBirthRankPostXX_Full
                    .Replace("{baseDateName}", baseDateName)
                    .Replace("{estimatedRank:N0}", estimatedRank.ToString("N0"))
            };
        }

        #endregion

        #region Birth Rune

        /// <summary>
        /// Maps <paramref name="baseDate"/> to one of the 24 Elder Futhark runes using the
        /// traditional Norse runic calendar (Runic Era / Futhark wheel), where each rune
        /// governs roughly a 15-day period of the year.
        ///
        /// <para>
        /// <b>Month encoding:</b> the inline <c>From</c>/<c>To</c> strings use
        /// zero-based month indices (0 = January … 11 = December) to compactly
        /// express cross-year boundaries (e.g., December wraps to "0-" = January).
        /// A <c>+1</c> offset is applied when constructing <see cref="DateTime"/> objects
        /// to convert back to 1-based <see cref="DateTime.Month"/> values.
        /// </para>
        /// <para>
        /// Rune names, symbols, and interpretations are sourced from
        /// <see cref="AppResources"/>, making them fully localisable.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The birth date whose rune is being determined.</param>
        /// <param name="baseDateValue">ISO-8601 string for display in the full text.</param>
        /// <returns>A <see cref="TickerData"/> with the rune name, symbol, and interpretation.</returns>
        [AIContext("CoreCalculation")]
        public BirthRuneResult CalculateBirthRune(DateTime baseDate, string baseDateValue)
        {
            AeonLog.Debug(LogCat, nameof(CalculateBirthRune), $"baseDate={baseDate:d}");
            var runes = new[]
            {
                new { Name = AppResources.Rune_Fehu_Name,     Symbol = "ᚠ", From = "5-29",  To = "6-14",  Brief = AppResources.Rune_Fehu_Brief,     Full = AppResources.Rune_Fehu_Full },
                new { Name = AppResources.Rune_Uruz_Name,     Symbol = "ᚢ", From = "6-14",  To = "6-29",  Brief = AppResources.Rune_Uruz_Brief,     Full = AppResources.Rune_Uruz_Full },
                new { Name = AppResources.Rune_Thurisaz_Name, Symbol = "ᚦ", From = "6-29",  To = "7-13",  Brief = AppResources.Rune_Thurisaz_Brief, Full = AppResources.Rune_Thurisaz_Full },
                new { Name = AppResources.Rune_Ansuz_Name,    Symbol = "ᚨ", From = "7-13",  To = "7-29",  Brief = AppResources.Rune_Ansuz_Brief,    Full = AppResources.Rune_Ansuz_Full },
                new { Name = AppResources.Rune_Raidho_Name,   Symbol = "ᚱ", From = "7-29",  To = "8-13",  Brief = AppResources.Rune_Raidho_Brief,   Full = AppResources.Rune_Raidho_Full },
                new { Name = AppResources.Rune_Kenaz_Name,    Symbol = "ᚲ", From = "8-13",  To = "8-28",  Brief = AppResources.Rune_Kenaz_Brief,    Full = AppResources.Rune_Kenaz_Full },
                new { Name = AppResources.Rune_Gebo_Name,     Symbol = "ᚷ", From = "8-28",  To = "9-13",  Brief = AppResources.Rune_Gebo_Brief,     Full = AppResources.Rune_Gebo_Full },
                new { Name = AppResources.Rune_Wunjo_Name,    Symbol = "ᚹ", From = "9-13",  To = "9-28",  Brief = AppResources.Rune_Wunjo_Brief,    Full = AppResources.Rune_Wunjo_Full },
                new { Name = AppResources.Rune_Hagalaz_Name,  Symbol = "ᚻ", From = "9-28",  To = "10-13", Brief = AppResources.Rune_Hagalaz_Brief,  Full = AppResources.Rune_Hagalaz_Full },
                new { Name = AppResources.Rune_Nauthiz_Name,  Symbol = "ᚾ", From = "10-13", To = "10-28", Brief = AppResources.Rune_Nauthiz_Brief,  Full = AppResources.Rune_Nauthiz_Full },
                new { Name = AppResources.Rune_Isa_Name,      Symbol = "ᛁ", From = "10-28", To = "11-13", Brief = AppResources.Rune_Isa_Brief,      Full = AppResources.Rune_Isa_Full },
                new { Name = AppResources.Rune_Jera_Name,     Symbol = "ᛃ", From = "11-13", To = "11-28", Brief = AppResources.Rune_Jera_Brief,     Full = AppResources.Rune_Jera_Full },
                new { Name = AppResources.Rune_Eihwaz_Name,   Symbol = "ᛇ", From = "11-28", To = "0-13",  Brief = AppResources.Rune_Eihwaz_Brief,   Full = AppResources.Rune_Eihwaz_Full },
                new { Name = AppResources.Rune_Perthro_Name,  Symbol = "ᚹ", From = "0-13",  To = "0-28",  Brief = AppResources.Rune_Perthro_Brief,  Full = AppResources.Rune_Perthro_Full },
                new { Name = AppResources.Rune_Algiz_Name,    Symbol = "ᛉ", From = "0-28",  To = "1-13",  Brief = AppResources.Rune_Algiz_Brief,    Full = AppResources.Rune_Algiz_Full },
                new { Name = AppResources.Rune_Sowilo_Name,   Symbol = "ᛋ", From = "1-13",  To = "1-27",  Brief = AppResources.Rune_Sowilo_Brief,   Full = AppResources.Rune_Sowilo_Full },
                new { Name = AppResources.Rune_Tiwaz_Name,    Symbol = "ᛏ", From = "1-27",  To = "2-14",  Brief = AppResources.Rune_Tiwaz_Brief,    Full = AppResources.Rune_Tiwaz_Full },
                new { Name = AppResources.Rune_Berkano_Name,  Symbol = "ᛒ", From = "2-14",  To = "2-30",  Brief = AppResources.Rune_Berkano_Brief,  Full = AppResources.Rune_Berkano_Full },
                new { Name = AppResources.Rune_Ehwaz_Name,    Symbol = "ᛖ", From = "2-30",  To = "3-14",  Brief = AppResources.Rune_Ehwaz_Brief,    Full = AppResources.Rune_Ehwaz_Full },
                new { Name = AppResources.Rune_Mannaz_Name,   Symbol = "ᛗ", From = "3-14",  To = "3-29",  Brief = AppResources.Rune_Mannaz_Brief,   Full = AppResources.Rune_Mannaz_Full },
                new { Name = AppResources.Rune_Laguz_Name,    Symbol = "ᛚ", From = "3-29",  To = "4-14",  Brief = AppResources.Rune_Laguz_Brief,    Full = AppResources.Rune_Laguz_Full },
                new { Name = AppResources.Rune_Ingwaz_Name,   Symbol = "ᛝ", From = "4-14",  To = "4-29",  Brief = AppResources.Rune_Ingwaz_Brief,   Full = AppResources.Rune_Ingwaz_Full },
                new { Name = AppResources.Rune_Othala_Name,   Symbol = "ᛟ", From = "4-29",  To = "5-14",  Brief = AppResources.Rune_Othala_Brief,   Full = AppResources.Rune_Othala_Full },
                new { Name = AppResources.Rune_Dagaz_Name,    Symbol = "ᛞ", From = "5-14",  To = "5-29",  Brief = AppResources.Rune_Dagaz_Brief,    Full = AppResources.Rune_Dagaz_Full }
            };

            int year = baseDate.Year;
            var birthRune = runes[0];

            foreach (var rune in runes)
            {
                var fromParts = rune.From.Split('-');
                var toParts = rune.To.Split('-');
                // month is "+1" as the original data is 0-based (0-11 for Jan-Dec), but DateTime is 1-based (1-12 for Jan-Dec)
                var runeStart = new DateTime(year, int.Parse(fromParts[0]) + 1, int.Parse(fromParts[1]));
                var runeEnd = new DateTime(year, int.Parse(toParts[0]) + 1, int.Parse(toParts[1]));

                if (baseDate >= runeStart && baseDate < runeEnd)
                {
                    birthRune = rune;
                    break;
                }
            }

            return new BirthRuneResult
            {
                RuneName   = birthRune.Name,
                RuneSymbol = birthRune.Symbol,
                RuneBrief  = birthRune.Brief,
                RuneFull   = birthRune.Full,
                BriefText = AppResources.Ticker_BirthRuneBrief_Template
                    .Replace("{birthRune.Name}", $"{birthRune.Name} ({birthRune.Symbol})")
                    .Replace("{birthRune.Brief}", birthRune.Brief),
                FullText = AppResources.Ticker_BirthRuneFull_Template
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{birthRune.Name}", $"{birthRune.Name} ({birthRune.Symbol})")
                    .Replace("{birthRune.Full}", birthRune.Full)
            };
        }

        #endregion

        #region Personal Year

        /// <summary>
        /// Derives the user's numerological Personal Year number for the current calendar year
        /// by reducing the sum of the current year's digital root, the birth month's digital
        /// root, and the birth day's digital root to a single digit (1–9).
        ///
        /// <para>
        /// <b>Algorithm source:</b> https://numerology.astro-seek.com/personal-year
        /// </para>
        /// <para>
        /// The nine interpretations are stored in <see cref="AppResources"/> (e.g.,
        /// <c>PersonalYear1_Brief</c> … <c>PersonalYear9_Full</c>), making them
        /// fully localisable without code changes.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The birth date used for month and day components.</param>
        /// <param name="baseDateValue">ISO-8601 string for display in the full text.</param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>
        /// A <see cref="TickerData"/> identifying the personal year number (1–9)
        /// and its thematic interpretation.
        /// </returns>
        [AIContext("CoreCalculation")]
        public PersonalYearResult CalculatePersonalYear(DateTime baseDate, string baseDateValue, DateTime? now = null)
        {
            int curYear = (now ?? DateTime.Now).Year;
            AeonLog.Debug(LogCat, nameof(CalculatePersonalYear), $"baseDate={baseDate:d} curYear={curYear}");

            int year  = ReduceToSingleDigit(curYear);
            int month = ReduceToSingleDigit(baseDate.Month);
            int day   = ReduceToSingleDigit(baseDate.Day);

            int personalYear = ReduceToSingleDigit(year + month + day);

            if (personalYear == 0)
                personalYear = 9;

            var interpretations = new[]
            {
                new { Brief = AppResources.PersonalYear1_Brief, Full = AppResources.PersonalYear1_Full },
                new { Brief = AppResources.PersonalYear2_Brief, Full = AppResources.PersonalYear2_Full },
                new { Brief = AppResources.PersonalYear3_Brief, Full = AppResources.PersonalYear3_Full },
                new { Brief = AppResources.PersonalYear4_Brief, Full = AppResources.PersonalYear4_Full },
                new { Brief = AppResources.PersonalYear5_Brief, Full = AppResources.PersonalYear5_Full },
                new { Brief = AppResources.PersonalYear6_Brief, Full = AppResources.PersonalYear6_Full },
                new { Brief = AppResources.PersonalYear7_Brief, Full = AppResources.PersonalYear7_Full },
                new { Brief = AppResources.PersonalYear8_Brief, Full = AppResources.PersonalYear8_Full },
                new { Brief = AppResources.PersonalYear9_Brief, Full = AppResources.PersonalYear9_Full }
            };

            return new PersonalYearResult
            {
                PersonalYearNumber = personalYear,
                CurrentYear        = curYear,
                BriefText = AppResources.Ticker_PersonalYearBrief_Template
                    .Replace("{curYear}", curYear.ToString())
                    .Replace("{personalYear}", personalYear.ToString())
                    .Replace("{interpretations[personalYear - 1].Brief}", interpretations[personalYear - 1].Brief),
                FullText = AppResources.Ticker_PersonalYearFull_Template
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{curYear}", curYear.ToString())
                    .Replace("{personalYear}", personalYear.ToString())
                    .Replace("{interpretations[personalYear - 1].Full}", interpretations[personalYear - 1].Full)
            };
        }

        #endregion

        #region Global Exhale

        /// <summary>
        /// Calculates the cumulative global CO₂ emissions (in billion metric tonnes) that
        /// occurred between <paramref name="baseDate"/> and today, using a polynomial
        /// regression model fitted to the Global Carbon Budget 2025 dataset.
        ///
        /// <para>
        /// <b>Data source:</b> https://globalcarbonbudget.org/datahub/the-latest-gcb-data-2025/
        /// </para>
        /// <para>
        /// <b>Model:</b> CO₂ in year Y (relative to 1900) ≈
        /// <c>0.0008·Y² − 0.0122·Y + 0.6859</c> (polynomial R² &gt; exponential).
        /// The integral of this gives the total cumulative emissions between two years.
        /// </para>
        /// <para>
        /// For dates before 1900, a fixed pre-industrial total of 11.77 billion tonnes
        /// (cumulative to 1900) is returned as a constant.
        /// </para>
        /// <para>
        /// <b>Unit toggling:</b> when <paramref name="useMetric"/> is <c>false</c>,
        /// result is converted from metric tonnes to short tons (×0.984252).
        /// </para>
        /// </summary>
        /// <param name="baseDate">The start date for measuring emissions.</param>
        /// <param name="baseDateName">Human-readable label for display in the full text.</param>
        /// <param name="baseDateValue">ISO-8601 string for display in the full text.</param>
        /// <param name="useMetric">
        /// <c>true</c> for metric tonnes; <c>false</c> for short tons.
        /// Sourced from <see cref="ViewModels.MainViewModel.UseMetric"/>.
        /// </param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>A <see cref="TickerData"/> with the estimated CO₂ emitted since <paramref name="baseDate"/>.</returns>
        [AIContext("CoreCalculation")]
        [AIContext("ExternalDataModel")]
        public GlobalExhaleResult CalculateGlobalExhale(DateTime baseDate, string baseDateName, string baseDateValue, bool useMetric, DateTime? now = null)
        {
            AeonLog.Debug(LogCat, nameof(CalculateGlobalExhale), $"baseDate={baseDate:d} useMetric={useMetric}");
            /* The data is taken from https://globalcarbonbudget.org/datahub/the-latest-gcb-data-2025/
            Year |    CO2/year
            1900 |  0.53572155
            1901 |  0.55284611
            1902 |  0.56685480
            ...
            2022 | 10.24229576
            2023 | 10.39684612
            2024 | 10.53454641
            */

            DateTime year1900 = new DateTime(1900, 1, 1);
            int baseYears = (int)((baseDate - year1900).TotalDays / 365.25);

            double totalCO2 = 11.77; // billion tons of CO2 emitted till 1900
            string amount = useMetric ? $"{totalCO2} {AppResources.UnitMetric_BTonnes}" : $"{(totalCO2 * 0.984252):F2} {AppResources.UnitImperial_BTons}";

            if (baseYears < 0)
            {
                return new GlobalExhaleResult
                {
                    IsPreTwentiethCentury   = true,
                    TotalCO2BillionTonnes   = totalCO2,
                    FormattedAmount         = amount,
                    UseMetric               = useMetric,
                    BriefText = AppResources.Ticker_GlobalExhalePreXX_Brief
                        .Replace("{amount}", amount),
                    FullText = AppResources.Ticker_GlobalExhalePreXX_Full
                        .Replace("{baseDateName}", baseDateName)
                        .Replace("{amount}", amount)
                };
            }

            // Approximation for year >= 1900 (polynomial gives a better R^2 than exponential):
            //    CO2_in_year = 0.0008 * (year - 1900)^2 - 0.0122 * (year - 1900) + 0.6859
            //    Total_CO2_emitted_till_a_date_since_1900_year = 0.0008/3 * (year - 1900)^3 - 0.0122/2 * (year - 1900)^2 + 0.6859 * (year - 1900)
            DateTime now_ = now ?? DateTime.Now;
            int nowYears = (int)((now_ - year1900).TotalDays / 365.25);
            double baseDaysInYear = (baseDate - new DateTime(baseDate.Year, 1, 1)).TotalDays;
            double nowDaysInYear = (now_ - new DateTime(now_.Year, 1, 1)).TotalDays;
            double x1 = baseYears + baseDaysInYear / 365.0;
            double x2 = nowYears + nowDaysInYear / 365.0;

            double totalCO2Base = 0.0008 / 3 * Math.Pow(x1, 3) - 0.0122 / 2 * Math.Pow(x1, 2) + 0.6859 * x1;
            double totalCO2Now = 0.0008 / 3 * Math.Pow(x2, 3) - 0.0122 / 2 * Math.Pow(x2, 2) + 0.6859 * x2;
            totalCO2 = totalCO2Now - totalCO2Base;
            amount = useMetric ? $"{totalCO2:F2} {AppResources.UnitMetric_BTonnes}" : $"{(totalCO2 * 0.984252):F2} {AppResources.UnitImperial_BTons}";

            return new GlobalExhaleResult
            {
                IsPreTwentiethCentury   = false,
                TotalCO2BillionTonnes   = totalCO2,
                FormattedAmount         = amount,
                UseMetric               = useMetric,
                BriefText = AppResources.Ticker_GlobalExhalePostXX_Brief
                    .Replace("{amount}", amount),
                FullText = AppResources.Ticker_GlobalExhalePostXX_Full
                    .Replace("{baseDateName}", baseDateName)
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{amount}", amount)
            };
        }

        #endregion

        #region Your Breath

        /// <summary>
        /// Estimates the cumulative number of breaths taken, total volume of air processed,
        /// and total mass of CO2 exhaled since <paramref name="baseDate"/>.
        ///
        /// <para>
        /// <b>Algorithm:</b>
        /// <list type="bullet">
        ///   <item><description>Breath rate: 14 breaths/min (midpoint of the 12-16 range).</description></item>
        ///   <item><description>Tidal volume: 0.5 litres per breath.</description></item>
        ///   <item><description>CO2 output: 1.04 kg/day.</description></item>
        ///   <item><description>Air volume is always in litres regardless of the active unit system,
        ///     because respiratory physiology uses metric exclusively.</description></item>
        ///   <item><description>CO2 mass respects the active unit system: kg (metric) or lbs (imperial).</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <b>Data source:</b> National Center for Biotechnology Information (NCBI),
        /// National Library of Medicine (NIH).
        /// </para>
        /// <para>
        /// This is a <b>live ticker</b> - called every second by the
        /// <see cref="ViewModels.MainViewModel"/> timer.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The user-selected origin date.</param>
        /// <param name="baseDateValue">ISO-8601 string of the base date for output formatting.</param>
        /// <param name="useMetric">
        /// <c>true</c> for kg (CO2 mass); <c>false</c> for lbs.
        /// Air volume is always litres regardless of this flag.
        /// </param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>A <see cref="YourBreathResult"/> with breath count, air volume, and CO2 mass since the base date.</returns>
        [AIContext("LiveTicker")]
        public YourBreathResult CalculateYourBreath(DateTime baseDate, string baseDateValue, bool useMetric, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            AeonLog.Debug(LogCat, nameof(CalculateYourBreath), $"baseDate={baseDate:d} useMetric={useMetric}");

            double totalSeconds = (now_ - baseDate).TotalSeconds;
            double totalDays    = (now_ - baseDate).TotalDays;

            double breaths   = CalculateBreaths(totalSeconds);
            double co2Kg     = totalDays * 1.04;
            double airLiters = breaths * 0.5;

            // CO2 mass: metric = kg, imperial = lbs (1 kg = 2.20462 lbs)
            double co2Display   = useMetric ? co2Kg : co2Kg * 2.20462;
            string co2Unit      = useMetric ? AppResources.UnitMetric_Kg
                                            : AppResources.UnitImperial_Lbs;

            string co2Formatted = $"{co2Display:N2} {co2Unit}";

            string briefText = AppResources.Ticker_YourBreathBrief
                .Replace("{breath_count}", breaths.ToString("N0"))
                .Replace("{co2_mass}",     co2Formatted);

            string fullText = AppResources.Ticker_YourBreathFull
                .Replace("{breath_count}", breaths.ToString("N0"))
                .Replace("{air_volume}",   airLiters.ToString("N0"))
                .Replace("{co2_mass}",     co2Formatted);

            return new YourBreathResult
            {
                BreathCount = breaths,
                AirLiters   = airLiters,
                Co2Kg       = co2Kg,
                UseMetric   = useMetric,
                BriefText   = briefText,
                FullText    = fullText
            };
        }

        #endregion

        #region Cellular Refresh

        /// <summary>
        /// Estimates the number of times the outer skin layer (epidermis) has been
        /// replaced and the total number of red blood cells generated since
        /// <paramref name="baseDate"/>, based on standard physiological averages.
        ///
        /// <para>
        /// <b>Algorithm:</b>
        /// <list type="bullet">
        ///   <item><description>Skin cycle duration: 27 days (average epidermal renewal period). Formatted as a whole number (N0).</description></item>
        ///   <item><description>RBC production rate: 2,000,000 new red blood cells per second. Displayed in billions (N2) for readability on a mobile screen.</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// <b>Data source:</b> National Center for Biotechnology Information (NCBI),
        /// National Library of Medicine (NIH).
        /// </para>
        /// <para>
        /// This is a <b>static ticker</b> - recalculated on base date change or
        /// via explicit user refresh. Not called by the 1-second timer.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The user-selected origin date.</param>
        /// <param name="baseDateName">Human-readable label for display in the full text.</param>
        /// <param name="baseDateValue">ISO-8601 string for the base date for output formatting.</param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>A <see cref="CellularRefreshResult"/> with skin cycle count and total RBCs (in billions) since the base date.</returns>
        [AIContext("CoreCalculation")]
        public CellularRefreshResult CalculateCellularRefresh(DateTime baseDate, string baseDateName, string baseDateValue, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            AeonLog.Debug(LogCat, nameof(CalculateCellularRefresh), $"baseDate={baseDate:d}");

            double totalSeconds = (now_ - baseDate).TotalSeconds;
            double totalDays    = (now_ - baseDate).TotalDays;

            double skinCycles        = totalDays / 27.0;
            double totalRbcsCreated  = totalSeconds * 2000000.0;
            double totalRbcsBillions = totalRbcsCreated / 1_000_000_000.0;

            string unitBillion = AppResources.Unit_Billion;

            string briefText = AppResources.Ticker_CellularRefreshBrief
                .Replace("{skin_count}", skinCycles.ToString("N0"))
                .Replace("{rbc_count_billions}", totalRbcsBillions.ToString("N2"))
                .Replace("{unit_billion}", unitBillion);

            string fullText = AppResources.Ticker_CellularRefreshFull
                .Replace("{skin_count}", skinCycles.ToString("N0"))
                .Replace("{rbc_count_billions}", totalRbcsBillions.ToString("N2"))
                .Replace("{unit_billion}", unitBillion)
                .Replace("{baseDate:d}", baseDate.ToString("d", System.Globalization.CultureInfo.CurrentUICulture));

            return new CellularRefreshResult
            {
                SkinCycles       = skinCycles,
                TotalRbcsCreated = totalRbcsCreated,
                BriefText        = briefText,
                FullText         = fullText
            };
        }

        #endregion

        #region Cosmic Stretch

        /// <summary>
        /// Calculates how much the observable universe has expanded in kilometres
        /// since <paramref name="baseDate"/>, based on the Hubble-Lemaitre Law.
        ///
        /// <para>
        /// <b>Algorithm:</b> the radius of the observable universe is approximately
        /// 46.5 billion light-years. Applying the Hubble Constant (H0 ~ 70 km/s/Mpc),
        /// the expansion rate of that radius is approximately 3,300,000 km/s.
        /// Elapsed seconds since the base date are multiplied by 3,300,000 to yield
        /// the total expansion in kilometres.
        /// </para>
        /// <para>
        /// <b>Scale choice:</b> the result is formatted in <b>million km</b> (dividing by
        /// 1,000,000). At 3.3 million km/s the display increments by 3 every second, which
        /// is the minimum change visible to the eye in a live ticker. Billion km would
        /// increment by 0.003/s and never visibly change; raw km would overflow
        /// any mobile screen. The <c>fullDistance</c> parenthetical is therefore omitted.
        /// </para>
        /// <para>
        /// This is a <b>live ticker</b> - called every second by the VM timer.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The origin date from which expansion is measured.</param>
        /// <param name="baseDateValue">ISO-8601 string for display in the full text.</param>
        /// <param name="useMetric">
        /// <c>true</c> to display kilometres; <c>false</c> to display miles.
        /// Sourced from <see cref="ViewModels.MainViewModel.UseMetric"/>.
        /// </param>
        /// <param name="now">Optional <see cref="DateTime"/> parameter for testing: substitutes an alternate "current time".</param>
        /// <returns>A <see cref="CosmicStretchResult"/> describing the universe expansion since the base date.</returns>
        [AIContext("LiveTicker")]
        public CosmicStretchResult CalculateCosmicStretch(DateTime baseDate, string baseDateValue, bool useMetric, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            double totalSeconds = (now_ - baseDate).TotalSeconds;
            AeonLog.Debug(LogCat, nameof(CalculateCosmicStretch), $"baseDate={baseDate:d} totalSeconds={totalSeconds} useMetric={useMetric}");

            // Observable universe radius expansion rate: ~3,300,000 km/s (Hubble flow)
            double kmExpanded = totalSeconds * 3300000.0;

            // Format in million km/miles: increments by ~3 every second at N0 - visibly live.
            // Billion km increments by 0.003/s and never visibly changes, so it is not used.
            string distance;
            if (useMetric)
            {
                long millionaireKm = (long)(kmExpanded / 1_000_000);
                distance = $"{millionaireKm:N0} {AppResources.UnitMetric_MKm}";
            }
            else
            {
                double miles = kmExpanded * 0.621371;
                long millionaireMiles = (long)(miles / 1_000_000);
                distance = $"{millionaireMiles:N0} {AppResources.UnitImperial_MMiles}";
            }

            return new CosmicStretchResult
            {
                KmExpanded = kmExpanded,
                Distance   = distance,
                UseMetric  = useMetric,
                BriefText = AppResources.Ticker_CosmicStretchBrief
                    .Replace("{distance}", distance),
                FullText = AppResources.Ticker_CosmicStretchFull
                    .Replace("{baseDate:d}", baseDate.ToString("d"))
                    .Replace("{distance}", distance)
            };
        }

        #endregion

        #region Vibrant Cosmos

        /// <summary>
        /// Calculates a continuously updating estimation of how many stars were born
        /// and supernovas appeared in the observable universe since the user's base date.
        ///
        /// <para>
        /// <b>Algorithm:</b> based on standard astronomical estimates:
        /// 4,800 stars are born per second and 30 supernovas occur per second in the
        /// observable universe. Elapsed seconds since the base date are multiplied by
        /// each rate. Both values are formatted with N0 for readability on mobile screens.
        /// </para>
        /// <para>
        /// <b>Live ticker:</b> called every 200 ms by the VM's dedicated Vibrant Cosmos
        /// timer to produce a non-uniform, natural rhythmic pulse in the display.
        /// </para>
        /// <para>
        /// <b>Side effect:</b> reads <see cref="AppResources"/> for output strings so
        /// the result language follows <c>AppResources.Culture</c>.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The origin date from which cosmic activity is measured.</param>
        /// <param name="now">Optional override for the current time; used by unit tests for determinism.</param>
        /// <returns>A <see cref="VibrantCosmosResult"/> with formatted star and supernova counts.</returns>
        [AIContext("LiveTicker")]
        public VibrantCosmosResult CalculateVibrantCosmos(DateTime baseDate, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            double totalSeconds = (now_ - baseDate).TotalSeconds;
            AeonLog.Debug(LogCat, nameof(CalculateVibrantCosmos), $"baseDate={baseDate:d} totalSeconds={totalSeconds}");

            double starsBorn  = totalSeconds * 4800.0;
            double supernovas = totalSeconds * 30.0;

            string starsBornFormatted  = starsBorn < 0  ? "0" : ((long)starsBorn).ToString("N0");
            string supernovasFormatted = supernovas < 0 ? "0" : ((long)supernovas).ToString("N0");

            string briefText = AppResources.Ticker_VibrantCosmosBrief
                .Replace("{stars_born}", starsBornFormatted)
                .Replace("{supernovas}", supernovasFormatted);

            string fullText = AppResources.Ticker_VibrantCosmosFull
                .Replace("{stars_born}", starsBornFormatted)
                .Replace("{supernovas}", supernovasFormatted);

            return new VibrantCosmosResult
            {
                StarsBorn  = starsBorn,
                Supernovas = supernovas,
                BriefText  = briefText,
                FullText   = fullText
            };
        }

        #endregion

        #region Global Crowd

        /// <summary>
        /// Estimates the global human population at <paramref name="baseDate"/> and at the
        /// current moment using a piecewise linear demographic model, then returns the
        /// comparison as a live-updating ticker.
        ///
        /// <para>
        /// <b>Algorithm:</b> three linear segments calibrated to UN demographic estimates:
        /// <list type="bullet">
        ///   <item><description>Before 1900: anchored at 978,000,000 on 1800-01-01, growing 18,398/day.</description></item>
        ///   <item><description>1900-1950: anchored at 1,650,000,000 on 1900-01-01, growing 47,919/day.</description></item>
        ///   <item><description>1950 onward: anchored at 2,525,149,000 on 1950-01-01, growing 203,206/day (approx 2.35/s net).</description></item>
        /// </list>
        /// All epoch anchors use UTC midnight to prevent local timezone shifts from
        /// causing visible population jumps at midnight.
        /// </para>
        /// <para>
        /// <b>Live ticker:</b> called every second by the VM timer. The 1950-onward
        /// segment adds ~2.35 people per second, producing a visibly incrementing count.
        /// </para>
        /// <para>
        /// <b>Side effect:</b> reads <see cref="AppResources"/> for output strings so
        /// the result language follows <c>AppResources.Culture</c>.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The user-selected origin date (e.g., birthday).</param>
        /// <param name="now">Optional override for current time; used by unit tests for determinism.</param>
        /// <returns>
        /// A <see cref="GlobalCrowdResult"/> with formatted population counts at the base
        /// date and at the current moment.
        /// </returns>
        [AIContext("LiveTicker")]
        public GlobalCrowdResult CalculateGlobalCrowd(DateTime baseDate, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.UtcNow;
            AeonLog.Debug(LogCat, nameof(CalculateGlobalCrowd), $"baseDate={baseDate:d}");

            // Use the shared HumanPopulationByDate helper for consistency with CalculateVibrantHumanity
            double currentPopulation = HumanPopulationByDate(now_);
            double basePopulation    = HumanPopulationByDate(baseDate.ToUniversalTime());

            string baseFormatted    = basePopulation    < 0 ? "0" : ((long)basePopulation).ToString("N0");
            string currentFormatted = currentPopulation < 0 ? "0" : ((long)currentPopulation).ToString("N0");

            string briefText = AppResources.Ticker_GlobalCrowdBrief
                .Replace("{base_population}",    baseFormatted)
                .Replace("{current_population}", currentFormatted);

            string fullText = AppResources.Ticker_GlobalCrowdFull
                .Replace("{baseDate:d}",         baseDate.ToString("d", System.Globalization.CultureInfo.CurrentUICulture))
                .Replace("{base_population}",    baseFormatted)
                .Replace("{current_population}", currentFormatted);

            return new GlobalCrowdResult
            {
                BasePopulation    = basePopulation,
                CurrentPopulation = currentPopulation,
                BaseYear          = baseDate.Year,
                CurrentYear       = now_.Year,
                HoverYear         = now_.Year,
                HoverPopulation   = currentPopulation / 1_000_000_000.0,
                BriefText         = briefText,
                FullText          = fullText
            };
        }

        /// <summary>
        /// Returns the estimated global human population on <paramref name="date"/> using a
        /// piecewise linear model anchored to three UN demographic reference points.
        ///
        /// <para>
        /// <b>Segments:</b>
        /// <list type="bullet">
        ///   <item><description>Before 1900-01-01 UTC: base 978,000,000 at 1800-01-01, rate 18,398/day.</description></item>
        ///   <item><description>1900-01-01 to 1950-01-01 UTC: base 1,650,000,000 at 1900-01-01, rate 47,919/day.</description></item>
        ///   <item><description>1950-01-01 onward UTC: base 2,525,149,000 at 1950-01-01, rate 203,206/day (~2.35/s).</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// All epoch anchors use UTC midnight to prevent local timezone shifts from
        /// causing population jumps at midnight.
        /// </para>
        /// </summary>
        /// <param name="date">The date for which to estimate the global population.</param>
        /// <returns>Estimated population as a non-negative double.</returns>
        private double GetPopulationByDate(DateTime date)
        {
            /* Data from "Estimates of historical world population" Wikipedia
               (https://en.wikipedia.org/wiki/Estimates_of_historical_world_population)
               derived from Department of Economic and Social Affairs of the United
               Nations (https://social.desa.un.org/, https://www.un.org/en/desa).

               Year | Population
                  1 | 300000000
               1000 | 310000000
               1250 | 400000000
               1500 | 500000000
               1750 | 791000000
               1800 | 978000000
               1850 | 1262000000
               1900 | 1650000000
               1910 | 1750000000
               1920 | 1860000000
               1930 | 2070000000
               1940 | 2300000000
               1950 | 2525149000
               1951 | 2572850917
               1952 | 2619292068
                ...
               2013 | 7162119434
               2014 | 7243784000
               2015 | 7349472000
            */

            // Use UTC to prevent local timezone shifts from causing population jumps
            DateTime epoch1800 = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime epoch1900 = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime epoch1950 = new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            double population = 0;

            // We'll use 3 different linear approximations for the periods before 1900, between 1900 and 1950, and after 1950,
            // since the growth rate of population has changed significantly in these periods.
            // The estimates won't be perfect, but they should give a reasonable approximation of the population for these dates.
            if (date < epoch1900)
            {
                double days = (date - epoch1800).TotalDays;
                population = days * 18398.0 + 978000000.0;
            }
            else if (date < epoch1950)
            {
                double days = (date - epoch1900).TotalDays;
                population = days * 47919.0 + 1650000000.0;
            }
            else
            {
                double days = (date - epoch1950).TotalDays;
                population = days * 203206.0 + 2525149000.0;
            }

            // Ensure we don't return negative populations for extreme edge cases
            return Math.Max(0, population);
        }

        #endregion

        #region Vibrant Humanity

        /// <summary>
        /// Calculates the estimated number of people born and deceased globally since
        /// the user's base date, including demographic sub-categories such as twins born
        /// and major causes of death (heart disease/stroke and cancer).
        ///
        /// <para>
        /// <b>Algorithm:</b> Uses the shared 3-epoch piecewise linear models
        /// <see cref="HumanBirthRankbyDate"/> (cumulative births) and
        /// <see cref="HumanPopulationByDate"/> (population), anchored to 1900-01-01 UTC.
        /// The delta between the base date and now gives births and deaths between the
        /// two dates. Sub-statistics are derived from fixed global ratios:
        /// twins approximately 2.4% of births, heart disease or stroke approximately
        /// 27% of deaths, cancer approximately 18% of deaths.
        /// </para>
        /// <para>
        /// <b>Side effect:</b> reads <see cref="AppResources"/> for all output strings
        /// so result language follows <c>AppResources.Culture</c>.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The user-selected origin date (e.g., birthday).</param>
        /// <param name="baseDateName">Human-readable label for display in the full text.</param>
        /// <param name="baseDateValue">ISO-8601 string for the base date for formatted output.</param>
        /// <param name="now">Optional override for current time; used by unit tests for determinism.</param>
        /// <returns>
        /// A <see cref="VibrantHumanityResult"/> with formatted birth and death counts
        /// and raw numeric fields for all five demographic sub-statistics.
        /// </returns>
        [AIContext("LiveTicker")]
        [AIContext("ExternalDataModel")]
        public VibrantHumanityResult CalculateVibrantHumanity(
            DateTime baseDate, string baseDateName, string baseDateValue, DateTime? now = null)
        {
            DateTime now_        = now ?? DateTime.UtcNow;
            DateTime baseDateUtc = baseDate.ToUniversalTime();

            AeonLog.Debug(LogCat, nameof(CalculateVibrantHumanity), $"baseDate={baseDate:d}");

            double bornBetweenDates  = Math.Max(0, HumanBirthRankbyDate(now_) - HumanBirthRankbyDate(baseDateUtc));
            double diedBetweenDates  = Math.Max(0, TotalDeathsByDate(now_) - TotalDeathsByDate(baseDateUtc));

            double twinsBorn    = bornBetweenDates * 0.024;
            double heartDeaths  = diedBetweenDates * 0.27;
            double cancerDeaths = diedBetweenDates * 0.18;

            string births  = ((long)bornBetweenDates).ToString("N0");
            string deaths  = ((long)diedBetweenDates).ToString("N0");
            string twins   = ((long)twinsBorn).ToString("N0");
            string heart   = ((long)heartDeaths).ToString("N0");
            string cancer  = ((long)cancerDeaths).ToString("N0");

            string briefText = AppResources.Ticker_VibrantHumanityBrief
                .Replace("{births}", births)
                .Replace("{deaths}", deaths);

            string fullText = AppResources.Ticker_VibrantHumanityFull
                .Replace("{baseDateName}", baseDateName)
                .Replace("{baseDate:d}",   baseDate.ToString("d", System.Globalization.CultureInfo.CurrentUICulture))
                .Replace("{births}",  births)
                .Replace("{deaths}",  deaths)
                .Replace("{twins}",   twins)
                .Replace("{heart}",   heart)
                .Replace("{cancer}",  cancer);

            return new VibrantHumanityResult
            {
                BornBetweenDates = bornBetweenDates,
                DiedBetweenDates = diedBetweenDates,
                TwinsBorn        = twinsBorn,
                HeartDeaths      = heartDeaths,
                CancerDeaths     = cancerDeaths,
                BriefText        = briefText,
                FullText         = fullText
            };
        }

        #endregion

        #region Life Log

        /// <summary>
        /// Estimates the total time an average person would have spent on various
        /// daily activities since <paramref name="baseDate"/>, based on American
        /// Time Use Survey (ATUS) average daily hours per activity.
        ///
        /// <para>
        /// <b>Algorithm:</b> each activity has a fixed average daily-hours constant
        /// sourced from ATUS data. The total hours for each activity equals
        /// <c>averageHoursPerDay * totalDays</c>. The brief view randomly selects
        /// two activities and formats their raw hours as N0. The full view lists
        /// all activities with hours converted to a readable
        /// "Y years M months D days H hours" string, omitting leading zero segments.
        /// </para>
        /// <para>
        /// <b>Side effect:</b> reads <see cref="AppResources"/> for all output
        /// strings and activity names so results automatically reflect the active
        /// locale set by <c>AppResources.Culture</c>.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The user-selected origin date.</param>
        /// <param name="baseDateName">Human-readable label for display in output strings.</param>
        /// <param name="baseDateValue">ISO-8601 string for the base date for output formatting.</param>
        /// <param name="rand">Optional <see cref="Random"/> instance for deterministic testing of brief-text activity selection.</param>
        /// <param name="now">Optional <see cref="DateTime"/> override for deterministic testing.</param>
        /// <returns>
        /// A <see cref="LifeLogResult"/> with brief text showing two random activities
        /// and full text listing all activities with readable time breakdowns.
        /// </returns>
        [AIContext("CoreCalculation")]
        public LifeLogResult CalculateLifeLog(DateTime baseDate, string baseDateName, string baseDateValue, Random? rand = null, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.Now;
            AeonLog.Debug(LogCat, nameof(CalculateLifeLog), $"baseDate={baseDate:d}");

            // Average daily hours spent on various activities (ATUS data)
            var dailyAverages = new Dictionary<string, double>
            {
                { AppResources.LifeLog_Activity_Sleeping,          8.8 },
                { AppResources.LifeLog_Activity_LeisureScreenTime, 5.2 },
                { AppResources.LifeLog_Activity_Working,           3.6 },
                { AppResources.LifeLog_Activity_HouseholdChores,   1.8 },
                { AppResources.LifeLog_Activity_EatingDrinking,    1.2 },
                { AppResources.LifeLog_Activity_Commuting,         1.1 },
                { AppResources.LifeLog_Activity_PersonalCare,      0.8 },
                { AppResources.LifeLog_Activity_Other,             1.5 },
            };

            double totalDays = (now_ - baseDate).TotalDays;

            // Calculate total hours for each activity
            var calculatedActivities = dailyAverages.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value * totalDays
            );

            // For the brief view: randomly select 2 activities
            var rng = rand ?? new Random();
            var randomKeys = calculatedActivities.Keys.OrderBy(_ => rng.Next()).Take(2).ToList();
            string activity1Name  = randomKeys[0];
            double activity1Hours = calculatedActivities[activity1Name];
            string activity2Name  = randomKeys[1];
            double activity2Hours = calculatedActivities[activity2Name];

            string briefText = AppResources.Ticker_LifeLogBrief
                .Replace("{activity_1}", activity1Name)
                .Replace("{hours_1}",    activity1Hours.ToString("N0"))
                .Replace("{activity_2}", activity2Name)
                .Replace("{hours_2}",    activity2Hours.ToString("N0"));

            // For the full view: convert raw hours to readable time string per activity
            var activityLines = new System.Text.StringBuilder();
            foreach (var kvp in calculatedActivities)
            {
                string readable     = FormatHoursAsReadableTime(kvp.Value);
                string activityName = kvp.Key.Replace("&", "&amp;");
                activityLines.Append("&bull; ").Append(activityName).Append(": ").Append(readable).Append("<br>");
            }

            string fullText = AppResources.Ticker_LifeLogFull
                .Replace("{baseDateName}",       baseDateName)
                .Replace("{baseDate:d}",         baseDate.ToString("d", System.Globalization.CultureInfo.CurrentUICulture))
                .Replace("{all_activities_list}", activityLines.ToString());

            // Build activity slices for the two-ring donut chart.
            // Colours are fixed data-palette values (not theme keys) chosen for
            // visual distinction on both dark and light backgrounds.
            double elapsedYearsToday    = totalDays / 365.25;
            double elapsedYearsForecast = elapsedYearsToday + 10.0;

            var sliceColors = new[]
            {
                "#5B9BD5", // Sleeping         - steel blue
                "#ED7D31", // Leisure          - orange
                "#A5A5A5", // Working          - gray
                "#FFC000", // Household        - amber
                "#70AD47", // Eating/Drinking  - green
                "#FF6B6B", // Commuting        - coral
                "#B07FD4", // Personal Care    - lavender
                "#78909C", // Other            - blue-grey
            };

            var activitySlices = new List<LifeLogSlice>();
            int colorIdx = 0;
            foreach (var kvp in dailyAverages)
            {
                double proportion = kvp.Value / 24.0;
                activitySlices.Add(new LifeLogSlice
                {
                    CategoryName    = kvp.Key,
                    DailyHours      = kvp.Value,
                    DailyProportion = proportion,
                    ColorHex        = sliceColors[colorIdx % sliceColors.Length],
                    YearsToday      = proportion * elapsedYearsToday,
                    YearsForecast   = proportion * elapsedYearsForecast,
                });
                colorIdx++;
            }

            return new LifeLogResult
            {
                TotalDays      = totalDays,
                ActivityHours  = calculatedActivities,
                ActivitySlices = activitySlices,
                Activity1Name  = activity1Name,
                Activity1Hours = activity1Hours,
                Activity2Name  = activity2Name,
                Activity2Hours = activity2Hours,
                BriefText      = briefText,
                FullText       = fullText
            };
        }

        /// <summary>
        /// Converts a raw total-hours value into a human-readable time string of the
        /// form "Y years M months D days H hours", omitting any leading components
        /// whose value is zero.
        /// </summary>
        /// <param name="totalHours">Raw total hours to convert.</param>
        /// <returns>
        /// A non-empty string such as "3 years 2 months 15 days 4 hours" or
        /// "45 days 6 hours". Returns "0 hours" when <paramref name="totalHours"/>
        /// is less than one.
        /// </returns>
        private static string FormatHoursAsReadableTime(double totalHours)
        {
            if (totalHours < 1.0)
                return string.Format(AppResources.Unit_HoursTemplate, 0);

            long hoursTotal   = (long)totalHours;
            long years        = hoursTotal / 8766;      // avg hours per year (365.25 * 24)
            long remaining    = hoursTotal % 8766;
            long months       = remaining / 730;        // avg hours per month (365.25/12 * 24)
            remaining         = remaining % 730;
            long days         = remaining / 24;
            long hours        = remaining % 24;

            var parts = new System.Text.StringBuilder();

            if (years > 0)
                parts.Append(string.Format(AppResources.Unit_YearsTemplate, years)).Append(' ');
            if (months > 0 || years > 0)
            {
                if (months > 0)
                    parts.Append(string.Format(AppResources.Unit_MonthsTemplate, months)).Append(' ');
            }
            if (days > 0 || months > 0 || years > 0)
            {
                if (days > 0)
                    parts.Append(string.Format(AppResources.Unit_DaysTemplate, days)).Append(' ');
            }
            if (hours > 0)
                parts.Append(string.Format(AppResources.Unit_HoursTemplate, hours));

            string result = parts.ToString().Trim();
            return string.IsNullOrEmpty(result)
                ? string.Format(AppResources.Unit_HoursTemplate, 0)
                : result;
        }

        #endregion


        #region Space Wait

        /// <summary>
        /// Calculates the countdown to the user's next "birthday" (orbital completion) on
        /// the planet in our solar system whose next full orbit is closest in time.
        ///
        /// <para>
        /// <b>Algorithm:</b> For each of the seven major planets (Mercury through Neptune),
        /// the method computes how many full orbital periods have elapsed since the base date,
        /// then determines the fractional day count until the next whole-number orbital birthday.
        /// The planet with the smallest remaining time is selected as the next milestone.
        /// Orbital periods are sourced from the NASA Goddard Space Flight Center Planetary Fact Sheet.
        /// </para>
        /// <para>
        /// <b>Ordinal suffix:</b> An English-language ordinal suffix (st/nd/rd/th) is appended
        /// to the age integer when the active culture is English. For all other cultures a plain
        /// numeric string is used.
        /// </para>
        /// <para>
        /// <b>Side effects:</b> reads <see cref="AppResources"/> for all output strings and
        /// planet names, so the result automatically reflects <c>AppResources.Culture</c>.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The user-selected origin date (UTC).</param>
        /// <param name="now">Optional override for the current time; used by unit tests for determinism.</param>
        /// <returns>
        /// A <see cref="SpaceWaitResult"/> with brief and full countdown descriptions, the next
        /// planet name, the ordinal age, and the raw <see cref="TimeSpan"/> countdown.
        /// </returns>
        [AIContext("LiveTicker")]
        public SpaceWaitResult CalculateSpaceWait(DateTime baseDate, DateTime? now = null)
        {
            DateTime now_ = now ?? DateTime.UtcNow;
            AeonLog.Debug(LogCat, nameof(CalculateSpaceWait), $"baseDate={baseDate:d}");

            // Orbital periods in Earth days (NASA Goddard Planetary Fact Sheet)
            var planets = new (string Key, string LocalizedName, double OrbitalDays)[]
            {
                ("Mercury", AppResources.Planet_Mercury, 87.97),
                ("Venus",   AppResources.Planet_Venus,   224.70),
                ("Mars",    AppResources.Planet_Mars,    686.98),
                ("Jupiter", AppResources.Planet_Jupiter, 4332.59),
                ("Saturn",  AppResources.Planet_Saturn,  10759.22),
                ("Uranus",  AppResources.Planet_Uranus,  30685.40),
                ("Neptune", AppResources.Planet_Neptune, 60189.00),
            };

            double totalDaysAlive = (now_ - baseDate).TotalDays;

            string nextPlanet = "";
            string nextPlanetLocalized = "";
            double minDaysToNext = double.MaxValue;
            int nextAge = 0;

            foreach (var planet in planets)
            {
                double orbitalPeriod = planet.OrbitalDays;
                double currentAgeFraction = totalDaysAlive / orbitalPeriod;
                int currentAge = (int)Math.Floor(currentAgeFraction);

                // Days until the next full orbit is completed
                double daysToNext = (currentAge + 1) * orbitalPeriod - totalDaysAlive;

                if (daysToNext < minDaysToNext)
                {
                    minDaysToNext = daysToNext;
                    nextPlanet = planet.Key;
                    nextPlanetLocalized = planet.LocalizedName;
                    nextAge = currentAge + 1;
                }
            }

            TimeSpan countdown = TimeSpan.FromDays(minDaysToNext);

            // Format the countdown readably for mobile: e.g. "12d 14h 32m 10s"
            string countdownFormatted = FormatCountdown(countdown);

            // Build ordinal age string: "4th", "21st" etc. in English; plain number otherwise
            string ageOrdinal = FormatOrdinal(nextAge);

            string briefText = AppResources.Ticker_SpaceWaitBrief
                .Replace("{planet}",    nextPlanetLocalized)
                .Replace("{countdown}", countdownFormatted);

            string fullText = AppResources.Ticker_SpaceWaitFull
                .Replace("{age}",       ageOrdinal)
                .Replace("{planet}",    nextPlanetLocalized)
                .Replace("{countdown}", countdownFormatted);

            return new SpaceWaitResult
            {
                NextPlanet = nextPlanetLocalized,
                NextAge    = nextAge,
                Countdown  = countdown,
                BriefText  = briefText,
                FullText   = fullText,
            };
        }

        /// <summary>
        /// Formats a <see cref="TimeSpan"/> as a compact, mobile-friendly countdown string.
        /// Shows only the non-zero components from days down to seconds.
        /// </summary>
        /// <param name="ts">The time span to format.</param>
        /// <returns>A string such as "12d 14h 32m 10s".</returns>
        private static string FormatCountdown(TimeSpan ts)
        {
            int days    = (int)ts.TotalDays;
            int hours   = ts.Hours;
            int minutes = ts.Minutes;
            int seconds = ts.Seconds;

            if (days > 0)
                return $"{days}d {hours}h {minutes}m {seconds}s";
            if (hours > 0)
                return $"{hours}h {minutes}m {seconds}s";
            if (minutes > 0)
                return $"{minutes}m {seconds}s";
            return $"{seconds}s";
        }

        /// <summary>
        /// Returns an ordinal string for <paramref name="n"/> when the active UI culture is
        /// English (e.g. 1 -> "1st", 2 -> "2nd", 21 -> "21st").
        /// For all other cultures, returns the plain number as a string.
        /// </summary>
        /// <param name="n">Non-negative integer to ordinalize.</param>
        /// <returns>Ordinal string respecting the active locale.</returns>
        private static string FormatOrdinal(int n)
        {
            var culture = System.Globalization.CultureInfo.CurrentUICulture;
            if (!culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase))
                return n.ToString();

            int abs = Math.Abs(n);
            int lastTwo = abs % 100;
            int lastOne = abs % 10;

            string suffix;
            if (lastTwo >= 11 && lastTwo <= 13)
                suffix = "th";
            else if (lastOne == 1)
                suffix = "st";
            else if (lastOne == 2)
                suffix = "nd";
            else if (lastOne == 3)
                suffix = "rd";
            else
                suffix = "th";

            return $"{n}{suffix}";
        }

        #endregion

        #region Vibrant Nature

        /// <summary>
        /// Estimates the number of new biological species described by science and
        /// the number of species driven to extinction globally since the user's base date,
        /// using a 3-epoch piecewise linear daily-rate model anchored to 1900-01-01.
        /// Recalculated on demand (static ticker with refresh button).
        ///
        /// <para>
        /// <b>Algorithm:</b> Discovery rates: 1900-1950 approx. 27.4/day; 1950-2000 approx. 41.1/day;
        /// 2000-present approx. 49.3/day. Extinction rates: 1900-1950 approx. 10/day;
        /// 1950-2000 approx. 50/day; 2000-present approx. 150/day. Rates derived from IISE and
        /// IPBES data. Taxonomic breakdowns use fixed proportions from the Catalogue of Life
        /// and IUCN: Insects/Invertebrates approx. 55% of discoveries and approx. 60% of extinctions,
        /// Plants approx. 15% of discoveries, Vertebrates approx. 2% of both.
        /// </para>
        /// <para>
        /// <b>Side effect:</b> reads <see cref="AppResources"/> for all output strings
        /// so result language follows <c>AppResources.Culture</c>.
        /// </para>
        /// </summary>
        /// <param name="baseDate">The user-selected origin date (e.g., birthday).</param>
        /// <param name="now">Optional override for current time; used by unit tests for determinism.</param>
        /// <returns>
        /// A <see cref="VibrantNatureResult"/> with formatted discovery and extinction counts
        /// and raw numeric fields for all taxonomic sub-statistics.
        /// </returns>
        [AIContext("CoreCalculation")]
        [AIContext("ExternalDataModel")]
        public VibrantNatureResult CalculateVibrantNature(DateTime baseDate, DateTime? now = null)
        {
            DateTime today       = now ?? DateTime.UtcNow;
            DateTime baseDateUtc = baseDate.ToUniversalTime();

            AeonLog.Debug(LogCat, nameof(CalculateVibrantNature), $"baseDate={baseDate:d}");

            double discoveredSince = Math.Max(0, DiscoveredSpeciesByDate(today) - DiscoveredSpeciesByDate(baseDateUtc));
            double extinctSince    = Math.Max(0, ExtinctSpeciesByDate(today)    - ExtinctSpeciesByDate(baseDateUtc));

            double insectsDiscovered    = discoveredSince * 0.55;
            double plantsDiscovered     = discoveredSince * 0.15;
            double vertebratesDiscovered = discoveredSince * 0.02;

            double insectsExtinct    = extinctSince * 0.60;
            double vertebratesExtinct = extinctSince * 0.02;

            string discovered          = ((long)discoveredSince).ToString("N0");
            string extinct             = ((long)extinctSince).ToString("N0");
            string insectsDiscStr      = ((long)insectsDiscovered).ToString("N0");
            string plantsDiscStr       = ((long)plantsDiscovered).ToString("N0");
            string vertebratesDiscStr  = ((long)vertebratesDiscovered).ToString("N0");
            string insectsExtStr       = ((long)insectsExtinct).ToString("N0");
            string vertebratesExtStr   = ((long)vertebratesExtinct).ToString("N0");

            string briefText = AppResources.Ticker_VibrantNatureBrief
                .Replace("{discovered}", discovered)
                .Replace("{extinct}",    extinct);

            string fullText = AppResources.Ticker_VibrantNatureFull
                .Replace("{discovered}",           discovered)
                .Replace("{extinct}",              extinct)
                .Replace("{insects_discovered}",   insectsDiscStr)
                .Replace("{plants_discovered}",    plantsDiscStr)
                .Replace("{vertebrates_discovered}", vertebratesDiscStr)
                .Replace("{insects_extinct}",      insectsExtStr)
                .Replace("{vertebrates_extinct}",  vertebratesExtStr);

            return new VibrantNatureResult
            {
                DiscoveredSince       = discoveredSince,
                ExtinctSince          = extinctSince,
                InsectsDiscovered     = insectsDiscovered,
                PlantsDiscovered      = plantsDiscovered,
                VertebratesDiscovered = vertebratesDiscovered,
                InsectsExtinct        = insectsExtinct,
                VertebratesExtinct    = vertebratesExtinct,
                BriefText             = briefText,
                FullText              = fullText
            };
        }

        /// <summary>
        /// Cumulative count of species described by science from 1900-01-01 up to
        /// <paramref name="date"/>, using a 3-epoch piecewise linear daily-rate model.
        /// Rates: 1900-1950 approx. 27.4/day; 1950-2000 approx. 41.1/day; 2000-present approx. 49.3/day.
        /// </summary>
        /// <param name="date">UTC date at which to evaluate the cumulative count.</param>
        /// <returns>Cumulative species described up to <paramref name="date"/>.</returns>
        private static double DiscoveredSpeciesByDate(DateTime date)
        {
            double days = (date - new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds / 86400.0;
            if (days < 18262)
                return days * 27.4;
            if (days < 36525)
                return (18262 * 27.4) + ((days - 18262) * 41.1);
            return (18262 * 27.4) + (18263 * 41.1) + ((days - 36525) * 49.3);
        }

        /// <summary>
        /// Cumulative count of species estimated to have gone extinct globally from 1900-01-01
        /// up to <paramref name="date"/>, using a 3-epoch piecewise linear daily-rate model.
        /// Rates: 1900-1950 approx. 10/day; 1950-2000 approx. 50/day; 2000-present approx. 150/day.
        /// <para>
        /// All epoch anchors use UTC midnight to prevent local timezone shifts from
        /// causing population jumps at midnight.
        /// </para>
        /// </summary>
        /// <param name="date">UTC date at which to evaluate the cumulative count.</param>
        /// <returns>Cumulative species lost to extinction up to <paramref name="date"/>.</returns>
        private static double ExtinctSpeciesByDate(DateTime date)
        {
            double days = (date - new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds / 86400.0;
            if (days < 18262)
                return days * 10.0;
            if (days < 36525)
                return (18262 * 10.0) + ((days - 18262) * 50.0);
            return (18262 * 10.0) + (18263 * 50.0) + ((days - 36525) * 150.0);
        }

        #endregion

        #region Tease Text

        /// <summary>
        /// Produces the teaser text displayed when the user taps the app logo.
        /// Randomly selects one of five stat lines, each sourced from a typed
        /// ticker result so no string-parsing is required.
        ///
        /// <para><b>Pool (one chosen at random each call):</b></para>
        /// <list type="number">
        ///   <item><description>Countdown - uses <see cref="CountdownResult.Days"/>, <see cref="CountdownResult.Hours"/>, <see cref="CountdownResult.Minutes"/> directly (DaysHours template, HTML stripped).</description></item>
        ///   <item><description>Heartbeats - uses <see cref="LifeOdometerResult.Heartbeats"/>.</description></item>
        ///   <item><description>Breaths - uses <see cref="LifeOdometerResult.Breaths"/>.</description></item>
        ///   <item><description>Galactic commute - uses <see cref="GalacticCommuteResult.Distance"/>.</description></item>
        ///   <item><description>Global exhale - uses <see cref="GlobalExhaleResult.FormattedAmount"/>.</description></item>
        /// </list>
        /// /// </summary>
        /// <param name="countdown">Typed countdown result from <see cref="CalculateCountdown"/>.</param>
        /// <param name="lifeOdometer">Typed life-odometer result from <see cref="CalculateLifeOdometer"/>.</param>
        /// <param name="galacticCommute">Typed galactic-commute result from <see cref="CalculateGalacticCommute"/>.</param>
        /// <param name="globalExhale">Typed global-exhale result from <see cref="CalculateGlobalExhale"/>.</param>
        /// <param name="baseDateName">Human-readable label for the origin date.</param>
        /// <param name="baseDate">The origin date; formatted with the current UI culture short-date format.</param>
        /// <returns>A single randomly-chosen formatted teaser string.</returns>
        [AIContext("UIPresentation")]
        public string GetRandomTeaseText(
            CountdownResult countdown, LifeOdometerResult lifeOdometer,
            GalacticCommuteResult galacticCommute, GlobalExhaleResult globalExhale,
            string baseDateName, DateTime baseDate)
        {
            string baseDateFormatted = baseDate.ToString("d");
            var teases = new[]
            {
                AppResources.Tease_Countdown
                    .Replace("{days}", countdown.Days.ToString())
                    .Replace("{hrs}",  countdown.Hours.ToString())
                    .Replace("{mins}", countdown.Minutes.ToString()),
                AppResources.Tease_Heartbeats
                    .Replace("{lifeOdometer.Heartbeats}", lifeOdometer.Heartbeats.ToString("N0"))
                    .Replace("{baseDateValue}", baseDateFormatted),
                AppResources.Tease_Breaths
                    .Replace("{lifeOdometer.Breaths}", lifeOdometer.Breaths.ToString("N0"))
                    .Replace("{baseDateValue}", baseDateFormatted),
                AppResources.Tease_GalacticCommute
                    .Replace("{galacticCommute.Distance}", galacticCommute.Distance)
                    .Replace("{baseDateValue}", baseDateFormatted),
                AppResources.Tease_GlobalExhale
                    .Replace("{baseDateName}", baseDateName)
                    .Replace("{globalExhale.Amount}", globalExhale.FormattedAmount)
            };
            return teases[new Random().Next(teases.Length)];
        }

        #endregion
    }
}
