using System;
using NUnit.Framework;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class ZoneRulesTests
    {
        #region Setup

        private ZoneTypeModel _normal;
        private ZoneTypeModel _safe;
        private ZoneTypeModel _super;
        private WheelModel _earlyWheel;
        private WheelModel _lateWheel;
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            var reward = TestModels.Item("item");
            _earlyWheel = TestModels.Wheel(new SliceModel(reward, 1), SliceModel.Bomb());
            _lateWheel = TestModels.Wheel(new SliceModel(reward, 5), SliceModel.Bomb());
            var safeWheel = TestModels.Wheel(new SliceModel(reward, 1));

            _normal = TestModels.ZoneType(0, false, new WheelTierModel(1, _earlyWheel), new WheelTierModel(10, _lateWheel));
            _safe = TestModels.ZoneType(5, true, new WheelTierModel(1, safeWheel));
            _super = TestModels.ZoneType(30, true, new WheelTierModel(1, safeWheel));
            _rules = new ZoneRules(new[] { _normal, _safe, _super }, 0.1f);
        }

        #endregion

        #region Tests

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
        public void GetZoneType_DoesNotDependOnListOrder()
        {
            var rules = new ZoneRules(new[] { _super, _safe, _normal }, 0.1f);

            Assert.AreSame(_super, rules.GetZoneType(30));
            Assert.AreSame(_safe, rules.GetZoneType(35));
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
            Assert.Throws<ArgumentException>(CreateRulesWithoutDefault);
        }

        [Test]
        public void Constructor_WithTwoDefaultZoneTypes_Throws()
        {
            Assert.Throws<ArgumentException>(CreateRulesWithTwoDefaults);
        }

        #endregion

        #region Helpers

        private void CreateRulesWithoutDefault()
        {
            new ZoneRules(new[] { _safe, _super }, 0f);
        }

        private void CreateRulesWithTwoDefaults()
        {
            new ZoneRules(new[] { _normal, TestModels.ZoneType(0, false) }, 0f);
        }

        #endregion
    }
}
