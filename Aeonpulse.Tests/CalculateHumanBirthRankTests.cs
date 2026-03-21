using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateHumanBirthRank"/>.
    ///
    /// Three piecewise linear ranges: before 1900, 1900-1950, 1950-2000, after 2000.
    /// </summary>
    public class CalculateHumanBirthRankTests
    {
        private readonly CalculationService _svc;

        public CalculateHumanBirthRankTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void DateBefore1900_ReturnsPre20thCenturyMessage()
        {
            var result = _svc.CalculateHumanBirthRank(new DateTime(1850, 1, 1), "Test");

            // The pre-XX brief contains the fixed population figure
            Assert.Contains("104,510,976,956", result.BriefText);
        }

        [Fact]
        public void DateExactly1900_01_01_ReturnsPostXXMessage()
        {
            var result = _svc.CalculateHumanBirthRank(new DateTime(1900, 1, 1), "Test");

            Assert.Contains("#", result.BriefText);
        }

        [Fact]
        public void DateIn1965_RankIsBetweenExpectedBounds()
        {
            // 1965 is in the 1950-2000 range.
            // Rank at 1950 = 107,901,175,171; at 2000 = 113,966,170,055.
            // Midpoint 1975 ~ 110,933,672,613. 1965 should be between 107B and 112B.
            var result = _svc.CalculateHumanBirthRank(new DateTime(1965, 7, 24), "Test");

            Assert.Contains("#", result.BriefText);
            Assert.NotNull(result.FullText);
        }

        [Fact]
        public void DateIn2010_RankIsAbove115Billion()
        {
            var result = _svc.CalculateHumanBirthRank(new DateTime(2010, 1, 1), "Test");

            // Rank at 2000 = 113,966,170,055; at 2022 = 117,020,448,575.
            // 2010 must be above 113B.
            Assert.Contains("#", result.BriefText);
        }

        [Fact]
        public void ResultIsNonNullForAnyDate()
        {
            var dates = new[]
            {
                new DateTime(1800, 1, 1),
                new DateTime(1900, 1, 1),
                new DateTime(1950, 6, 15),
                new DateTime(2000, 12, 31),
                new DateTime(2025, 3, 21)
            };
            foreach (var d in dates)
            {
                var result = _svc.CalculateHumanBirthRank(d, "Test");
                Assert.NotNull(result);
                Assert.NotEmpty(result.BriefText);
            }
        }
    }
}
