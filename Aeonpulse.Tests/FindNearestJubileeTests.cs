using Aeonpulse.Services;
using Aeonpulse.Tests.Helpers;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.FindNearestJubilee"/>.
    ///
    /// The method is a pure function; every case is fully deterministic.
    /// Tests cover all four jubilee families (major power-of-10, minor
    /// leading-digit, quarter fractions, repeating-digit "nice" numbers)
    /// and boundary values.
    /// </summary>
    public class FindNearestJubileeTests
    {
        // --- Major power-of-10 jubilee wins --------------------------------

        [Theory]
        [InlineData(9,   10)]    // just below 10
        [InlineData(99,  100)]   // just below 100
        [InlineData(999, 1000)]  // just below 1000
        public void ReturnsNextPowerOfTen_WhenNoCandidateIsSmaller(long input, long expected)
        {
            Assert.Equal(expected, CalculationService.FindNearestJubilee(input));
        }

        // --- Minor leading-digit jubilee wins ------------------------------

        [Theory]
        [InlineData(5,  6)]     // diff=5, minor = ceil(5.5/1)*1 = 6
        [InlineData(12, 20)]    // diff=12, minor=20 beats quarter=25
        [InlineData(21, 25)]    // diff=21, quarter=25 beats minor=30
        [InlineData(45, 50)]    // diff=45, minor=50 ties quarter=50 -> 50
        public void ReturnsMinorJubilee_WhenSmallerThanMajor(long input, long expected)
        {
            Assert.Equal(expected, CalculationService.FindNearestJubilee(input));
        }

        // --- Quarter jubilee wins ------------------------------------------

        [Theory]
        [InlineData(11,  20)]    // diff=11, minor=20 beats quarter=25
        [InlineData(26,  30)]    // diff=26, minor=30 beats quarter=50
        [InlineData(51,  60)]    // diff=51, minor=60 beats quarter=75
        [InlineData(260, 300)]   // diff=260, minor=300 beats quarter=500
        public void ReturnsQuarterJubilee_WhenSmallerThanOtherCandidates(long input, long expected)
        {
            Assert.Equal(expected, CalculationService.FindNearestJubilee(input));
        }

        // --- Repeating-digit "nice" jubilee wins ---------------------------

        [Theory]
        [InlineData(100,  111)]   // 111 < next minor (200), < major (1000)
        [InlineData(200,  222)]
        [InlineData(1000, 1111)]
        [InlineData(1112, 2000)]  // 1111 already passed, next minor = 2000
        public void ReturnsNiceJubilee_WhenSmallerThanOtherCandidates(long input, long expected)
        {
            Assert.Equal(expected, CalculationService.FindNearestJubilee(input));
        }

        // --- Always returns a value strictly greater than the input --------

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(50)]
        [InlineData(500)]
        [InlineData(12345)]
        public void ReturnValueIsAlwaysGreaterThanInput(long input)
        {
            Assert.True(CalculationService.FindNearestJubilee(input) > input);
        }
    }
}
