using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculatePersonalYear"/>.
    ///
    /// Algorithm: digital root of (year_root + month_root + day_root).
    /// If result == 0 the method substitutes 9.
    /// </summary>
    public class CalculatePersonalYearTests
    {
        private readonly CalculationService _svc;

        public CalculatePersonalYearTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Theory]
        //  baseDate       currentYear  expected personalYear
        //  month=7, day=24 -> month_root=7, day_root=6
        //  2026 -> year_root=1  => 1+7+6=14 -> 5
        [InlineData(1965, 7, 24, 2026, 5)]
        //  month=1, day=1 -> roots both 1
        //  2026 -> 1   => 1+1+1=3
        [InlineData(2000, 1, 1,  2026, 3)]
        //  month=9, day=9 -> roots 9,9
        //  2024 -> year_root=8  => 8+9+9=26 -> 8
        [InlineData(1990, 9, 9,  2024, 8)]
        public void KnownInputs_ReturnExpectedPersonalYear(
            int bYear, int bMonth, int bDay, int currentYear, int expected)
        {
            var baseDate = new DateTime(bYear, bMonth, bDay);
            var now      = new DateTime(currentYear, 6, 1); // month/day irrelevant

            var result = _svc.CalculatePersonalYear(baseDate, baseDate.ToString("yyyy-MM-dd"), now);

            Assert.Contains(expected.ToString(), result.BriefText);
            Assert.Contains(currentYear.ToString(), result.BriefText);
        }

        [Fact]
        public void ResultIsNeverZero()
        {
            // Edge: inputs that would produce a raw sum of 0 (e.g. day=9 month=9
            // year digital-root=9 -> 27 -> 9 -> result should be 9, not 0)
            var baseDate = new DateTime(1999, 9, 9);
            var now      = new DateTime(2016, 1, 1); // 2+0+1+6=9

            var result = _svc.CalculatePersonalYear(baseDate, "1999-09-09", now);

            Assert.DoesNotContain("Year 0", result.BriefText);
        }

        [Fact]
        public void FullTextContainsCurrentYear()
        {
            var baseDate = new DateTime(1980, 3, 15);
            var now      = new DateTime(2026, 6, 1);

            var result = _svc.CalculatePersonalYear(baseDate, "1980-03-15", now);

            Assert.Contains("2026", result.FullText);
        }
    }
}
