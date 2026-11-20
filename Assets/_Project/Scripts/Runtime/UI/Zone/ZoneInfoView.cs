using UnityEngine;
using WheelOfFortune.Gameplay;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneInfoView : MonoBehaviour
    {
        [SerializeField] private ZoneInfoEntryView[] _entries;

        public void Show(int currentZone, ZoneRules rules)
        {
            foreach (var entry in _entries)
            {
                entry.Show(rules.GetNextZoneOfType(currentZone, entry.ZoneType));
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _entries = GetComponentsInChildren<ZoneInfoEntryView>(true);
        }
#endif
    }
}
