using NUnit.Framework;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.Tests
{
    public class AmountFormatterTests
    {
        [TestCase(0, "0")]
        [TestCase(578, "578")]
        [TestCase(9999, "9999")]
        [TestCase(10000, "10K")]
        [TestCase(12500, "12.5K")]
        [TestCase(1500000, "1.5M")]
        public void Format_ShortensLargeAmounts(int amount, string expected)
        {
            Assert.AreEqual(expected, AmountFormatter.Format(amount));
        }

        [Test]
        public void FormatMultiplier_AddsPrefix()
        {
            Assert.AreEqual("x300", AmountFormatter.FormatMultiplier(300));
        }
    }
}
