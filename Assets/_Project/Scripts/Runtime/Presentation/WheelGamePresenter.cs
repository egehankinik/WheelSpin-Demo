using UnityEngine;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.Gameplay.Events;
using WheelOfFortune.UI.Events;
using WheelOfFortune.UI.Popups;
using WheelOfFortune.UI.Rewards;
using WheelOfFortune.UI.Wheel;
using WheelOfFortune.UI.Zone;

namespace WheelOfFortune.Presentation
{
    public sealed class WheelGamePresenter : MonoBehaviour
    {
        [SerializeField] private ZoneSettings _zoneSettings;
        [SerializeField] private EconomySettings _economySettings;
        [SerializeField] private WheelView _wheelView;
        [SerializeField] private ZoneBarView _zoneBarView;
        [SerializeField] private ZoneInfoView _zoneInfoView;
        [SerializeField] private RewardPanelView _rewardPanelView;
        [SerializeField] private RewardFlyAnimator _rewardFlyAnimator;
        [SerializeField] private BombPopupView _bombPopupView;
        [SerializeField] private CollectedPopupView _collectedPopupView;
        [SerializeField] private int _targetFrameRate = 60;

        private IEventBus _events;
        private ZoneRules _rules;
        private GameSession _session;

        private void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;
            _events = new EventBus();
            _rules = _zoneSettings.CreateRules();
            var wallet = new PlayerPrefsWallet(_economySettings.WalletKey, _economySettings.StartingBalance);
            _session = new GameSession(_rules, _economySettings.CreateRules(), new SystemRandomProvider(), wallet, _events);

            _zoneBarView.Initialize(_rules);
            _wheelView.Initialize(_events);
            _rewardPanelView.Initialize(_events);
            _bombPopupView.Initialize(_events);
            _collectedPopupView.Initialize(_events);
        }

        private void OnEnable()
        {
            _events.Subscribe<ZoneChangedEvent>(HandleZoneChanged);
            _events.Subscribe<SessionStateChangedEvent>(HandleStateChanged);
            _events.Subscribe<BombHitEvent>(HandleBombHit);
            _events.Subscribe<RewardsCollectedEvent>(HandleRewardsCollected);
            _events.Subscribe<RewardAmountChangedEvent>(HandleRewardAmountChanged);
            _events.Subscribe<RewardsClearedEvent>(HandleRewardsCleared);
            _events.Subscribe<SpinRequestedEvent>(HandleSpinRequested);
            _events.Subscribe<LeaveRequestedEvent>(HandleLeaveRequested);
            _events.Subscribe<GiveUpRequestedEvent>(HandleGiveUpRequested);
            _events.Subscribe<ReviveRequestedEvent>(HandleReviveRequested);
            _events.Subscribe<RestartRequestedEvent>(HandleRestartRequested);
        }

        private void OnDisable()
        {
            _events.Unsubscribe<ZoneChangedEvent>(HandleZoneChanged);
            _events.Unsubscribe<SessionStateChangedEvent>(HandleStateChanged);
            _events.Unsubscribe<BombHitEvent>(HandleBombHit);
            _events.Unsubscribe<RewardsCollectedEvent>(HandleRewardsCollected);
            _events.Unsubscribe<RewardAmountChangedEvent>(HandleRewardAmountChanged);
            _events.Unsubscribe<RewardsClearedEvent>(HandleRewardsCleared);
            _events.Unsubscribe<SpinRequestedEvent>(HandleSpinRequested);
            _events.Unsubscribe<LeaveRequestedEvent>(HandleLeaveRequested);
            _events.Unsubscribe<GiveUpRequestedEvent>(HandleGiveUpRequested);
            _events.Unsubscribe<ReviveRequestedEvent>(HandleReviveRequested);
            _events.Unsubscribe<RestartRequestedEvent>(HandleRestartRequested);
        }

        private void Start()
        {
            _session.StartRun();
        }

        private void HandleZoneChanged(ZoneChangedEvent zoneChanged)
        {
            var zone = zoneChanged.Zone;
            _wheelView.Bind(_session.CurrentWheel, _session.GetSliceAmount, zone > 1);
            _zoneBarView.ShowZone(zone);
            _zoneInfoView.Show(zone, _rules);
        }

        private void HandleStateChanged(SessionStateChangedEvent stateChanged)
        {
            _wheelView.SetSpinInteractable(_session.CanSpin);
            _rewardPanelView.SetExitInteractable(_session.CanLeave);
        }

        private void HandleRewardAmountChanged(RewardAmountChangedEvent amountChanged)
        {
            _rewardPanelView.SetAmount(amountChanged.Reward, amountChanged.Total);
        }

        private void HandleRewardsCleared(RewardsClearedEvent cleared)
        {
            _rewardPanelView.Clear();
        }

        private void HandleSpinRequested(SpinRequestedEvent request)
        {
            if (_session.TryBeginSpin(out var result))
            {
                _wheelView.Spin(result.SliceIndex, () => HandleWheelStopped(result));
            }
        }

        private void HandleWheelStopped(SpinResult result)
        {
            if (result.IsBomb)
            {
                _wheelView.PlayBombHit(_session.CompleteSpin);
                return;
            }

            var from = _wheelView.GetSliceIconRect(result.SliceIndex);
            var to = _rewardPanelView.GetIconTarget(result.Reward);
            _rewardFlyAnimator.Play(result.Reward.Icon, from, to, _session.CompleteSpin);
        }

        private void HandleLeaveRequested(LeaveRequestedEvent request)
        {
            _session.TryLeave();
        }

        private void HandleBombHit(BombHitEvent bombHit)
        {
            _bombPopupView.Show(bombHit.ReviveCost, bombHit.CanAffordRevive);
        }

        private void HandleGiveUpRequested(GiveUpRequestedEvent request)
        {
            if (_session.TryGiveUp())
            {
                _bombPopupView.Close();
            }
        }

        private void HandleReviveRequested(ReviveRequestedEvent request)
        {
            if (_session.TryRevive())
            {
                _bombPopupView.Close();
            }
        }

        private void HandleRewardsCollected(RewardsCollectedEvent collected)
        {
            _collectedPopupView.Show(collected.Balance);
        }

        private void HandleRestartRequested(RestartRequestedEvent request)
        {
            _collectedPopupView.Close();
            _session.StartRun();
        }
    }
}
