using NUnit.Framework;
using UnityEditor;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class ShippedConfigurationTests
    {
        private const string ZoneSettingsPath = "Assets/_Project/Data/zone_settings.asset";
        private const string EconomySettingsPath = "Assets/_Project/Data/economy_settings.asset";
        private const int ZonesToCheck = 120;

        private ZoneSettings _zoneSettings;
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            _zoneSettings = AssetDatabase.LoadAssetAtPath<ZoneSettings>(ZoneSettingsPath);
            Assert.IsNotNull(_zoneSettings, $"Missing {ZoneSettingsPath}");
            _rules = _zoneSettings.CreateRules();
        }

        [TestCase(1, false)]
        [TestCase(4, false)]
        [TestCase(5, true)]
        [TestCase(6, false)]
        [TestCase(10, true)]
        [TestCase(29, false)]
        [TestCase(30, true)]
        public void Player_CanLeaveOnlyAtSafeAndSuperZones(int zone, bool expected)
        {
            Assert.AreEqual(expected, _rules.CanLeaveFrom(_rules.GetZoneType(zone)));
        }

        [Test]
        public void EverySafeZoneIsBombFree_AndEveryRiskyZoneHasABomb()
        {
            for (var zone = 1; zone <= ZonesToCheck; zone++)
            {
                var zoneType = _rules.GetZoneType(zone);
                var wheel = _rules.GetWheel(zone);
                Assert.IsNotNull(wheel, $"Zone {zone} has no wheel");
                Assert.AreEqual(!zoneType.IsSafe, wheel.HasBomb, $"Zone {zone} ({zoneType.name})");
            }
        }

        [Test]
        public void EveryZoneType_HasAWheelFromZoneOne()
        {
            foreach (var zoneType in _zoneSettings.ZoneTypes)
            {
                Assert.IsNotNull(zoneType.GetWheel(1), zoneType.name);
            }
        }

        [Test]
        public void NormalZones_UnlockNewWheelsAsTheRunProgresses()
        {
            var firstWheel = _rules.GetWheel(1);
            var changes = 0;
            var previous = firstWheel;
            for (var zone = 2; zone <= ZonesToCheck; zone++)
            {
                if (_rules.GetZoneType(zone).IsSafe)
                {
                    continue;
                }

                var wheel = _rules.GetWheel(zone);
                if (wheel != previous)
                {
                    changes++;
                    previous = wheel;
                }
            }

            Assert.Greater(changes, 0, "Normal zones always use the same wheel");
        }

        [Test]
        public void SessionBuiltFromShippedSettings_DoesNotAllowLeavingAtStart()
        {
            var economySettings = AssetDatabase.LoadAssetAtPath<EconomySettings>(EconomySettingsPath);
            Assert.IsNotNull(economySettings, $"Missing {EconomySettingsPath}");
            var session = new GameSession(_rules, economySettings.CreateRules(), new FixedRandomProvider(), new FakeWallet(0), new EventBus());

            session.StartRun();

            Assert.IsFalse(session.CanLeave);
        }

        [Test]
        public void ShippedEconomy_ChargesForRevive()
        {
            var economy = AssetDatabase.LoadAssetAtPath<EconomySettings>(EconomySettingsPath).CreateRules();

            Assert.Greater(economy.ReviveCost, 0);
            Assert.IsNotNull(economy.CurrencyReward);
        }
    }
}
