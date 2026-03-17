using Aeonpulse.Models;
using System;
using System.Linq;
using Aeonpulse.Resources;

namespace Aeonpulse.Services
{
    public class CalculationService
    {
        #region Helper Methods

        private static long FindNearestJubilee(long diff)
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

        private static int ReduceToSingleDigit(int num)
        {
            while (num > 9)
            {
                num = num.ToString().Sum(c => c - '0');
            }
            return num;
        }

        #endregion

        #region Time Jubilees

        public TickerData CalculateTimeJubilees(DateTime baseDate, string baseDateName, string baseDateValue)
        {
            DateTime now = DateTime.Now;
            int bYear = baseDate.Year;
            int bMonth = baseDate.Month;
            int bDay = baseDate.Day;
            int nYear = now.Year;

            long passedDays = (long)(now - baseDate).TotalDays;
            long passedYears = (long)(passedDays / 365.24219);
            long passedMonths = passedYears * 12 + (now.Month - baseDate.Month);
            long passedWeeks = passedDays / 7;
            long passedHours = passedDays * 24;
            long passedMinutes = passedHours * 60;
            long passedSeconds = passedMinutes * 60;

            // Find next jubilee

            long daysTillNearestJubilee = long.MaxValue;
            DateTime nearestJubileeDate = now;
            long nearestJubileeValue = long.MaxValue;
            string nearestJubileeUnit = "";

            // Years
            long nearestJubileeYears = FindNearestJubilee(passedYears);
            DateTime nearestJubileeYearsDate = new DateTime(bYear + (int)nearestJubileeYears, bMonth, bDay);
            long daysToYearsJubilee = (long)(nearestJubileeYearsDate - now).TotalDays;
            if (daysToYearsJubilee > 0 && daysToYearsJubilee < daysTillNearestJubilee)
            {
                nearestJubileeDate = nearestJubileeYearsDate;
                daysTillNearestJubilee = daysToYearsJubilee;
                nearestJubileeValue = nearestJubileeYears;
                nearestJubileeUnit = AppResources.Unit_Years;
            }

            // Months
            long nearestJubileeMonths = FindNearestJubilee(passedMonths);
            DateTime nearestJubileeMonthsDate = baseDate.AddMonths((int)nearestJubileeMonths);
            long daysToMonthsJubilee = (long)(nearestJubileeMonthsDate - now).TotalDays;
            if (daysToMonthsJubilee > 0 && daysToMonthsJubilee < daysTillNearestJubilee)
            {
                nearestJubileeDate = nearestJubileeMonthsDate;
                daysTillNearestJubilee = daysToMonthsJubilee;
                nearestJubileeValue = nearestJubileeMonths;
                nearestJubileeUnit = AppResources.Unit_Months;
            }

            // Weeks
            long nearestJubileeWeeks = FindNearestJubilee(passedWeeks);
            DateTime nearestJubileeWeeksDate = baseDate.AddDays(nearestJubileeWeeks * 7);
            long daysToWeeksJubilee = (long)(nearestJubileeWeeksDate - now).TotalDays;
            if (daysToWeeksJubilee > 0 && daysToWeeksJubilee < daysTillNearestJubilee)
            {
                nearestJubileeDate = nearestJubileeWeeksDate;
                daysTillNearestJubilee = daysToWeeksJubilee;
                nearestJubileeValue = nearestJubileeWeeks;
                nearestJubileeUnit = AppResources.Unit_Weeks;
            }

            // Days
            long nearestJubileeDays = FindNearestJubilee(passedDays);
            DateTime nearestJubileeDaysDate = baseDate.AddDays(nearestJubileeDays);
            long daysToDaysJubilee = (long)(nearestJubileeDaysDate - now).TotalDays;
            if (daysToDaysJubilee > 0 && daysToDaysJubilee < daysTillNearestJubilee)
            {
                nearestJubileeDate = nearestJubileeDaysDate;
                daysTillNearestJubilee = daysToDaysJubilee;
                nearestJubileeValue = nearestJubileeDays;
                nearestJubileeUnit = AppResources.Unit_Days;
            }

            // Hours
            long nearestJubileeHours = FindNearestJubilee(passedHours);
            DateTime nearestJubileeHoursDate = baseDate.AddHours(nearestJubileeHours);
            long daysToHoursJubilee = (long)(nearestJubileeHoursDate - now).TotalDays;
            if (daysToHoursJubilee > 0 && daysToHoursJubilee < daysTillNearestJubilee)
            {
                nearestJubileeDate = nearestJubileeHoursDate;
                daysTillNearestJubilee = daysToHoursJubilee;
                nearestJubileeValue = nearestJubileeHours;
                nearestJubileeUnit = AppResources.Unit_Hours;
            }

            // Minutes
            long nearestJubileeMinutes = FindNearestJubilee(passedMinutes);
            DateTime nearestJubileeMinutesDate = baseDate.AddMinutes(nearestJubileeMinutes);
            long daysToMinutesJubilee = (long)(nearestJubileeMinutesDate - now).TotalDays;
            if (daysToMinutesJubilee > 0 && daysToMinutesJubilee < daysTillNearestJubilee)
            {
                nearestJubileeDate = nearestJubileeMinutesDate;
                daysTillNearestJubilee = daysToMinutesJubilee;
                nearestJubileeValue = nearestJubileeMinutes;
                nearestJubileeUnit = AppResources.Unit_Minutes;
            }

            // Seconds
            long nearestJubileeSeconds = FindNearestJubilee(passedSeconds);
            DateTime nearestJubileeSecondsDate = baseDate.AddSeconds(nearestJubileeSeconds);
            long daysToSecondsJubilee = (long)(nearestJubileeSecondsDate - now).TotalDays;
            if (daysToSecondsJubilee > 0 && daysToSecondsJubilee < daysTillNearestJubilee)
            {
                nearestJubileeDate = nearestJubileeSecondsDate;
                daysTillNearestJubilee = daysToSecondsJubilee;
                nearestJubileeValue = nearestJubileeSeconds;
                nearestJubileeUnit = AppResources.Unit_Seconds;
            }

            string nextJubilee = $"{nearestJubileeValue:N0} {nearestJubileeUnit}";

            return new TickerData
            {
                BriefText = $"Next milestone: {nextJubilee} on {nearestJubileeDate:d}",
                FullText = $"Since {baseDateName} on {baseDate:d}, incredible milestones of time have passed. The next big marker is {nextJubilee} on {nearestJubileeDate:d}!"
            };
        }

