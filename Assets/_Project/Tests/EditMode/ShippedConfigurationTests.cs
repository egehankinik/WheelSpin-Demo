using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using WheelOfFortune.Configuration;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.UI.Wheel;

namespace WheelOfFortune.Tests
{
    public class ShippedConfigurationTests
    {
        #region Setup

        private const string ZONE_SETTINGS_PATH = "Assets/_Project/Data/zone_settings.asset";
        private const string ECONOMY_SETTINGS_PATH = "Assets/_Project/Data/economy_settings.asset";
        private const string WHEEL_PANEL_PATH = "Assets/_Project/Prefabs/UI/ui_panel_wheel.prefab";
        private const int ZONES_TO_CHECK = 120;

        private GameConfiguration _configuration;
        private ZoneRules _rules;

        [SetUp]
        public void SetUp()
        {
            var zoneSettings = AssetDatabase.LoadAssetAtPath<ZoneSettings>(ZONE_SETTINGS_PATH);
            var economySettings = AssetDatabase.LoadAssetAtPath<EconomySettings>(ECONOMY_SETTINGS_PATH);
            Assert.IsTrue(zoneSettings, $"Missing {ZONE_SETTINGS_PATH}");
            Assert.IsTrue(economySettings, $"Missing {ECONOMY_SETTINGS_PATH}");

            _configuration = new GameConfiguration(zoneSettings, economySettings);
            _rules = _configuration.ZoneRules;
        }

        #endregion

        #region Tests

        [TestCase(1, false)]
        [TestCase(4, false)]
        [TestCase(5, true)]
        [TestCase(6, false)]
        [TestCase(10, true)]
        [TestCase(29, false)]
        [TestCase(30, true)]
        public void Player_CanLeaveOnlyAtSafeAndSuperZones(int zone, bool isLeaveExpected)
        {
            Assert.AreEqual(isLeaveExpected, _rules.CanLeaveFrom(_rules.GetZoneType(zone)));
        }

        [Test]
        public void EverySafeZoneIsBombFree_AndEveryRiskyZoneHasABomb()
        {
            for (var zone = 1; zone <= ZONES_TO_CHECK; zone++)
            {
                var zoneType = _rules.GetZoneType(zone);
                var wheel = _rules.GetWheel(zone);
                Assert.IsNotNull(wheel, $"Zone {zone} has no wheel");
                Assert.AreEqual(!zoneType.IsSafe, wheel.HasBomb, $"Zone {zone} ({_configuration.GetDefinition(zoneType).name})");
            }
        }

        [Test]
        public void NormalZones_UnlockNewWheelsAsTheRunProgresses()
        {
            var previous = _rules.GetWheel(1);
            var changes = 0;
            for (var zone = 2; zone <= ZONES_TO_CHECK; zone++)
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
        public void EveryShippedWheel_FitsTheWheelView()
        {
            var wheelPanel = AssetDatabase.LoadAssetAtPath<GameObject>(WHEEL_PANEL_PATH);
            Assert.IsTrue(wheelPanel, $"Missing {WHEEL_PANEL_PATH}");
            var wheelView = wheelPanel.GetComponentInChildren<WheelViewMono>(true);
            Assert.IsTrue(wheelView, "The wheel panel has no wheel view");

            foreach (var wheel in _configuration.WheelDefinitions)
            {
                Assert.IsTrue(wheelView.CanDisplay(wheel), $"{wheel.name} has {wheel.SliceCount} slices, the view has {wheelView.SliceCapacity}");
            }
        }

        [Test]
        public void SessionBuiltFromShippedSettings_DoesNotAllowLeavingAtStart()
        {
            var session = new GameSession(_rules, _configuration.EconomyRules, new FixedRandomProvider(), new FakeWallet(), new EventBus());

            session.TryStartRun();

            Assert.IsFalse(session.IsLeaveAllowed);
        }

        [Test]
        public void ShippedEconomy_ChargesACurrencyForRevive()
        {
            var economy = _configuration.EconomyRules;

            Assert.Greater(economy.ReviveCost, 0);
            Assert.IsTrue(economy.ReviveCurrency.IsCurrency);
            Assert.GreaterOrEqual(economy.ReviveCurrency.StartingBalance, economy.ReviveCost);
        }

        #endregion
    }
}
