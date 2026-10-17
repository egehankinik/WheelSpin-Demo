using NUnit.Framework;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class ZoneRulesTests
    {
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            _rules = new ZoneRules(5, 30, 0.1f);
        }

        [TestCase(1, ZoneType.Normal)]
        [TestCase(4, ZoneType.Normal)]
        [TestCase(5, ZoneType.Safe)]
        [TestCase(25, ZoneType.Safe)]
        [TestCase(30, ZoneType.Super)]
        [TestCase(35, ZoneType.Safe)]
        [TestCase(60, ZoneType.Super)]
        public void GetZoneType_ReturnsExpectedType(int zone, ZoneType expected)
        {
            Assert.AreEqual(expected, _rules.GetZoneType(zone));
        }

        [TestCase(1, ZoneType.Safe, 5)]
        [TestCase(5, ZoneType.Safe, 5)]
        [TestCase(26, ZoneType.Safe, 35)]
        [TestCase(1, ZoneType.Super, 30)]
        [TestCase(31, ZoneType.Super, 60)]
        public void GetNextZoneOfType_SkipsOtherZoneTypes(int fromZone, ZoneType zoneType, int expected)
        {
            Assert.AreEqual(expected, _rules.GetNextZoneOfType(fromZone, zoneType));
        }

        [TestCase(100, 1, 100)]
        [TestCase(100, 2, 110)]
        [TestCase(100, 11, 200)]
        public void GetScaledAmount_GrowsWithZone(int baseAmount, int zone, int expected)
        {
            Assert.AreEqual(expected, _rules.GetScaledAmount(baseAmount, zone));
        }
    }
}