        #endregion

        #region Countdown

        public TickerData CalculateCountdown(DateTime baseDate)
        {
            DateTime now = DateTime.Now;
            int bYear = baseDate.Year;
            int bMonth = baseDate.Month;
            int bDay = baseDate.Day;
            int nYear = now.Year;

            // Find next year jubilee for countdown
            DateTime nearest = new DateTime(nYear, bMonth, bDay);
            if (nearest < now)
                nearest = nearest.AddYears(1);

            long seconds = (long)(nearest - now).TotalSeconds;
            long days = seconds / 86400;
            long hrs = (seconds - days * 86400) / 3600;
            long mins = (seconds - days * 86400 - hrs * 3600) / 60;
            long secs = seconds % 60;

            string countdown;
            string countdownFull;

            if (seconds < 86400) // less than a day
            {
                countdown = $"{hrs}h : {mins}m : {secs}s until next anniversary";
                countdownFull = $"We're counting the seconds! Only {hrs} hours, {mins} minutes, and {secs} seconds left until you hit the next anniversary on {nearest:d}.";
            }
            else // more than a day
            {
                countdownFull = $"We're counting the seconds! Only {days} days, {hrs} hours, {mins} minutes, and {secs} seconds left until you hit the next anniversary on {nearest:d}.";
                if (seconds < 2592000) // more than a day but less than a month
                {
                    countdown = $"{days} days {hrs}h : {mins}m until next anniversary";
                }
                else // more than a month
                {
                    countdown = $"{days} days until next anniversary";
                }
            }

            return new TickerData
            {
                BriefText = countdown,
                FullText = countdownFull
            };
        }

