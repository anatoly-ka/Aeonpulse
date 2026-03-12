using Aeonpulse.Models;
using System;
using System.Linq;

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
                nearestJubileeUnit = "years";
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
                nearestJubileeUnit = "months";
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
                nearestJubileeUnit = "weeks";
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
                nearestJubileeUnit = "days";
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
                nearestJubileeUnit = "hours";
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
                nearestJubileeUnit = "minutes";
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
                nearestJubileeUnit = "seconds";
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
                    distance = $"{(kmTraveled / 1000000000):F2} billion km";
                else if (kmTraveled > 1000000)
                    distance = $"{(kmTraveled / 1000000):F2} million km";
                else
                {
                    distance = $"{kmTraveled:N0} km";
                    fullDistance = ""; // same as distance - no need
                }
            }
            else
            {
                double miles = kmTraveled * 0.621371;
                fullDistance = $"({miles:N0} miles) ";
                if (miles > 1000000000)
                    distance = $"{(miles / 1000000000):F2} billion miles";
                else if (miles > 1000000)
                    distance = $"{(miles / 1000000):F2} million miles";
                else
                {
                    distance = $"{miles:N0} miles";
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
                new { Name = "Proxima Centauri", Ly =  4.246d, Info = "a red dwarf star in the constellation Centaurus, the closest known star to the Sun" },
                new { Name = "Alpha Centauri", Ly =  4.321d, Info = "a binary pair of Sun-like stars in the constellation Centaurus that, with Proxima, form a triple system" },
                new { Name = "Barnard's Star", Ly =  5.963d, Info = "a red dwarf star in the constellation Ophiuchus with the largest proper motion of any known star" },
                new { Name = "Luhman 16", Ly =  6.5d, Info = "a binary brown dwarf system in the constellation Vela, one of the closest to the Sun" },
                new { Name = "Lalande 21185", Ly =  8.29d, Info = "a star in the constellation Ursa Major. One of the brightest red dwarfs near Earth, but still too dim at magnitude 7.52." },
                new { Name = "Sirius", Ly =  8.71d, Info = "a star in the constellation Canis Major. The brightest star in the night sky" },
                new { Name = "Epsilon Eridani (Ran)", Ly =  10.47d, Info = "a visible young Sun-like star with a debris disk in the constellation Eridanus" },
                new { Name = "Procyon", Ly =  11.46d, Info = "the brightest star in the constellation Canis Minor and the eighth-brightest star in the night sky" },
                new { Name = "61 Cygni", Ly =  11.4d, Info = "a binary star system in the constellation Cygnus" },
                new { Name = "Epsilon Indi", Ly =  11.87d, Info = "a nearby star system in the constellation Indus with a brown dwarf companion" },
                new { Name = "Tau Ceti", Ly =  11.91d, Info = "a Sun-like star in the constellation Cetus with a planetary system" },
                new { Name = "Groombridge 1618", Ly =  15.89d, Info = "a nearby star in the constellation Ursa Major with a high proper motion" },
                new { Name = "Omicron2 Eridani (Keid)", Ly =  16.33d, Info = "a triple star system in the constellation Eridanus" },
                new { Name = "70 Ophiuchi", Ly =  16.71d, Info = "a binary star system in the constellation Ophiuchus" },
                new { Name = "Altair", Ly =  16.73d, Info = "the twelfth-brightest star in the night sky and the brightest star in the constellation Aquila" },
                new { Name = "Alsafi", Ly =  18d, Info = "a star in the constellation Cepheus" },
                new { Name = "Eta Cassiopeiae (Achird)", Ly =  19.33d, Info = "a binary star system in the constellation Cassiopeia" },
                new { Name = "36 Ophiuchi (Guniibuu)", Ly =  19.5d, Info = "a triple star system in the constellation Ophiuchus" },
                new { Name = "Delta Pavonis", Ly =  19.89d, Info = "a nearby star in the constellation Pavo with a high metallicity, making it a candidate for hosting planets" },
                new { Name = "Vega", Ly =  25d, Info = "a star in the constellation Lyra. The fifth-brightest star in the night sky and the second-brightest star in the northern celestial hemisphere" },
                new { Name = "Fomalhaut", Ly =  25.13d, Info = "a bright star in the constellation Piscis Austrinus" },
                new { Name = "Pollux", Ly =  33.78d, Info = "the brightest star in the constellation Gemini" },
                new { Name = "Denebola", Ly =  35.9d, Info = "the second-brightest star in the constellation Leo" },
                new { Name = "Arcturus", Ly =  36.7d, Info = "the brightest star in the constellation Boötes and the fourth-brightest star in the night sky" },
                new { Name = "Capella", Ly =  42.9d, Info = "the brightest star in the constellation Auriga and the sixth-brightest star in the night sky" },
                new { Name = "Rasalhague", Ly =  47.8d, Info = "the brightest star in the constellation Ophiuchus" },
                new { Name = "Alderamin", Ly =  49.1d, Info = "the brightest star in the constellation Cepheus" },
                new { Name = "Castor", Ly =  51.6d, Info = "a multiple star system in the constellation Gemini" },
                new { Name = "Caph", Ly =  53.1d, Info = "a star in the constellation Cassiopeia" },
                new { Name = "Menkent", Ly =  58.8d, Info = "a star in the constellation Centaurus" },
                new { Name = "Aldebaran", Ly =  65.1d, Info = "the brightest star in the constellation Taurus" },
                new { Name = "Larawag", Ly =  66d, Info = "a star in the constellation Auriga" },
                new { Name = "Hamal", Ly =  66.3d, Info = "the brightest star in the constellation Aries" },
                new { Name = "Aljanah", Ly =  72d, Info = "a star in the constellation Cepheus" },
                new { Name = "Alphecca", Ly =  75d, Info = "the brightest star in the constellation Corona Borealis" },
                new { Name = "Ankaa", Ly =  77d, Info = "the brightest star in the constellation Phoenix" },
                new { Name = "Merak", Ly =  79.1d, Info = "a star in the constellation Ursa Major" },
                new { Name = "Regulus", Ly =  79.3d, Info = "the brightest star in the constellation Leo" },
                new { Name = "Alsephina", Ly =  80.6d, Info = "a star in the constellation Centaurus" },
                new { Name = "Menkalinan", Ly =  81.1d, Info = "a star in the constellation Auriga" },
                new { Name = "Alioth", Ly =  82.6d, Info = "a star in the constellation Ursa Major" },
                new { Name = "Mizar", Ly =  83d, Info = "a star in the constellation Ursa Major" },
                new { Name = "Phecda", Ly =  83.2d, Info = "a star in the constellation Ursa Major" },
                new { Name = "Sabik", Ly =  88d, Info = "a star in the constellation Ophiuchus" },
                new { Name = "Gacrux", Ly =  88.6d, Info = "the brightest star in the constellation Crux" },
                new { Name = "Algol", Ly =  94d, Info = "a star in the constellation Perseus" },
                new { Name = "Diphda", Ly =  96.3d, Info = "the brightest star in the constellation Cetus" },
                new { Name = "Alpheratz", Ly =  97d, Info = "the brightest star in the constellation Andromeda" },
                new { Name = "Alnair", Ly =  101d, Info = "the brightest star in the constellation Grus" },
                new { Name = "Alkaid", Ly =  103.9d, Info = "the star at the end of the Big Dipper's handle in the constellation Ursa Major" },
                new { Name = "Alhena", Ly =  109d, Info = "a star in the constellation Gemini" },
                new { Name = "Miaplacidus", Ly =  113.2d, Info = "the second-brightest star in the constellation Carina" },
                new { Name = "Dubhe", Ly =  123d, Info = "a star in the constellation Ursa Major" },
                new { Name = "Muhlifain", Ly =  130d, Info = "a star in the constellation Cepheus" },
                new { Name = "Algieba", Ly =  130.3d, Info = "a star in the constellation Leo" },
                new { Name = "Kochab", Ly =  130.9d, Info = "a star in the constellation Ursa Minor" },
                new { Name = "Elnath", Ly =  134d, Info = "a star in the constellation Taurus" },
                new { Name = "Achernar", Ly =  139d, Info = "the brightest star in the constellation Eridanus" }
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
                new { Name = "Fehu (ᚠ)", From = "5-29", To = "6-14", Brief = "Success and rewards for hard work", Full = "Fehu is all about getting the rewards you've earned - think money, success, and good fortune from your hard work. It's a reminder to be thankful for what you have and to share your good luck with others, which can help you keep the good vibes going." },
                new { Name = "Uruz (ᚢ)", From = "6-14", To = "6-29", Brief = "Good health and inner strength", Full = "Uruz is like a boost of pure energy and strength. It encourages you to tap into your inner power, stay resilient, and keep pushing through tough times. It's about feeling strong, healthy, and ready to take on whatever life throws at you." },
                new { Name = "Thurisaz (ᚦ)", From = "6-29", To = "7-13", Brief = "Protection, caution, wait for the right moment", Full = "Thurisaz is your protective shield, inspired by Thor. It's telling you to pause and think things through before acting. Use your judgment, be careful, and don't rush - sometimes waiting for the right moment is the smartest move." },
                new { Name = "Ansuz (ᚨ)", From = "7-13", To = "7-29", Brief = "Communication and advice from wise people", Full = "Ansuz is all about clear communication and learning from those who've been around the block. Listen to wise friends or mentors, soak up their advice, and let it help you grow smarter and wiser yourself." },
                new { Name = "Raidho (ᚱ)", From = "7-29", To = "8-13", Brief = "Journey, change, moving toward goals", Full = "Raidho is the rune for life's journey - whether you're traveling, moving, or just chasing your dreams. It encourages you to embrace changes, trust the process, and keep moving toward your goals, one step at a time." },
                new { Name = "Kenaz (ᚲ)", From = "8-13", To = "8-28", Brief = "Intuition, clarity, hope", Full = "Kenaz shines a light in dark or confusing times. It helps you find clarity, spark new ideas, and gives you hope when things feel tough. Trust your gut - sometimes your intuition leads you straight to the answers you need." },
                new { Name = "Gebo (ᚷ)", From = "8-28", To = "9-13", Brief = "Harmony and giving in relationships", Full = "Gebo is all about give-and-take in relationships. When you're generous and open, you create harmony and attract kindness in return. It's a reminder that good things happen when you share and connect with others." },
                new { Name = "Wunjo (ᚹ)", From = "9-13", To = "9-28", Brief = "Happiness and well-being", Full = "Wunjo brings happiness and peaceful vibes. It's about enjoying the simple pleasures in life, feeling content, and spreading joy to those around you. Take time to relax and appreciate your own well-being." },
                new { Name = "Hagalaz (ᚺ/ᚻ)", From = "9-28", To = "10-13", Brief = "Change, breaking old habits", Full = "Hagalaz shakes things up, but in a good way - it helps you break old, bad habits and make room for positive changes. Even if things feel chaotic, it's clearing the path for a fresh start and better days ahead." },
                new { Name = "Nauthiz (ᚾ)", From = "10-13", To = "10-28", Brief = "Stay alert and patient in tough times", Full = "Naudhiz is your reminder to stay strong and patient when life gets tough. It's about facing obstacles head-on, staying alert, and learning from every challenge. Don't be afraid - you've got what it takes to get through." },
                new { Name = "Isa (ᛁ)", From = "10-28", To = "11-13", Brief = "Patience, pause, don't rush decisions", Full = "Isa is like a pause button, letting you know it's okay to slow down and take a break. Instead of rushing, use this time to reflect, be patient, and wait for things to clear up before making any big moves." },
                new { Name = "Jera (ᚼ)", From = "11-13", To = "11-28", Brief = "Karma, reward comes with patience", Full = "Jera reminds you that good things take time - like planting seeds and waiting for them to grow. Keep putting in effort, and your rewards will come, even if they take a while. Patience pays off in the end!" },
                new { Name = "Eihwaz (ᚽ)", From = "11-28", To = "0-13", Brief = "Protection in pursuing goals, imagination helps", Full = "Eihwaz is your trusty sidekick when you're working toward your goals. It helps you stay safe and resilient, and encourages you to use your imagination to tackle challenges and find creative solutions." },
                new { Name = "Perthro (ᚹ)", From = "0-13", To = "0-28", Brief = "Mysteries, secrets, surprises", Full = "Perthro is all about uncovering secrets and embracing surprises. It's a reminder to be open to unexpected twists and to trust that even mysteries can lead to important lessons and personal growth." },
                new { Name = "Algiz (ᛉ)", From = "0-28", To = "1-13", Brief = "Protection, awareness, rely on friends", Full = "Algiz offers a protective vibe, keeping you safe from harm. It also encourages you to stay aware, rely on your friends and community, and look forward to happy times ahead." },
                new { Name = "Sowilo (ᛊ)", From = "1-13", To = "1-27", Brief = "Health, relax, let go of worries", Full = "Sowilo is all about feeling good and keeping your energy up. It tells you to relax, let go of worries, and take care of your body - because staying healthy makes everything else easier." },
                new { Name = "Tiwaz (ᛏ)", From = "1-27", To = "2-14", Brief = "Success, willpower, victory, romance", Full = "Tiwaz gives you a boost of confidence and courage. Whether you're chasing goals or starting a new romance, this rune says keep going - you've got the willpower to win and come out on top." },
                new { Name = "Berkano (ᛒ)", From = "2-14", To = "2-30", Brief = "Family, feminine energy, new beginnings", Full = "Berkano is all about nurturing, family, and new beginnings. It often signals happy events like weddings or births, and encourages you to embrace growth and positive changes in your personal life." },
                new { Name = "Ehwaz (ᛖ)", From = "2-30", To = "3-14", Brief = "Movement, change, travel, trust new directions", Full = "Ehwaz is the rune for moving forward and adapting to new things. Whether you're traveling, changing jobs, or starting fresh, trust that these changes are leading you to something better." },
                new { Name = "Mannaz (ᛗ)", From = "3-14", To = "3-29", Brief = "Teamwork, support, expanding contacts", Full = "Mannaz is all about working together and building strong connections. It encourages you to reach out, accept support, and grow your network - success is easier when you're part of a team." },
                new { Name = "Laguz (ᛚ)", From = "3-29", To = "4-14", Brief = "Intuition, imagination, spiritual connection", Full = "Laguz is your guide to trusting your intuition and going with the flow. It helps you tap into your creativity and spiritual side, so you can handle whatever comes your way with grace." },
                new { Name = "Ingwaz (ᛝ)", From = "4-14", To = "4-29", Brief = "Closure, fulfillment, new beginnings", Full = "Ingwaz helps you wrap things up and start fresh. It's about feeling fulfilled, closing old chapters, and stepping confidently into new beginnings with a sense of peace." },
                new { Name = "Othala (ᛟ)", From = "4-29", To = "5-14", Brief = "Heritage, material goods, advice from elders", Full = "Othala connects you to your roots and family traditions. It's about appreciating what you've inherited - whether it's wisdom, values, or material things - and seeking guidance from elders and old friends when you need it." },
                new { Name = "Dagaz (ᛞ)", From = "5-14", To = "5-29", Brief = "Growth, hope, optimism for new times", Full = "Dagaz is like a sunrise after a tough night - bringing hope, growth, and positive change. It's a sign that things are looking up, so stay optimistic and keep moving forward." }
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
                new { Brief = "New beginnings and fresh starts", Full = "Personal Year 1 is characterized by feelings of loneliness and challenges in relationships, as individuals seek greater independence and pursue new beginnings. This shift can cause tension with partners used to doing things together, leading to misunderstandings. People often do not realize their desire for solitude is influenced by this year's vibration, making it hard to explain their actions. The experience varies - some may face situations requiring assertiveness, and failing to seize these opportunities can result in loss or loneliness. Despite the challenges, Personal Year 1 offers a chance for renewal, rest from past responsibilities, and rewards for previous efforts. While important connections may be formed, romantic relationships typically begin in Personal Years 2 or 6." },
                new { Brief = "Cooperation and partnerships", Full = "Personal Year 2 is a time for forming new relationships, increased cooperation, and sharing emotions. People become more understanding and prefer connecting with others rather than being alone. Ideas and opportunities from the previous year can now grow, and maintaining a peaceful environment is important to avoid stress and illness. Overall, it is a period of harmony, support, and personal growth." },
                new { Brief = "Creativity and self-expression", Full = "The vibrations of the number three encourage mental activity, social interaction, and self-expression. This year is not suited for routine tasks; instead, it demands stimulating and imaginative pursuits to channel increased mental energy. If not properly directed, this energy can lead to irritability or sleep disturbances. The period also brings opportunities to resolve past relationships and issues, often through rational communication, which helps alleviate emotional distress. Additionally, it is an ideal time for studying, as mental energy reaches its peak." },
                new { Brief = "Hard work and building foundations", Full = "Personal Year 4 is an active period requiring efficient energy management. People are prone to overwork, ignoring warning signs, and making impulsive decisions that can lead to setbacks. Challenges often arise, making security and future planning a priority; many prefer staying home and focusing on family. This year favors property purchases and investments, but individuals may seek support due to self-doubt and difficulty recognizing their progress. Effective organization and stress management are crucial to avoid health issues. By channeling energy constructively and learning to relax, Personal Year 4 can yield significant success." },
                new { Brief = "Change and freedom", Full = "Personal Year 5 is marked by change and freedom, breaking away from previous constraints. Throughout these twelve months, individuals experience shifts internally and externally, prompting them to embrace new opportunities and let go of old habits. This year prioritizes both intellectual and physical independence, urging people to explore new ideas, enjoy solitude in nature, and make meaningful connections. The energy of number five encourages responsible adaptation, self-discipline, and focus, allowing for high levels of inspiration and progress when goals are set thoughtfully." },
                new { Brief = "Responsibility and nurturing", Full = "Personal Year 6 is marked by increased responsibility, choices, and a focus on family and community. The discipline learned previously is tested as individuals become more aware of their impact on others and their surroundings. This period encourages helping others, balancing creative energy, and managing obligations without becoming overwhelmed. Maintaining personal peace is crucial to avoid bitterness and health issues. Support from loved ones is important for making decisions and sustaining healthy relationships. Year 6 is ideal for building new connections, strengthening existing ones, and resolving family conflicts, often resulting in greater harmony and attractiveness to others." },
                new { Brief = "Analysis and understanding", Full = "The number seven is linked to emotional changes, challenges in relationships, and personal growth. During Personal Year 7, individuals face spiritual lessons and may experience loss or illness. This period encourages self-reliance, compassion, and understanding, both toward oneself and others. Helping others can put one's problems in perspective, fostering personal development. Approaching situations with patience and sincere support leads to meaningful progress in relationships and inner strength." },
                new { Brief = "Attainment and capital gains", Full = "Personal Year 8 is a time to restore balance, build on past experience, and reap rewards from previous efforts. Success hinges on hard work and integrity, as karma reflects one's actions. This period offers opportunities for creativity and independence if energy is used wisely. Patience and wisdom help maintain strength and balance. Expect improvements in health, career, and relationships - act honestly, stay positive, and communicate effectively to achieve success." },
                new { Brief = "Reflection and reaching out", Full = "Personal Year 9 marks the end of cycles and prepares individuals for new beginnings in Personal Year 1. This period can bring feelings of instability, prompting change even if it's uncomfortable. While it may cause confusion and difficulty - sometimes ending relationships or leading to significant life changes - it also offers growth in personal awareness and responsibility. People often reorganize priorities and take on new responsibilities, fostering self-development. Though challenging, Personal Year 9 provides valuable opportunities for learning and setting the stage for future progress." }
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
            string amount = useMetric ? $"{totalCO2} billion tonnes" : $"{(totalCO2 * 0.984252):F2} billion imperial tons";

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
            amount = useMetric ? $"{totalCO2:F2} billion tonnes" : $"{(totalCO2 * 0.984252):F2} billion imperial tons";

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
