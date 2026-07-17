using UnityEngine;

namespace WarmFamilyPuzzle
{
    // Keeps UI inside notch/home-indicator safe area on iPhone/Android.
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rectTransform;
        Rect applied = Rect.zero;

        void Awake()
        {
            rectTransform = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != applied)
            {
                Apply();
            }
        }

        void Apply()
        {
            applied = Screen.safeArea;
            Vector2 min = applied.position;
            Vector2 max = applied.position + applied.size;
            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