        #endregion

        #region Life Odometer

        public TickerData CalculateLifeOdometer(DateTime baseDate, string baseDateName, string baseDateValue)
        {
            DateTime now = DateTime.Now;
            long seconds = (long)(now - baseDate).TotalSeconds;

            long heartbeats = seconds * 70 / 60;
            long breaths = seconds * 16 / 60;

            return new TickerData
            {
                BriefText = $"{heartbeats:N0} heartbeats and {breaths:N0} breaths",
                FullText = $"Approximately {heartbeats:N0} heartbeats have drummed and {breaths:N0} breaths have been processed since {baseDateName} on {baseDate:d}."
            };
        }

        #endregion

        #region Alien Anniversaries

        public TickerData CalculateAlienAnniversaries(DateTime baseDate, string baseDateName, string baseDateValue)
        {
            DateTime now = DateTime.Now;
            long earthDays = (long)(now - baseDate).TotalDays;

            // Mars: 686.98 Earth days = 1 Mars year
            double marsYears = earthDays / 686.98;

            // Venus: 224.7 Earth days = 1 Venus year
            double venusYears = earthDays / 224.7;

            return new TickerData
            {
                BriefText = $"{marsYears:F2} years on Mars, {venusYears:F2} on Venus",
                FullText = $"Since {baseDateName} on {baseDate:d}, the planets have finished their laps at different speeds. On Mars, this timeline has spanned {marsYears:F2} Martian years, while on Venus, this timeline has spanned {venusYears:F2} Venusian years."
            };
        }

        #endregion

        #region Galactic Commute

        public TickerData CalculateGalacticCommute(DateTime baseDate, string baseDateValue, bool useMetric)
        {
            DateTime now = DateTime.Now;
            long seconds = (long)(now - baseDate).TotalSeconds;

            // Solar system moves at ~220-230 km/s through the galaxy
            double kmTraveled = seconds * 225;

            string distance;
            string fullDistance = $"({(kmTraveled)} km) ";
            if (useMetric)
            {
                if (kmTraveled > 1000000000)
                    distance = $"{(kmTraveled / 1000000000):F2} {AppResources.UnitMetric_BKm}";
                else if (kmTraveled > 1000000)
                    distance = $"{(kmTraveled / 1000000):F2} {AppResources.UnitMetric_MKm}";
                else
                {
                    distance = $"{kmTraveled:N0} {AppResources.UnitMetric_Km}";
                    fullDistance = ""; // same as distance - no need
                }
            }
            else
            {
                double miles = kmTraveled * 0.621371;
                fullDistance = $"({miles:N0} {AppResources.UnitImperial_Miles}) ";
                if (miles > 1000000000)
                    distance = $"{(miles / 1000000000):F2} {AppResources.UnitImperial_BMiles}";
                else if (miles > 1000000)
                    distance = $"{(miles / 1000000):F2} {AppResources.UnitImperial_MMiles}";
                else
                {
                    distance = $"{miles:N0} {AppResources.UnitImperial_Miles}";
                    fullDistance = ""; // same as distance - no need
                }
            }

            return new TickerData
            {
                BriefText = $"{distance} through the Galaxy",
                FullText = $"Since {baseDate:d}, Earth has hitched a ride for a {distance} {fullDistance}journey around the center of the Milky Way."
            };
        }

        #endregion

        #region Photon Path

