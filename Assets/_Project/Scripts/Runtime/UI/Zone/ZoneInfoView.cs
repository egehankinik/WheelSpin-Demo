using TMPro;
using UnityEngine;
using WheelOfFortune.UI.Common;

namespace WheelOfFortune.UI.Zone
{
    public sealed class ZoneInfoView : MonoBehaviour
    {
        private const string SuperZoneTextName = "ui_text_zone_info_super_value";
        private const string SafeZoneTextName = "ui_text_zone_info_safe_value";

        [SerializeField] private TMP_Text _superZoneText;
        [SerializeField] private TMP_Text _safeZoneText;

        public void Show(int nextSuperZone, int nextSafeZone)
        {
            _superZoneText.text = nextSuperZone.ToString();
            _safeZoneText.text = nextSafeZone.ToString();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _superZoneText = UiReferenceFinder.Find<TMP_Text>(this, SuperZoneTextName);
            _safeZoneText = UiReferenceFinder.Find<TMP_Text>(this, SafeZoneTextName);
        }
#endif
    }
}
