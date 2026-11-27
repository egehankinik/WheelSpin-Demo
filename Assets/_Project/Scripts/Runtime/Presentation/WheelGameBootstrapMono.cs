using System;
using UnityEngine;
using WheelOfFortune.Configuration;
using WheelOfFortune.Core.Events;
using WheelOfFortune.Data;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Presentation
{
    public sealed class WheelGameBootstrapMono : MonoBehaviour
    {
        #region Fields

        [SerializeField] private ZoneSettings _zoneSettings;
        [SerializeField] private EconomySettings _economySettings;
        [SerializeField] private WheelGameViews _views;
        [SerializeField] private int _targetFrameRate = 60;

        private WheelGamePresenter _presenter;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;

            var configuration = new GameConfiguration(_zoneSettings, _economySettings);
            EnsureWheelsFitView(configuration);

            var events = new EventBus();
            var wallet = new PlayerPrefsWallet(configuration.WalletKeyPrefix);
            var session = new GameSession(configuration.ZoneRules, configuration.EconomyRules, new SystemRandomProvider(), wallet, events);
            _presenter = new WheelGamePresenter(events, session, configuration, _views);
        }

        private void OnEnable()
        {
            _presenter?.Enable();
        }

        private void Start()
        {
            _presenter?.StartGame();
        }

        private void OnDisable()
        {
            _presenter?.Disable();
        }

        #endregion

        #region Private Methods

        private void EnsureWheelsFitView(GameConfiguration configuration)
        {
            foreach (var wheel in configuration.WheelDefinitions)
            {
                if (!_views.WheelView.CanDisplay(wheel))
                {
                    throw new InvalidOperationException($"{wheel.name} has {wheel.SliceCount} slices but the wheel view can show 1 to {_views.WheelView.SliceCapacity}.");
                }
            }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_zoneSettings || _views == null || !_views.WheelView)
            {
                return;
            }

            foreach (var zoneType in _zoneSettings.ZoneTypes)
            {
                if (!zoneType)
                {
                    continue;
                }

                foreach (var tier in zoneType.WheelTiers)
                {
                    if (tier.Wheel && !_views.WheelView.CanDisplay(tier.Wheel))
                    {
                        Debug.LogError($"{tier.Wheel.name} has {tier.Wheel.SliceCount} slices but the wheel view can show 1 to {_views.WheelView.SliceCapacity}.", this);
                    }
                }
            }
        }
#endif

        #endregion
    }
}