        public TickerData CalculatePhotonPath(DateTime baseDate, string baseDateValue, bool useMetric)
        {
            var stars = new[]
            {
                new { Name = "Proxima Centauri", Ly =  4.246d, Info = AppResources.Star_ProximaCentauri_Info },
                new { Name = "Alpha Centauri", Ly =  4.321d, Info = AppResources.Star_AlphaCentauri_Info },
                new { Name = "Barnard's Star", Ly =  5.963d, Info = AppResources.Star_BarnardsStarInfo },
                new { Name = "Luhman 16", Ly =  6.5d, Info = AppResources.Star_Luhman16_Info },
                new { Name = "Lalande 21185", Ly =  8.29d, Info = AppResources.Star_Lalande21185_Info },
                new { Name = "Sirius", Ly =  8.71d, Info = AppResources.Star_Sirius_Info },
                new { Name = "Epsilon Eridani (Ran)", Ly =  10.47d, Info = AppResources.Star_EpsilonEridani_Info },
                new { Name = "Procyon", Ly =  11.46d, Info = AppResources.Star_Procyon_Info },
                new { Name = "61 Cygni", Ly =  11.4d, Info = AppResources.Star_61Cygni_Info },
                new { Name = "Epsilon Indi", Ly =  11.87d, Info = AppResources.Star_EpsilonIndi_Info },
                new { Name = "Tau Ceti", Ly =  11.91d, Info = AppResources.Star_TauCeti_Info },
                new { Name = "Groombridge 1618", Ly =  15.89d, Info = AppResources.Star_Groombridge1618_Info },
                new { Name = "Omicron2 Eridani (Keid)", Ly =  16.33d, Info = AppResources.Star_Omicron2Eridani_Info },
                new { Name = "70 Ophiuchi", Ly =  16.71d, Info = AppResources.Star_70Ophiuchi_Info },
                new { Name = "Altair", Ly =  16.73d, Info = AppResources.Star_Altair_Info },
                new { Name = "Alsafi", Ly =  18d, Info = AppResources.Star_InCepheus_Info },
                new { Name = "Eta Cassiopeiae (Achird)", Ly =  19.33d, Info = AppResources.Star_EtaCassiopeiae_Info },
                new { Name = "36 Ophiuchi (Guniibuu)", Ly =  19.5d, Info = AppResources.Star_36Ophiuchi_Info },
                new { Name = "Delta Pavonis", Ly =  19.89d, Info = AppResources.Star_DeltaPavonis_Info },
                new { Name = "Vega", Ly =  25d, Info = AppResources.Star_Vega_Info },
                new { Name = "Fomalhaut", Ly =  25.13d, Info = AppResources.Star_Fomalhaut_Info },
                new { Name = "Pollux", Ly =  33.78d, Info = AppResources.Star_Pollux_Info },
                new { Name = "Denebola", Ly =  35.9d, Info = AppResources.Star_Denebola_Info },
                new { Name = "Arcturus", Ly =  36.7d, Info = AppResources.Star_Arcturus_Info },
                new { Name = "Capella", Ly =  42.9d, Info = AppResources.Star_Capella_Info },
                new { Name = "Rasalhague", Ly =  47.8d, Info = AppResources.Star_Rasalhague_Info },
                new { Name = "Alderamin", Ly =  49.1d, Info = AppResources.Star_Alderamin_Info },
                new { Name = "Castor", Ly =  51.6d, Info = AppResources.Star_Castor_Info },
                new { Name = "Caph", Ly =  53.1d, Info = AppResources.Star_Caph_Info },
                new { Name = "Menkent", Ly =  58.8d, Info = AppResources.Star_InCentaurus_Info },
                new { Name = "Aldebaran", Ly =  65.1d, Info = AppResources.Star_Aldebaran_Info },
                new { Name = "Larawag", Ly =  66d, Info = AppResources.Star_InAuriga_Info },
                new { Name = "Hamal", Ly =  66.3d, Info = AppResources.Star_Hamal_Info },
                new { Name = "Aljanah", Ly =  72d, Info = AppResources.Star_InCepheus_Info },
                new { Name = "Alphecca", Ly =  75d, Info = AppResources.Star_Alphecca_Info },
                new { Name = "Ankaa", Ly =  77d, Info = AppResources.Star_Ankaa_Info },
                new { Name = "Merak", Ly =  79.1d, Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = "Regulus", Ly =  79.3d, Info = AppResources.Star_Regulus_Info },
                new { Name = "Alsephina", Ly =  80.6d, Info = AppResources.Star_InCentaurus_Info },
                new { Name = "Menkalinan", Ly =  81.1d, Info = AppResources.Star_InAuriga_Info },
                new { Name = "Alioth", Ly =  82.6d, Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = "Mizar", Ly =  83d, Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = "Phecda", Ly =  83.2d, Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = "Sabik", Ly =  88d, Info = AppResources.Star_Sabik_Info },
                new { Name = "Gacrux", Ly =  88.6d, Info = AppResources.Star_Gacrux_Info },
                new { Name = "Algol", Ly =  94d, Info = AppResources.Star_Algol_Info },
                new { Name = "Diphda", Ly =  96.3d, Info = AppResources.Star_Diphda_Info },
                new { Name = "Alpheratz", Ly =  97d, Info = AppResources.Star_Alpheratz_Info },
                new { Name = "Alnair", Ly =  101d, Info = AppResources.Star_Alnair_Info },
                new { Name = "Alkaid", Ly =  103.9d, Info = AppResources.Star_Alkaid_Info },
                new { Name = "Alhena", Ly =  109d, Info = AppResources.Star_Alhena_Info },
                new { Name = "Miaplacidus", Ly =  113.2d, Info = AppResources.Star_Miaplacidus_Info },
                new { Name = "Dubhe", Ly =  123d, Info = AppResources.Star_InUrsaMajor_Info },
                new { Name = "Muhlifain", Ly =  130d, Info = AppResources.Star_InCepheus_Info },
                new { Name = "Algieba", Ly =  130.3d, Info = AppResources.Star_Algieba_Info },
                new { Name = "Kochab", Ly =  130.9d, Info = AppResources.Star_Kochab_Info },
                new { Name = "Elnath", Ly =  134d, Info = AppResources.Star_Elnath_Info },
                new { Name = "Achernar", Ly =  139d, Info = AppResources.Star_Achernar_Info }
            };

            DateTime now = DateTime.Now;
            long seconds = (long)(now - baseDate).TotalSeconds;

            // Light travels at 299,792 km/s
            double kmTraveled = seconds * 299792.458;
            double lightYears = kmTraveled / 9460730472580.8;

            string distance = $"{lightYears:F2} light-years";
            string fullDistance = useMetric ? $"{(kmTraveled / 1000000):F2} million km" : $"{(kmTraveled * 0.621371 / 1000000):F2} million miles";

            string bText = "";
            string fText = "";

            if (lightYears < 0.00237188)
            {
                if (kmTraveled > 11000000000)
                {
                    bText = "Light has reached the Heliopause";
                    fText = $"If a starship left at light speed on {baseDate:d}, it would have traveled {fullDistance}, and already reached the Heliopause - the boundary, roughly 11-12 billion miles (18-20 billion km) out, where solar wind is halted by the interstellar medium.";
                }
                else
                {
                    bText = "Light is still within the Solar System";
                    fText = $"If a starship left at light speed on {baseDate:d}, it would have traveled {fullDistance}, and is within our Solar System still.";
                }
            }
            else if (lightYears < 1.5)
            {
                bText = "Light has reached the Oort Cloud";
                fText = $"If a starship left at light speed on {baseDate:d}, it would have traveled {fullDistance}, and already reached the Oort Cloud of our Solar System - the theoretical outer edge of the Sun's gravitational influence.";
            }
            else if (lightYears < 4.246)
            {
                bText = "Light has reached interstellar space";
                fText = $"If a starship left at light speed on {baseDate:d}, it would have traveled {distance} ({fullDistance}), and already reached interstellar space - the vast, mostly empty region between stars. The next stop is Proxima Centauri, located 4.246 light-years away.";
            }
            else
            {
                foreach (var star in stars)
                {
                    if (lightYears < star.Ly)
                        break;
                    bText = $"Light has reached {star.Name}";
                    fText = $"If a starship left at light speed on {baseDate:d}, it would have traveled {distance} ({fullDistance}), and already reached {star.Name}, located {star.Ly} light-years away - {star.Info}";
                }
            }

            return new TickerData
            {
                BriefText = bText,
                FullText = fText
            };
        }

