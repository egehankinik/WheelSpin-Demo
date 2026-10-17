using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "economy_settings", menuName = "WheelOfFortune/Economy Settings")]
    public sealed class EconomySettings : ScriptableObject
    {
        [SerializeField] private RewardDefinition _currencyReward;
        [SerializeField, Min(0)] private int _reviveCost = 25;
        [SerializeField, Min(0)] private int _startingBalance = 100;
        [SerializeField] private string _walletKey = "wallet_gold";

        public RewardDefinition CurrencyReward => _currencyReward;
        public int ReviveCost => _reviveCost;
        public int StartingBalance => _startingBalance;
        public string WalletKey => _walletKey;
    }
}
