using System;
using NUnit.Framework;
using UnityEngine;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.Tests
{
    public class PlayerPrefsWalletTests
    {
        #region Setup

        private const string KEY_PREFIX = "wheel_of_fortune_tests_";

        private RewardModel _gold;
        private RewardModel _cash;
        private PlayerPrefsWallet _wallet;

        [SetUp]
        public void SetUp()
        {
            _gold = TestModels.Currency("gold", 100);
            _cash = TestModels.Currency("cash");
            DeleteKeys();
            _wallet = new PlayerPrefsWallet(KEY_PREFIX);
        }

        [TearDown]
        public void TearDown()
        {
            DeleteKeys();
        }

        #endregion

        #region Tests

        [Test]
        public void NewCurrency_StartsAtItsStartingBalance()
        {
            Assert.AreEqual(100, _wallet.GetBalance(_gold));
            Assert.AreEqual(0, _wallet.GetBalance(_cash));
        }

        [Test]
        public void Currencies_AreStoredSeparately()
        {
            _wallet.Add(_cash, 40);

            Assert.AreEqual(40, _wallet.GetBalance(_cash));
            Assert.AreEqual(100, _wallet.GetBalance(_gold));
        }

        [Test]
        public void TrySpend_WithEnoughBalance_Subtracts()
        {
            Assert.IsTrue(_wallet.TrySpend(_gold, 25));
            Assert.AreEqual(75, _wallet.GetBalance(_gold));
        }

        [Test]
        public void TrySpend_WithoutEnoughBalance_FailsAndKeepsBalance()
        {
            Assert.IsFalse(_wallet.TrySpend(_gold, 101));
            Assert.AreEqual(100, _wallet.GetBalance(_gold));
        }

        [Test]
        public void Balance_PersistsAcrossWalletInstances()
        {
            _wallet.Add(_gold, 50);

            Assert.AreEqual(150, new PlayerPrefsWallet(KEY_PREFIX).GetBalance(_gold));
        }

        [Test]
        public void Add_ForANonCurrencyReward_Throws()
        {
            Assert.Throws<ArgumentException>(AddNonCurrency);
        }

        #endregion

        #region Helpers

        private void AddNonCurrency()
        {
            _wallet.Add(TestModels.Item("item"), 1);
        }

        private void DeleteKeys()
        {
            PlayerPrefs.DeleteKey(KEY_PREFIX + _gold.Id);
            PlayerPrefs.DeleteKey(KEY_PREFIX + _cash.Id);
        }

        #endregion
    }
}
