using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "wheel_", menuName = "WheelOfFortune/Wheel Definition")]
    public sealed class WheelDefinition : ScriptableObject
    {
        [SerializeField] private string _title;
        [SerializeField] private string _subtitle;
        [SerializeField] private Sprite _wheelSprite;
        [SerializeField] private Sprite _indicatorSprite;
        [SerializeField] private Color _glowColor = new Color(1f, 1f, 1f, 0.25f);
        [SerializeField] private List<WheelSlice> _slices = new List<WheelSlice>();

        public string Title => _title;
        public string Subtitle => _subtitle;
        public Sprite WheelSprite => _wheelSprite;
        public Sprite IndicatorSprite => _indicatorSprite;
        public Color GlowColor => _glowColor;
        public IReadOnlyList<WheelSlice> Slices => _slices;
        public int SliceCount => _slices.Count;
        public float SliceAngle => SliceCount > 0 ? 360f / SliceCount : 0f;

        public bool HasBomb
        {
            get
            {
                foreach (var slice in _slices)
                {
                    if (slice.IsBomb)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        internal void Initialize(IEnumerable<WheelSlice> slices)
        {
            _slices = new List<WheelSlice>(slices);
        }

        private void OnValidate()
        {
            for (var i = 0; i < _slices.Count; i++)
            {
                var slice = _slices[i];
                if (!slice.IsBomb && slice.Reward == null)
                {
                    Debug.LogWarning($"{name}: slice {i} has no reward assigned.", this);
                }
            }
        }
    }
}
