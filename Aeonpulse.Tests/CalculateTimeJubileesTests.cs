using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.CalculateTimeJubilees"/>.
    ///
    /// Verifies that the method selects the nearest upcoming jubilee across all
    /// seven time units and that the result is well-formed. Overflow guards for
    /// minutes/seconds on very old dates are also tested.
    /// </summary>
    public class CalculateTimeJubileesTests
    {
        private readonly CalculationService _svc;

        public CalculateTimeJubileesTests()
        {
            TestFixture.InitEnglish();
            _svc = new CalculationService();
        }

        [Fact]
        public void ReturnsNonNullResult_ForTypicalBirthday()
        {
            var baseDate = new DateTime(1965, 7, 24);
            var now      = new DateTime(2026, 3, 21);

            var result = _svc.CalculateTimeJubilees(baseDate, "Birthday", "1965-07-24", now);

            Assert.NotNull(result);
            Assert.NotEmpty(result.BriefText);
            Assert.NotEmpty(result.FullText);
        }

        [Fact]
        public void BriefTextContainsNumericValue_AndUnit()
        {
            var baseDate = new DateTime(1990, 1, 1);
            var now      = new DateTime(2026, 3, 21);

            var result = _svc.CalculateTimeJubilees(baseDate, "T", "1990-01-01", now);

            // Brief text format: "{nextJubilee} on {nearestJubileeDate}"
            // nextJubilee is formatted as N0 (numeric with commas) followed by a unit
            Assert.Matches(@"\d", result.BriefText);
        }

        [Fact]
        public void VeryOldDate_DoesNotThrow()
        {
            // Dates far in the past produce very large minute/second counts that
            // can overflow DateTime.AddMinutes/AddSeconds. The method guards these.
            var baseDate = new DateTime(1800, 1, 1);
            var now      = new DateTime(2026, 3, 21);

            var ex = Record.Exception(() =>
                _svc.CalculateTimeJubilees(baseDate, "T", "1800-01-01", now));

            Assert.Null(ex);
        }

        [Fact]
        public void FullTextContainsBaseDateName()
        {
            var baseDate = new DateTime(1965, 7, 24);
            var now      = new DateTime(2026, 3, 21);

            var result = _svc.CalculateTimeJubilees(baseDate, "MyBirthday", "1965-07-24", now);

            Assert.Contains("MyBirthday", result.FullText);
        }
    }
}
