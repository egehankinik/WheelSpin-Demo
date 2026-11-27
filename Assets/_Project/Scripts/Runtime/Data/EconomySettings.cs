using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "economy_settings", menuName = "WheelOfFortune/Economy Settings")]
    public sealed class EconomySettings : ScriptableObject
    {
        #region Fields

        [SerializeField] private RewardDefinition _reviveCurrency;
        [SerializeField, Min(0)] private int _reviveCost = 25;
        [SerializeField] private string _walletKeyPrefix = "wallet_";

        #endregion

        #region Properties

        public RewardDefinition ReviveCurrency
        {
            get { return _reviveCurrency; }
        }

        public int ReviveCost
        {
            get { return _reviveCost; }
        }

        public string WalletKeyPrefix
        {
            get { return _walletKeyPrefix; }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!_reviveCurrency)
            {
                Debug.LogError($"{name}: revive currency is not assigned.", this);
            }
            else if (!_reviveCurrency.IsCurrency)
            {
                Debug.LogError($"{name}: {_reviveCurrency.name} is not marked as a currency.", this);
            }
        }
#endif

        #endregion
    }
}
