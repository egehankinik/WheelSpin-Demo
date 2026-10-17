using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "reward_", menuName = "WheelOfFortune/Reward Definition")]
    public sealed class RewardDefinition : ScriptableObject
    {
        [SerializeField] private Sprite _icon;

        public Sprite Icon => _icon;

        internal void Initialize(Sprite icon)
        {
            _icon = icon;
        }
    }
}
