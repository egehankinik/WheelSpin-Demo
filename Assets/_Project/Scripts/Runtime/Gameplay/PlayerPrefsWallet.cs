using System;
using UnityEngine;

namespace WheelOfFortune.Gameplay
{
    public sealed class PlayerPrefsWallet : IWallet
    {
        private readonly string _key;

        public PlayerPrefsWallet(string key, int startingBalance)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Wallet key is required.", nameof(key));
            }

            _key = key;
            Balance = PlayerPrefs.GetInt(_key, startingBalance);
        }

        public int Balance { get; private set; }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Balance += amount;
            Save();
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (Balance < amount)
            {
                return false;
            }

            Balance -= amount;
            Save();
            return true;
        }

        private void Save()
        {
            PlayerPrefs.SetInt(_key, Balance);
            PlayerPrefs.Save();
        }
    }
}
