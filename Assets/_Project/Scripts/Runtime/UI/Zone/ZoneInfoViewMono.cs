using System;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneInfoViewMono : MonoBehaviour
    {
        #region Fields

        [SerializeField] private ZoneInfoEntryViewMono[] _entries;

        #endregion

        #region Public Methods

        public void Show(Func<ZoneTypeDefinition, int> nextZoneOfType)
        {
            foreach (var entry in _entries)
            {
                entry.Show(nextZoneOfType(entry.ZoneType));
            }
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            _entries = GetComponentsInChildren<ZoneInfoEntryViewMono>(true);
        }
#endif

        #endregion
    }
}
