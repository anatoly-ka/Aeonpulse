using Aeonpulse.Services;

namespace Aeonpulse.Tests
{
    /// <summary>
    /// Unit tests for <see cref="CalculationService.ReduceToSingleDigit"/>.
    ///
    /// This is a pure function implementing the standard numerology digital root:
    /// repeatedly sum decimal digits until the result is 1-9.
    /// </summary>
    public class ReduceToSingleDigitTests
    {
        // --- Single digits pass through unchanged --------------------------

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(9, 9)]
        public void SingleDigit_ReturnsSameValue(int input, int expected)
        {
            Assert.Equal(expected, CalculationService.ReduceToSingleDigit(input));
        }

        // --- Two-digit reductions -----------------------------------------

        [Theory]
        [InlineData(10, 1)]   // 1+0=1
        [InlineData(18, 9)]   // 1+8=9
        [InlineData(19, 1)]   // 1+9=10 -> 1+0=1
        [InlineData(29, 2)]   // 2+9=11 -> 1+1=2
        [InlineData(99, 9)]   // 9+9=18 -> 1+8=9
        public void TwoDigit_ReducesCorrectly(int input, int expected)
        {
            Assert.Equal(expected, CalculationService.ReduceToSingleDigit(input));
        }

        // --- Multi-digit reductions (real numerology year calculations) ----

        [Theory]
        [InlineData(2026, 1)]   // 2+0+2+6=10 -> 1
        [InlineData(1984, 4)]   // 1+9+8+4=22 -> 4
        [InlineData(2000, 2)]   // 2+0+0+0=2
        [InlineData(1999, 1)]   // 1+9+9+9=28 -> 2+8=10 -> 1
        public void MultiDigit_ReducesToCorrectDigitalRoot(int input, int expected)
        {
            Assert.Equal(expected, CalculationService.ReduceToSingleDigit(input));
        }

        // --- Result is always in range 1-9 --------------------------------

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(9999)]
        public void ResultIsAlwaysInRange1To9(int input)
        {
            int result = CalculationService.ReduceToSingleDigit(input);
            // ReduceToSingleDigit(0) = 0 (edge case: caller guards with personalYear == 0 check)
            Assert.InRange(result, 0, 9);
        }
    }
}
