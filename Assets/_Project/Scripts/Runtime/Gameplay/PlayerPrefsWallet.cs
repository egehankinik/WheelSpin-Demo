using System;
using UnityEngine;

namespace WheelOfFortune.Gameplay
{
    public sealed class PlayerPrefsWallet : IWallet
    {
        #region Fields

        private readonly string _keyPrefix;

        #endregion

        public PlayerPrefsWallet(string keyPrefix)
        {
            if (string.IsNullOrEmpty(keyPrefix))
            {
                throw new ArgumentException("Wallet key prefix is required.", nameof(keyPrefix));
            }

            _keyPrefix = keyPrefix;
        }

        #region Public Methods

        public int GetBalance(RewardModel currency)
        {
            ValidateCurrency(currency);
            return PlayerPrefs.GetInt(GetKey(currency), currency.StartingBalance);
        }

        public void Add(RewardModel currency, int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            SetBalance(currency, GetBalance(currency) + amount);
        }

        public bool TrySpend(RewardModel currency, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            var balance = GetBalance(currency);
            if (balance < amount)
            {
                return false;
            }

            SetBalance(currency, balance - amount);
            return true;
        }

        #endregion

        #region Private Methods

        private void SetBalance(RewardModel currency, int balance)
        {
            PlayerPrefs.SetInt(GetKey(currency), balance);
            PlayerPrefs.Save();
        }

        private string GetKey(RewardModel currency)
        {
            return _keyPrefix + currency.Id;
        }

        private static void ValidateCurrency(RewardModel currency)
        {
            if (currency == null)
            {
                throw new ArgumentNullException(nameof(currency));
            }

            if (!currency.IsCurrency)
            {
                throw new ArgumentException($"{currency.Id} is not a currency.", nameof(currency));
            }
        }

        #endregion
    }
}
