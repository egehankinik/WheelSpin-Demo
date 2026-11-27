using NUnit.Framework;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Gameplay;
using WheelOfFortune.Gameplay.Events;

namespace WheelOfFortune.Tests
{
    public class GameSessionTests
    {
        #region Setup

        private const int REWARD_SLICE_INDEX = 0;
        private const int BOMB_SLICE_INDEX = 1;
        private const int CASH_SLICE_INDEX = 2;
        private const int ITEM_SLICE_INDEX = 3;
        private const int REVIVE_COST = 25;
        private const int STARTING_BALANCE = 100;

        private RewardModel _gold;
        private RewardModel _cash;
        private RewardModel _item;
        private ZoneTypeModel _normal;
        private ZoneTypeModel _safe;
        private ZoneTypeModel _super;
        private FixedRandomProvider _random;
        private FakeWallet _wallet;
        private EventBus _events;
        private GameSession _session;

        [SetUp]
        public void SetUp()
        {
            _gold = TestModels.Currency("gold", STARTING_BALANCE);
            _cash = TestModels.Currency("cash");
            _item = TestModels.Item("item");
            var riskyWheel = TestModels.Wheel(new SliceModel(_gold, 100), SliceModel.Bomb(), new SliceModel(_cash, 10), new SliceModel(_item, 1));
            var safeWheel = TestModels.Wheel(new SliceModel(_gold, 100), new SliceModel(_gold, 200));
            _normal = TestModels.ZoneType(0, false, new WheelTierModel(1, riskyWheel));
            _safe = TestModels.ZoneType(5, true, new WheelTierModel(1, safeWheel));
            _super = TestModels.ZoneType(30, true, new WheelTierModel(1, safeWheel));

            _random = new FixedRandomProvider();
            _events = new EventBus();
            _wallet = new FakeWallet();
            _session = CreateSession();
            _session.TryStartRun();
        }

        #endregion

        #region Run Start

        [Test]
        public void StartRun_BeginsAtZoneOneAndIdle()
        {
            Assert.AreEqual(1, _session.CurrentZone);
            Assert.AreEqual(SessionState.Idle, _session.State);
            Assert.IsTrue(_session.Inventory.IsEmpty);
        }

        [Test]
        public void StartRun_DuringARun_IsRejected()
        {
            Spin(REWARD_SLICE_INDEX);

            Assert.IsFalse(_session.TryStartRun());
            Assert.AreEqual(2, _session.CurrentZone);
            Assert.IsFalse(_session.Inventory.IsEmpty);
        }

        [Test]
        public void StartRun_WhileSpinning_IsRejected()
        {
            _session.TryBeginSpin(out _);

            Assert.IsFalse(_session.TryStartRun());
            Assert.AreEqual(SessionState.Spinning, _session.State);
        }

        [Test]
        public void StartRun_AfterCollecting_RestartsTheRun()
        {
            SpinTo(5);
            _session.TryLeave();

            Assert.IsTrue(_session.TryStartRun());
            Assert.AreEqual(1, _session.CurrentZone);
            Assert.IsTrue(_session.Inventory.IsEmpty);
        }

        #endregion

        #region Spinning

