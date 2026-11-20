using NUnit.Framework;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.Gameplay.Events;

namespace WheelOfFortune.Tests
{
    public class GameSessionTests
    {
        private const int RewardSliceIndex = 0;
        private const int BombSliceIndex = 1;
        private const int ReviveCost = 25;
        private const int StartingBalance = 100;

        private TestAssets _assets;
        private RewardDefinition _gold;
        private ZoneTypeDefinition _normal;
        private ZoneTypeDefinition _safe;
        private ZoneTypeDefinition _super;
        private FixedRandomProvider _random;
        private FakeWallet _wallet;
        private EventBus _events;
        private GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _assets = new TestAssets();
            _gold = _assets.Reward();
            var riskyWheel = _assets.Wheel(new WheelSlice(_gold, 100), WheelSlice.Bomb());
            var safeWheel = _assets.Wheel(new WheelSlice(_gold, 100), new WheelSlice(_gold, 200));
            _normal = _assets.ZoneType(0, false, new WheelTier(1, riskyWheel));
            _safe = _assets.ZoneType(5, true, new WheelTier(1, safeWheel));
            _super = _assets.ZoneType(30, true, new WheelTier(1, safeWheel));

            _random = new FixedRandomProvider();
            _events = new EventBus();
            _session = CreateSession(StartingBalance);
            _session.StartRun();
        }

        [TearDown]
        public void TearDown()
        {
            _assets.DestroyAll();
        }

        [Test]
        public void StartRun_BeginsAtZoneOneAndIdle()
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
        public void RewardSpin_PublishesZoneChangedWithZoneType()
        {
            ZoneChangedEvent received = default;
            _events.Subscribe<ZoneChangedEvent>(e => received = e);

            SpinTo(5);

            Assert.AreEqual(5, received.Zone);
            Assert.AreSame(_safe, received.ZoneType);
        }

        [Test]
        public void BombSpin_SetsLostStateAndPublishesReviveCost()
        {
            BombHitEvent received = default;
            _events.Subscribe<BombHitEvent>(e => received = e);

            Spin(BombSliceIndex);

            Assert.AreEqual(SessionState.Lost, _session.State);
            Assert.IsFalse(_session.CanSpin);
            Assert.AreEqual(ReviveCost, received.ReviveCost);
            Assert.IsTrue(received.CanAffordRevive);
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
        public void Revive_AfterBomb_SpendsCostAndKeepsRewards()
        {
            Spin(RewardSliceIndex);
            Spin(BombSliceIndex);

            Assert.IsTrue(_session.TryRevive());

            Assert.AreEqual(StartingBalance - ReviveCost, _wallet.Balance);
            Assert.AreEqual(100, _session.Inventory.GetAmount(_gold));
            Assert.AreEqual(SessionState.Idle, _session.State);
        }

        [Test]
        public void Revive_WithoutEnoughCurrency_FailsAndKeepsBalance()
        {
            _session = CreateSession(ReviveCost - 1);
            _session.StartRun();
            Spin(BombSliceIndex);

            Assert.IsFalse(_session.CanRevive);
            Assert.IsFalse(_session.TryRevive());
            Assert.AreEqual(ReviveCost - 1, _wallet.Balance);
            Assert.AreEqual(SessionState.Lost, _session.State);
        }

        [Test]
        public void Revive_WhenNotLost_Fails()
        {
            Assert.IsFalse(_session.TryRevive());
            Assert.AreEqual(StartingBalance, _wallet.Balance);
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
            Assert.AreSame(_normal, _session.CurrentZoneType);
            Assert.IsFalse(_session.CanLeave);
            Assert.IsFalse(_session.TryLeave());
        }

        [Test]
        public void CanLeaveInSafeZone()
        {
            SpinTo(5);

            Assert.AreSame(_safe, _session.CurrentZoneType);
            Assert.IsTrue(_session.TryLeave());
            Assert.AreEqual(SessionState.Collected, _session.State);
        }

        [Test]
        public void CanLeaveInSuperZone()
        {
            SpinTo(30);

            Assert.AreSame(_super, _session.CurrentZoneType);
            Assert.IsTrue(_session.TryLeave());
        }

        [Test]
        public void CannotLeaveWhileSpinning()
        {
            SpinTo(5);
            _session.TryBeginSpin(out _);

            Assert.IsFalse(_session.TryLeave());
        }

        [Test]
        public void Leave_DepositsCollectedCurrencyIntoWallet()
        {
            RewardsCollectedEvent received = default;
            _events.Subscribe<RewardsCollectedEvent>(e => received = e);
            SpinTo(5);

            _session.TryLeave();

            Assert.AreEqual(StartingBalance + 400, _wallet.Balance);
            Assert.AreEqual(400, received.DepositedCurrency);
            Assert.AreEqual(_wallet.Balance, received.Balance);
        }

        private GameSession CreateSession(int balance)
        {
            _wallet = new FakeWallet(balance);
            var rules = new ZoneRules(new[] { _normal, _safe, _super }, 0f);
            var economy = new EconomyRules(ReviveCost, _gold);
            return new GameSession(rules, economy, _random, _wallet, _events);
        }

        private void SpinTo(int zone)
        {
            while (_session.CurrentZone < zone)
            {
                Spin(RewardSliceIndex);
            }
        }

        private void Spin(int sliceIndex)
        {
            _random.NextValue = sliceIndex;
            _session.TryBeginSpin(out _);
            _session.CompleteSpin();
        }
    }
}
