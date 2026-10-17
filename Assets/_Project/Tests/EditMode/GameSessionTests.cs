using NUnit.Framework;
using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class GameSessionTests
    {
        private const int RewardSliceIndex = 0;
        private const int BombSliceIndex = 1;

        private RewardDefinition _gold;
        private FakeWheelProvider _wheels;
        private FixedRandomProvider _random;
        private GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _gold = ScriptableObject.CreateInstance<RewardDefinition>();
            _gold.Initialize(null);

            var normalWheel = CreateWheel(new WheelSlice(_gold, 100), WheelSlice.Bomb());
            var safeWheel = CreateWheel(new WheelSlice(_gold, 100), new WheelSlice(_gold, 200));
            _wheels = new FakeWheelProvider(normalWheel, safeWheel, safeWheel);
            _random = new FixedRandomProvider();
            _session = new GameSession(new ZoneRules(5, 30, 0f), _wheels, _random);
            _session.StartRun();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gold);
            _wheels.Destroy();
        }

        [Test]
        public void Start_BeginsAtZoneOneAndIdle()
        {
            Assert.AreEqual(1, _session.CurrentZone);
            Assert.AreEqual(SessionState.Idle, _session.State);
            Assert.IsTrue(_session.Inventory.IsEmpty);
        }

        [Test]
        public void RewardSpin_AddsRewardAndAdvancesZone()
        {
            Spin(RewardSliceIndex);

            Assert.AreEqual(100, _session.Inventory.GetAmount(_gold));
            Assert.AreEqual(2, _session.CurrentZone);
            Assert.AreEqual(SessionState.Idle, _session.State);
        }

        [Test]
        public void BombSpin_SetsLostState()
        {
            Spin(BombSliceIndex);

            Assert.AreEqual(SessionState.Lost, _session.State);
            Assert.IsFalse(_session.CanSpin);
        }

        [Test]
        public void GiveUp_AfterBomb_ClearsRewardsAndRestarts()
        {
            Spin(RewardSliceIndex);
            Spin(BombSliceIndex);

            Assert.IsTrue(_session.TryGiveUp());

            Assert.IsTrue(_session.Inventory.IsEmpty);
            Assert.AreEqual(1, _session.CurrentZone);
        }

        [Test]
        public void Revive_AfterBomb_KeepsRewards()
        {
            Spin(RewardSliceIndex);
            Spin(BombSliceIndex);

            Assert.IsTrue(_session.TryRevive());
            Assert.AreEqual(100, _session.Inventory.GetAmount(_gold));
            Assert.AreEqual(SessionState.Idle, _session.State);
        }

        [Test]
        public void CannotSpinWhileSpinning()
        {
            _random.NextValue = RewardSliceIndex;
            _session.TryBeginSpin(out _);

            Assert.IsFalse(_session.TryBeginSpin(out _));
        }

        [Test]
        public void CannotLeaveInNormalZone()
        {
            Assert.IsFalse(_session.TryLeave());
        }

        [Test]
        public void CanLeaveInNormalZone_WhenRulesAllowIt()
        {
            var session = new GameSession(new ZoneRules(5, 30, 0f, true), _wheels, _random);
            session.StartRun();

            Assert.IsTrue(session.TryLeave());
        }

        [Test]
        public void CanLeaveInSafeZone()
        {
            for (var i = 0; i < 4; i++)
            {
                Spin(RewardSliceIndex);
            }

            Assert.AreEqual(ZoneType.Safe, _session.CurrentZoneType);
            Assert.IsTrue(_session.TryLeave());
            Assert.AreEqual(SessionState.Collected, _session.State);
        }

        [Test]
        public void CannotLeaveWhileSpinning()
        {
            for (var i = 0; i < 4; i++)
            {
                Spin(RewardSliceIndex);
            }

            _session.TryBeginSpin(out _);

            Assert.IsFalse(_session.TryLeave());
        }

        private void Spin(int sliceIndex)
        {
            _random.NextValue = sliceIndex;
            _session.TryBeginSpin(out _);
            _session.CompleteSpin();
        }

        private static WheelDefinition CreateWheel(params WheelSlice[] slices)
        {
            var wheel = ScriptableObject.CreateInstance<WheelDefinition>();
            wheel.Initialize(slices);
            return wheel;
        }

        private sealed class FixedRandomProvider : IRandomProvider
        {
            public int NextValue { get; set; }

            public int Range(int minInclusive, int maxExclusive) => NextValue;
        }

        private sealed class FakeWheelProvider : IWheelProvider
        {
            private readonly WheelDefinition _normal;
            private readonly WheelDefinition _safe;
            private readonly WheelDefinition _super;

            public FakeWheelProvider(WheelDefinition normal, WheelDefinition safe, WheelDefinition super)
            {
                _normal = normal;
                _safe = safe;
                _super = super;
            }

            public WheelDefinition GetWheel(ZoneType zoneType)
            {
                switch (zoneType)
                {
                    case ZoneType.Super:
                        return _super;
                    case ZoneType.Safe:
                        return _safe;
                    default:
                        return _normal;
                }
            }

            public void Destroy()
            {
                Object.DestroyImmediate(_normal);
                Object.DestroyImmediate(_safe);
                if (_super != _safe)
                {
                    Object.DestroyImmediate(_super);
                }
            }
        }
    }
}