        [Test]
        public void RewardSpin_AddsRewardAndAdvancesZone()
        {
            Spin(REWARD_SLICE_INDEX);

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
        public void CannotSpinWhileSpinning()
        {
            _session.TryBeginSpin(out _);

            Assert.IsFalse(_session.TryBeginSpin(out _));
        }

        [Test]
        public void CompleteSpin_CalledTwice_AppliesTheResultOnce()
        {
            _session.TryBeginSpin(out _);

            _session.CompleteSpin();
            _session.CompleteSpin();

            Assert.AreEqual(100, _session.Inventory.GetAmount(_gold));
            Assert.AreEqual(2, _session.CurrentZone);
        }

        [Test]
        public void CompleteSpin_WithoutASpin_DoesNothing()
        {
            _session.CompleteSpin();

            Assert.AreEqual(1, _session.CurrentZone);
            Assert.AreEqual(SessionState.Idle, _session.State);
        }

        #endregion

        #region Bomb And Revive

        [Test]
        public void BombSpin_SetsLostStateAndPublishesReviveCost()
        {
            BombHitEvent received = default;
            _events.Subscribe<BombHitEvent>(e => received = e);

            Spin(BOMB_SLICE_INDEX);

            Assert.AreEqual(SessionState.Lost, _session.State);
            Assert.IsFalse(_session.IsSpinAllowed);
            Assert.AreEqual(REVIVE_COST, received.ReviveCost);
            Assert.IsTrue(received.IsReviveAffordable);
        }

        [Test]
        public void GiveUp_AfterBomb_ClearsRewardsAndRestarts()
        {
            Spin(REWARD_SLICE_INDEX);
            Spin(BOMB_SLICE_INDEX);

            Assert.IsTrue(_session.TryGiveUp());

            Assert.IsTrue(_session.Inventory.IsEmpty);
            Assert.AreEqual(1, _session.CurrentZone);
        }

        [Test]
        public void Revive_AfterBomb_SpendsCostAndKeepsRewards()
        {
            Spin(REWARD_SLICE_INDEX);
            Spin(BOMB_SLICE_INDEX);

            Assert.IsTrue(_session.TryRevive());

            Assert.AreEqual(STARTING_BALANCE - REVIVE_COST, _wallet.GetBalance(_gold));
            Assert.AreEqual(100, _session.Inventory.GetAmount(_gold));
            Assert.AreEqual(SessionState.Idle, _session.State);
        }

        [Test]
        public void Revive_WithoutEnoughCurrency_FailsAndKeepsBalance()
        {
            _wallet.SetBalance(_gold, REVIVE_COST - 1);
            Spin(BOMB_SLICE_INDEX);

            Assert.IsFalse(_session.IsReviveAllowed);
            Assert.IsFalse(_session.TryRevive());
            Assert.AreEqual(REVIVE_COST - 1, _wallet.GetBalance(_gold));
            Assert.AreEqual(SessionState.Lost, _session.State);
        }

        [Test]
        public void Revive_WhenNotLost_Fails()
        {
            Assert.IsFalse(_session.TryRevive());
            Assert.AreEqual(STARTING_BALANCE, _wallet.GetBalance(_gold));
        }

        #endregion

        #region Leaving

        [Test]
        public void CannotLeaveInNormalZone()
        {
            Assert.AreSame(_normal, _session.CurrentZoneType);
            Assert.IsFalse(_session.IsLeaveAllowed);
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
        public void Leave_DepositsEveryCollectedCurrencyIntoTheWallet()
        {
            RewardsCollectedEvent received = default;
            _events.Subscribe<RewardsCollectedEvent>(e => received = e);
            Spin(CASH_SLICE_INDEX);
            Spin(ITEM_SLICE_INDEX);
            SpinTo(5);

            _session.TryLeave();

            Assert.AreEqual(STARTING_BALANCE + 200, _wallet.GetBalance(_gold));
            Assert.AreEqual(10, _wallet.GetBalance(_cash));
            Assert.AreEqual(_wallet.GetBalance(_gold), received.ReviveCurrencyBalance);
        }

        [Test]
        public void Leave_WithCollectedItems_OnlyDepositsCurrencies()
        {
            Spin(ITEM_SLICE_INDEX);
            SpinTo(5);

            Assert.IsTrue(_session.TryLeave());
            Assert.AreEqual(1, _session.Inventory.GetAmount(_item));
        }

        #endregion

        #region Helpers

        private GameSession CreateSession()
        {
            var rules = new ZoneRules(new[] { _normal, _safe, _super }, 0f);
            var economy = new EconomyRules(_gold, REVIVE_COST);
            return new GameSession(rules, economy, _random, _wallet, _events);
        }

        private void SpinTo(int zone)
        {
            while (_session.CurrentZone < zone)
            {
                Spin(REWARD_SLICE_INDEX);
            }
        }

        private void Spin(int sliceIndex)
        {
            _random.NextValue = sliceIndex;
            _session.TryBeginSpin(out _);
            _session.CompleteSpin();
        }

        #endregion
    }
}