        #endregion

        #region Human Birth Rank

        public TickerData CalculateHumanBirthRank(DateTime baseDate, string baseDateName)
        {
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

            long days = (long)(baseDate - new DateTime(1900, 1, 1)).TotalDays;
            double estimatedRank = 0;

            // We'll use 3 different linear approximations for the periods before 1950, between 1950 and 2000, and after 2000,
            // since the growth rate of population has changed significantly in these periods.
            // The estimates won't be perfect, but they should give a reasonable approximation of the birth rank for these dates.
            if (days < 0)
            {
                return new TickerData
                {
                    BriefText = "One of the first 104,510,976,956 humans",
                    FullText = $"The moment when {baseDateName} is before the XX century, the number of humans have ever lived on Earth till 1900 is estimated as 104,510,976,956."
                };
            }
            else if (days < 18262) // before 1950
            {
                estimatedRank = days * (107901175171.0 - 104510976956.0) / 18262.0 + 104510976956.0;
            }
            else if (days < 36525) // before 2000
            {
                estimatedRank = (days - 18262) * (113966170055.0 - 107901175171.0) / 18263.0 + 107901175171.0;
            }
            else // after 2000
            {
                estimatedRank = (days - 36525) * (117020448575.0 - 113966170055.0) / 8036.0 + 113966170055.0;
            }

            return new TickerData
            {
                BriefText = $"Human #{estimatedRank:N0} on this date",
                FullText = $"The moment when {baseDateName} marks the arrival of human #{estimatedRank:N0} in the story of Earth, out of all who have ever lived."
            };
        }

