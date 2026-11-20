using System;
using NUnit.Framework;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class ZoneRulesTests
    {
        private TestAssets _assets;
        private ZoneTypeDefinition _normal;
        private ZoneTypeDefinition _safe;
        private ZoneTypeDefinition _super;
        private WheelDefinition _earlyWheel;
        private WheelDefinition _lateWheel;
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            _assets = new TestAssets();
            var reward = _assets.Reward();
            _earlyWheel = _assets.Wheel(new WheelSlice(reward, 1), WheelSlice.Bomb());
            _lateWheel = _assets.Wheel(new WheelSlice(reward, 5), WheelSlice.Bomb());
            var safeWheel = _assets.Wheel(new WheelSlice(reward, 1));

            _normal = _assets.ZoneType(0, false, new WheelTier(1, _earlyWheel), new WheelTier(10, _lateWheel));
            _safe = _assets.ZoneType(5, true, new WheelTier(1, safeWheel));
            _super = _assets.ZoneType(30, true, new WheelTier(1, safeWheel));
            _rules = new ZoneRules(new[] { _normal, _safe, _super }, 0.1f);
        }

        [TearDown]
        public void TearDown()
        {
            _assets.DestroyAll();
        }

        [TestCase(1, 0)]
        [TestCase(4, 0)]
        [TestCase(5, 5)]
        [TestCase(25, 5)]
        [TestCase(30, 30)]
        [TestCase(35, 5)]
        [TestCase(60, 30)]
        public void GetZoneType_PicksTheLargestMatchingInterval(int zone, int expectedInterval)
        {
            Assert.AreEqual(expectedInterval, _rules.GetZoneType(zone).Interval);
        }

        [Test]
        public void CanLeaveFrom_OnlySafeZoneTypes()
        {
            Assert.IsFalse(_rules.CanLeaveFrom(_normal));
            Assert.IsTrue(_rules.CanLeaveFrom(_safe));
            Assert.IsTrue(_rules.CanLeaveFrom(_super));
        }

        [TestCase(1, 5)]
        [TestCase(5, 5)]
        [TestCase(26, 35)]
        public void GetNextZoneOfType_Safe_SkipsSuperZones(int fromZone, int expected)
        {
            Assert.AreEqual(expected, _rules.GetNextZoneOfType(fromZone, _safe));
        }

        [TestCase(1, 30)]
        [TestCase(31, 60)]
        public void GetNextZoneOfType_Super(int fromZone, int expected)
        {
            Assert.AreEqual(expected, _rules.GetNextZoneOfType(fromZone, _super));
        }

        [Test]
        public void GetWheel_UsesTheHighestUnlockedTier()
        {
            Assert.AreSame(_earlyWheel, _rules.GetWheel(9));
            Assert.AreSame(_lateWheel, _rules.GetWheel(11));
        }

        [TestCase(100, 1, 100)]
        [TestCase(100, 2, 110)]
        [TestCase(100, 11, 200)]
        public void GetScaledAmount_GrowsWithZone(int baseAmount, int zone, int expected)
        {
            Assert.AreEqual(expected, _rules.GetScaledAmount(baseAmount, zone));
        }

        [Test]
        public void Constructor_WithoutDefaultZoneType_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ZoneRules(new[] { _safe, _super }, 0f));
        }

        [Test]
        public void Constructor_WithTwoDefaultZoneTypes_Throws()
        {
            var secondDefault = _assets.ZoneType(0, false);
            Assert.Throws<ArgumentException>(() => new ZoneRules(new[] { _normal, secondDefault }, 0f));
        }
    }
}
