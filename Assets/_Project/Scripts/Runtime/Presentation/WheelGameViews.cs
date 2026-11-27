using System;
using UnityEngine;
using WheelOfFortune.UI.Popups;
using WheelOfFortune.UI.Rewards;
using WheelOfFortune.UI.Wheel;
using WheelOfFortune.UI.Zone;

namespace WheelOfFortune.Presentation
{
    [Serializable]
    public sealed class WheelGameViews
    {
        [SerializeField] private WheelViewMono _wheelView;
        [SerializeField] private ZoneBarViewMono _zoneBarView;
        [SerializeField] private ZoneInfoViewMono _zoneInfoView;
        [SerializeField] private RewardPanelViewMono _rewardPanelView;
        [SerializeField] private RewardFlyAnimatorMono _rewardFlyAnimator;
        [SerializeField] private BombPopupViewMono _bombPopupView;
        [SerializeField] private CollectedPopupViewMono _collectedPopupView;

        public WheelViewMono WheelView
        {
            get { return _wheelView; }
        }

        public ZoneBarViewMono ZoneBarView
        {
            get { return _zoneBarView; }
        }

        public ZoneInfoViewMono ZoneInfoView
        {
            get { return _zoneInfoView; }
        }

        public RewardPanelViewMono RewardPanelView
        {
            get { return _rewardPanelView; }
        }

        public RewardFlyAnimatorMono RewardFlyAnimator
        {
            get { return _rewardFlyAnimator; }
        }

        public BombPopupViewMono BombPopupView
        {
            get { return _bombPopupView; }
        }

        public CollectedPopupViewMono CollectedPopupView
        {
            get { return _collectedPopupView; }
        }
    }
}
