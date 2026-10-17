using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;
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

        private ZoneRules _rules;
        private GameSession _session;
        private IWallet _wallet;

        private void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;
            _rules = _zoneSettings.CreateRules();
            _session = new GameSession(_rules, _zoneSettings, new SystemRandomProvider());
            _wallet = new PlayerPrefsWallet(_economySettings.WalletKey, _economySettings.StartingBalance);
            _zoneBarView.Initialize(_rules);
        }

        private void OnEnable()
        {
            _session.ZoneChanged += HandleZoneChanged;
            _session.StateChanged += RefreshButtons;
            _session.BombHit += HandleBombHit;
            _session.RewardsCollected += HandleRewardsCollected;
            _session.Inventory.RewardChanged += _rewardPanelView.SetAmount;
            _session.Inventory.Cleared += _rewardPanelView.Clear;
            _wheelView.SpinClicked += HandleSpinClicked;
            _rewardPanelView.ExitClicked += HandleExitClicked;
            _bombPopupView.GiveUpClicked += HandleGiveUpClicked;
            _bombPopupView.ReviveClicked += HandleReviveClicked;
            _collectedPopupView.RestartClicked += HandleRestartClicked;
        }

        private void OnDisable()
        {
            _session.ZoneChanged -= HandleZoneChanged;
            _session.StateChanged -= RefreshButtons;
            _session.BombHit -= HandleBombHit;
            _session.RewardsCollected -= HandleRewardsCollected;
            _session.Inventory.RewardChanged -= _rewardPanelView.SetAmount;
            _session.Inventory.Cleared -= _rewardPanelView.Clear;
            _wheelView.SpinClicked -= HandleSpinClicked;
            _rewardPanelView.ExitClicked -= HandleExitClicked;
            _bombPopupView.GiveUpClicked -= HandleGiveUpClicked;
            _bombPopupView.ReviveClicked -= HandleReviveClicked;
            _collectedPopupView.RestartClicked -= HandleRestartClicked;
        }

        private void Start()
        {
            _session.StartRun();
        }

        private void HandleZoneChanged(int zone)
        {
            _wheelView.Bind(_session.CurrentWheel, _session.GetSliceAmount, zone > 1);
            _zoneBarView.ShowZone(zone);
            _zoneInfoView.Show(_rules.GetNextZoneOfType(zone, ZoneType.Super), _rules.GetNextZoneOfType(zone, ZoneType.Safe));
        }

        private void RefreshButtons()
        {
            _wheelView.SetSpinInteractable(_session.CanSpin);
            _rewardPanelView.SetExitInteractable(_session.CanLeave);
        }

        private void HandleSpinClicked()
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

        private void HandleExitClicked()
        {
            _session.TryLeave();
        }

        private void HandleBombHit()
        {
            var cost = _economySettings.ReviveCost;
            _bombPopupView.Show(cost, _wallet.Balance >= cost);
        }

        private void HandleGiveUpClicked()
        {
            if (_session.TryGiveUp())
            {
                _bombPopupView.Close();
            }
        }

        private void HandleReviveClicked()
        {
            if (!_session.CanRevive || !_wallet.TrySpend(_economySettings.ReviveCost))
            {
                return;
            }

            _bombPopupView.Close();
            _session.TryRevive();
        }

        private void HandleRewardsCollected()
        {
            var earnedCurrency = _session.Inventory.GetAmount(_economySettings.CurrencyReward);
            if (earnedCurrency > 0)
            {
                _wallet.Add(earnedCurrency);
            }

            _collectedPopupView.Show(_wallet.Balance);
        }

        private void HandleRestartClicked()
        {
            _collectedPopupView.Close();
            _session.StartRun();
        }
    }
}
