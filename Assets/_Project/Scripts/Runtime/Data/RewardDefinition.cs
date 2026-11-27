using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "reward_", menuName = "WheelOfFortune/Reward Definition")]
    public sealed class RewardDefinition : ScriptableObject
    {
        #region Fields

        [SerializeField, Tooltip("Unique id. Currencies use it as their wallet key.")]
        private string _id;
        [SerializeField] private Sprite _icon;
        [SerializeField, Tooltip("Currencies are added to the wallet when the player leaves with the rewards.")]
        private bool _isCurrency;
        [SerializeField, Min(0)] private int _startingBalance;

        #endregion

        #region Properties

        public string Id
        {
            get { return _id; }
        }

        public Sprite Icon
        {
            get { return _icon; }
        }

        public bool IsCurrency
        {
            get { return _isCurrency; }
        }

        public int StartingBalance
        {
            get { return _startingBalance; }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_id))
            {
                Debug.LogError($"{name}: reward id is empty.", this);
            }

            if (!_icon)
            {
                Debug.LogError($"{name}: reward has no icon.", this);
            }
        }
#endif

        #endregion
    }
}
