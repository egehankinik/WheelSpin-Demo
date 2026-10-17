using UnityEngine;

namespace WheelOfFortune.UI.Common
{
    public static class UiReferenceFinder
    {
        public static T Find<T>(Component root, string childName) where T : Component
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName && child.TryGetComponent<T>(out var component))
                {
                    return component;
                }
            }

            Debug.LogWarning($"{root.name}: child '{childName}' with {typeof(T).Name} was not found.", root);
            return null;
        }
    }
}