        #endregion

        #region Birth Rune

        public TickerData CalculateBirthRune(DateTime baseDate, string baseDateValue)
        {
            var runes = new[]
            {
                new { Name = "Fehu (ᚠ)", From = "5-29", To = "6-14", Brief = AppResources.Rune_Fehu_Brief, Full = AppResources.Rune_Fehu_Full },
                new { Name = "Uruz (ᚢ)", From = "6-14", To = "6-29", Brief = AppResources.Rune_Uruz_Brief, Full = AppResources.Rune_Uruz_Full },
                new { Name = "Thurisaz (ᚦ)", From = "6-29", To = "7-13", Brief = AppResources.Rune_Thurisaz_Brief, Full = AppResources.Rune_Thurisaz_Full },
                new { Name = "Ansuz (ᚨ)", From = "7-13", To = "7-29", Brief = AppResources.Rune_Ansuz_Brief, Full = AppResources.Rune_Ansuz_Full },
                new { Name = "Raidho (ᚱ)", From = "7-29", To = "8-13", Brief = AppResources.Rune_Raidho_Brief, Full = AppResources.Rune_Raidho_Full },
                new { Name = "Kenaz (ᚲ)", From = "8-13", To = "8-28", Brief = AppResources.Rune_Kenaz_Brief, Full = AppResources.Rune_Kenaz_Full },
                new { Name = "Gebo (ᚷ)", From = "8-28", To = "9-13", Brief = AppResources.Rune_Gebo_Brief, Full = AppResources.Rune_Gebo_Full },
                new { Name = "Wunjo (ᚹ)", From = "9-13", To = "9-28", Brief = AppResources.Rune_Wunjo_Brief, Full = AppResources.Rune_Wunjo_Full },
                new { Name = "Hagalaz (ᚺ/ᚻ)", From = "9-28", To = "10-13", Brief = AppResources.Rune_Hagalaz_Brief, Full = AppResources.Rune_Hagalaz_Full },
                new { Name = "Nauthiz (ᚾ)", From = "10-13", To = "10-28", Brief = AppResources.Rune_Nauthiz_Brief, Full = AppResources.Rune_Nauthiz_Full },
                new { Name = "Isa (ᛁ)", From = "10-28", To = "11-13", Brief = AppResources.Rune_Isa_Brief, Full = AppResources.Rune_Isa_Full },
                new { Name = "Jera (ᚼ)", From = "11-13", To = "11-28", Brief = AppResources.Rune_Jera_Brief, Full = AppResources.Rune_Jera_Full },
                new { Name = "Eihwaz (ᚽ)", From = "11-28", To = "0-13", Brief = AppResources.Rune_Eihwaz_Brief, Full = AppResources.Rune_Eihwaz_Full },
                new { Name = "Perthro (ᚹ)", From = "0-13", To = "0-28", Brief = AppResources.Rune_Perthro_Brief, Full = AppResources.Rune_Perthro_Full },
                new { Name = "Algiz (ᛉ)", From = "0-28", To = "1-13", Brief = AppResources.Rune_Algiz_Brief, Full = AppResources.Rune_Algiz_Full },
                new { Name = "Sowilo (ᛊ)", From = "1-13", To = "1-27", Brief = AppResources.Rune_Sowilo_Brief, Full = AppResources.Rune_Sowilo_Full },
                new { Name = "Tiwaz (ᛏ)", From = "1-27", To = "2-14", Brief = AppResources.Rune_Tiwaz_Brief, Full = AppResources.Rune_Tiwaz_Full },
                new { Name = "Berkano (ᛒ)", From = "2-14", To = "2-30", Brief = AppResources.Rune_Berkano_Brief, Full = AppResources.Rune_Berkano_Full },
                new { Name = "Ehwaz (ᛖ)", From = "2-30", To = "3-14", Brief = AppResources.Rune_Ehwaz_Brief, Full = AppResources.Rune_Ehwaz_Full },
                new { Name = "Mannaz (ᛗ)", From = "3-14", To = "3-29", Brief = AppResources.Rune_Mannaz_Brief, Full = AppResources.Rune_Mannaz_Full },
                new { Name = "Laguz (ᛚ)", From = "3-29", To = "4-14", Brief = AppResources.Rune_Laguz_Brief, Full = AppResources.Rune_Laguz_Full },
                new { Name = "Ingwaz (ᛝ)", From = "4-14", To = "4-29", Brief = AppResources.Rune_Ingwaz_Brief, Full = AppResources.Rune_Ingwaz_Full },
                new { Name = "Othala (ᛟ)", From = "4-29", To = "5-14", Brief = AppResources.Rune_Othala_Brief, Full = AppResources.Rune_Othala_Full },
                new { Name = "Dagaz (ᛞ)", From = "5-14", To = "5-29", Brief = AppResources.Rune_Dagaz_Brief, Full = AppResources.Rune_Dagaz_Full }
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

            return new TickerData
            {
                BriefText = $"Date rune is {birthRune.Name}: {birthRune.Brief}",
                FullText = $"According to Viking lore, {baseDate:d} is governed by the {birthRune.Name} rune: {birthRune.Full}."
            };
        }

        #endregion

        #region Personal Year

        public TickerData CalculatePersonalYear(DateTime baseDate, string baseDateValue)
        {
            // Simple numerology calculation, taken from https://numerology.astro-seek.com/personal-year

            int curYear = DateTime.Now.Year;

            int year = ReduceToSingleDigit(curYear);
            int month = ReduceToSingleDigit(baseDate.Month);
            int day = ReduceToSingleDigit(baseDate.Day);

            int personalYear = ReduceToSingleDigit(year + month + day);

            // Ensure personalYear is between 1 and 9
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

            return new TickerData
            {
                BriefText = $"Year {curYear} is Numerology Year {personalYear}: {interpretations[personalYear - 1].Brief}",
                FullText = $"In Numerology, for those who were born on {baseDate:d}, year {curYear} is Personal Year {personalYear}: {interpretations[personalYear - 1].Full}"
            };
        }

        #endregion

        #region Global Exhale

        public TickerData CalculateGlobalExhale(DateTime baseDate, string baseDateName, string baseDateValue, bool useMetric)
        {
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
            string amount = useMetric ? $"{totalCO2} {AppResources.Ticker_GlobalExhaleMetric_BTonnes}" : $"{(totalCO2 * 0.984252):F2} {AppResources.Ticker_GlobalExhaleImperial_BTons}";

            if (baseYears < 0)
            {
                return new TickerData
                {
                    BriefText = $"Till 1900, {amount} of CO2 emitted",
                    FullText = $"The moment when {baseDateName} is before the XX century - till 1900, humanity has released {amount} of CO2 into the atmosphere. Still a tiny amount compared to later times."
                };
            }

            // Approximation for year >= 1900 (polynomial gives a better R^2 than exponential):
            //    CO2_in_year = 0.0008 * (year - 1900)^2 - 0.0122 * (year - 1900) + 0.6859
            //    Total_CO2_emitted_till_a_date_since_1900_year = 0.0008/3 * (year - 1900)^3 - 0.0122/2 * (year - 1900)^2 + 0.6859 * (year - 1900)
            DateTime now = DateTime.Now;
            int nowYears = (int)((now - year1900).TotalDays / 365.25);
            double baseDaysInYear = (baseDate - new DateTime(baseDate.Year, 1, 1)).TotalDays;
            double nowDaysInYear = (now - new DateTime(now.Year, 1, 1)).TotalDays;
            double x1 = baseYears + baseDaysInYear / 365.0;
            double x2 = nowYears + nowDaysInYear / 365.0;

            double totalCO2Base = 0.0008 / 3 * Math.Pow(x1, 3) - 0.0122 / 2 * Math.Pow(x1, 2) + 0.6859 * x1;
            double totalCO2Now = 0.0008 / 3 * Math.Pow(x2, 3) - 0.0122 / 2 * Math.Pow(x2, 2) + 0.6859 * x2;
            totalCO2 = totalCO2Now - totalCO2Base;
            amount = useMetric ? $"{totalCO2:F2} {AppResources.Ticker_GlobalExhaleMetric_BTonnes}" : $"{(totalCO2 * 0.984252):F2} {AppResources.Ticker_GlobalExhaleImperial_BTons}";

            return new TickerData
            {
                BriefText = $"{amount} of CO2 emitted",
                FullText = $"Since {baseDateName} on {baseDate:d}, humanity has released {amount} of CO2 into the atmosphere. A massive global exhale."
            };
        }

        #endregion

        #region Tease Text

        public string GetRandomTeaseText(TickerData countdown, TickerData lifeOdometer, TickerData galacticCommute, TickerData globalExhale, string baseDateName, string baseDateValue)
        {
            var teases = new[]
            {
                $"Only {countdown.BriefText}! Time is flying, and I'm counting every second. Find your next big milestone.",
                $"My heart has drummed {lifeOdometer.BriefText.Split(" and ")[0]} since {baseDateValue}. My internal engine never stops! Check your vitals on AeonPulse.",
                $"My lungs have processed {lifeOdometer.BriefText.Split(" and ")[1]} since {baseDateValue}. And yours? Check your vitals on AeonPulse.",
                $"Since {baseDateValue}, I've hitched a ride on Earth for a {galacticCommute.BriefText}. I'm literally a space traveler! How far have you traveled?"
            };

            return teases[new Random().Next(teases.Length)];
        }

        #endregion
    }
}
