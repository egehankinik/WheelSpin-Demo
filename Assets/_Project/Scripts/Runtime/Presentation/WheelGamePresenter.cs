using System;
using WheelOfFortune.Configuration;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
using WheelOfFortune.Gameplay.Events;
using WheelOfFortune.UI.Events;

namespace WheelOfFortune.Presentation
{
    public sealed class WheelGamePresenter
    {
        #region Fields

        private readonly IEventBus _events;
        private readonly GameSession _session;
        private readonly GameConfiguration _configuration;
        private readonly WheelGameViews _views;
        private SpinResult _currentSpin;

        #endregion

        public WheelGamePresenter(IEventBus events, GameSession session, GameConfiguration configuration, WheelGameViews views)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _views = views ?? throw new ArgumentNullException(nameof(views));

            _views.ZoneBarView.Initialize(GetZoneTypeDefinition);
            _views.WheelView.Initialize(_events);
            _views.RewardPanelView.Initialize(_events);
            _views.RewardFlyAnimator.Initialize(_events);
            _views.BombPopupView.Initialize(_events);
            _views.CollectedPopupView.Initialize(_events);
        }

        #region Public Methods

        public void Enable()
        {
            _events.Subscribe<ZoneChangedEvent>(HandleZoneChanged);
            _events.Subscribe<SessionStateChangedEvent>(HandleStateChanged);
            _events.Subscribe<BombHitEvent>(HandleBombHit);
            _events.Subscribe<RewardsCollectedEvent>(HandleRewardsCollected);
            _events.Subscribe<RewardAmountChangedEvent>(HandleRewardAmountChanged);
            _events.Subscribe<RewardsClearedEvent>(HandleRewardsCleared);
            _events.Subscribe<SpinRequestedEvent>(HandleSpinRequested);
            _events.Subscribe<SpinAnimationInterruptedEvent>(HandleSpinAnimationInterrupted);
            _events.Subscribe<LeaveRequestedEvent>(HandleLeaveRequested);
            _events.Subscribe<GiveUpRequestedEvent>(HandleGiveUpRequested);
            _events.Subscribe<ReviveRequestedEvent>(HandleReviveRequested);
            _events.Subscribe<RestartRequestedEvent>(HandleRestartRequested);
        }

        public void Disable()
        {
            _events.Unsubscribe<ZoneChangedEvent>(HandleZoneChanged);
            _events.Unsubscribe<SessionStateChangedEvent>(HandleStateChanged);
            _events.Unsubscribe<BombHitEvent>(HandleBombHit);
            _events.Unsubscribe<RewardsCollectedEvent>(HandleRewardsCollected);
            _events.Unsubscribe<RewardAmountChangedEvent>(HandleRewardAmountChanged);
            _events.Unsubscribe<RewardsClearedEvent>(HandleRewardsCleared);
            _events.Unsubscribe<SpinRequestedEvent>(HandleSpinRequested);
            _events.Unsubscribe<SpinAnimationInterruptedEvent>(HandleSpinAnimationInterrupted);
            _events.Unsubscribe<LeaveRequestedEvent>(HandleLeaveRequested);
            _events.Unsubscribe<GiveUpRequestedEvent>(HandleGiveUpRequested);
            _events.Unsubscribe<ReviveRequestedEvent>(HandleReviveRequested);
            _events.Unsubscribe<RestartRequestedEvent>(HandleRestartRequested);
        }

        public void StartGame()
        {
            _session.TryStartRun();
        }

        #endregion

        #region Session Event Handlers

        private void HandleZoneChanged(ZoneChangedEvent zoneChanged)
        {
            var zone = zoneChanged.Zone;
            var wheel = _configuration.GetDefinition(_session.CurrentWheel);
            _views.WheelView.Bind(wheel, _session.GetSliceAmount, zone > 1);
            _views.ZoneBarView.ShowZone(zone);
            _views.ZoneInfoView.Show(GetNextZoneOfType);
        }

        private void HandleStateChanged(SessionStateChangedEvent stateChanged)
        {
            _views.WheelView.SetSpinInteractable(_session.IsSpinAllowed);
            _views.RewardPanelView.SetExitInteractable(_session.IsLeaveAllowed);
        }

        private void HandleRewardAmountChanged(RewardAmountChangedEvent amountChanged)
        {
            _views.RewardPanelView.SetAmount(_configuration.GetDefinition(amountChanged.Reward), amountChanged.Total);
        }

        private void HandleRewardsCleared(RewardsClearedEvent cleared)
        {
            _views.RewardPanelView.Clear();
        }

        private void HandleBombHit(BombHitEvent bombHit)
        {
            _views.BombPopupView.Show(bombHit.ReviveCost, bombHit.IsReviveAffordable);
        }

        private void HandleRewardsCollected(RewardsCollectedEvent collected)
        {
            _views.CollectedPopupView.Show(collected.ReviveCurrencyBalance);
        }

        #endregion

        #region Request Event Handlers

        private void HandleSpinRequested(SpinRequestedEvent request)
        {
            if (_session.TryBeginSpin(out _currentSpin))
            {
                _views.WheelView.Spin(_currentSpin.SliceIndex, HandleWheelStopped);
            }
        }

        private void HandleSpinAnimationInterrupted(SpinAnimationInterruptedEvent interrupted)
        {
            _session.CompleteSpin();
        }

        private void HandleLeaveRequested(LeaveRequestedEvent request)
        {
            _session.TryLeave();
        }

        private void HandleGiveUpRequested(GiveUpRequestedEvent request)
        {
            if (_session.TryGiveUp())
            {
                _views.BombPopupView.Close();
            }
        }

        private void HandleReviveRequested(ReviveRequestedEvent request)
        {
            if (_session.TryRevive())
            {
                _views.BombPopupView.Close();
            }
        }

        private void HandleRestartRequested(RestartRequestedEvent request)
        {
            if (_session.TryStartRun())
            {
                _views.CollectedPopupView.Close();
            }
        }

        #endregion

        #region Private Methods

        private void HandleWheelStopped()
        {
            if (_currentSpin.IsBomb)
            {
                _views.WheelView.PlayBombHit(_session.CompleteSpin);
                return;
            }

            var reward = _configuration.GetDefinition(_currentSpin.Reward);
            var from = _views.WheelView.GetSliceIconRect(_currentSpin.SliceIndex);
            var to = _views.RewardPanelView.GetIconTarget(reward);
            _views.RewardFlyAnimator.Play(reward.Icon, from, to, _session.CompleteSpin);
        }

        private ZoneTypeDefinition GetZoneTypeDefinition(int zone)
        {
            return _configuration.GetDefinition(_configuration.ZoneRules.GetZoneType(zone));
        }

        private int GetNextZoneOfType(ZoneTypeDefinition zoneType)
        {
            return _configuration.ZoneRules.GetNextZoneOfType(_session.CurrentZone, _configuration.GetZoneType(zoneType));
        }

        #endregion
    }
}
