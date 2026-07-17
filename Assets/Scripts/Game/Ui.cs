using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WarmFamilyPuzzle
{
    // Small factory for the code-built uGUI. Rounded corners come from one
    // shared 9-slice sprite; corner radius is set per image through
    // pixelsPerUnitMultiplier.
    public static class Ui
    {
        static Font font;

        public static Font DefaultFont
        {
            get
            {
                if (font == null)
                {
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }

                return font;
            }
        }

        public static GameObject Node(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        public static RectTransform Rect(GameObject go)
        {
            return (RectTransform)go.transform;
        }

        public static GameObject Stretch(Transform parent, string name)
        {
            GameObject go = Node(parent, name);
            var rt = Rect(go);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go;
        }

        public static Image RoundedImage(GameObject go, Color color, float cornerRadius)
        {
            var img = go.AddComponent<Image>();
            img.sprite = UiSprites.Rounded;
            img.type = Image.Type.Sliced;
            img.color = color;
            SetCornerRadius(img, cornerRadius);
            return img;
        }

        public static Image RingImage(GameObject go, Color color, float cornerRadius)
        {
            var img = go.AddComponent<Image>();
            img.sprite = UiSprites.Ring;
            img.type = Image.Type.Sliced;
            img.color = color;
            SetCornerRadius(img, cornerRadius);
            return img;
        }

        public static void SetCornerRadius(Image img, float cornerRadius)
        {
            img.pixelsPerUnitMultiplier = UiSprites.BaseRadius / Mathf.Max(cornerRadius, 1f);
        }

        public static GameObject Panel(Transform parent, string name, Color color, float cornerRadius)
        {
            GameObject go = Node(parent, name);
            RoundedImage(go, color, cornerRadius);
            return go;
        }

        public static Text Label(Transform parent, string text, int size, Color color,
            FontStyle style = FontStyle.Normal, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            GameObject go = Node(parent, "Text");
            var t = go.AddComponent<Text>();
            t.font = DefaultFont;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.raycastTarget = false;
            return t;
        }

        public static Button PillButton(Transform parent, string label, Color bg, Color fg,
            float width, float height, int fontSize, UnityAction onClick)
        {
            GameObject go = Panel(parent, "Button_" + label, bg, height * 0.5f);
            var rt = Rect(go);
            rt.sizeDelta = new Vector2(width, height);

            Text t = Label(go.transform, label, fontSize, fg, FontStyle.Bold);
            StretchInto(t.rectTransform, go.transform);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();
            SetPressTint(btn, bg);
            WireTap(btn);
            if (onClick != null)
            {
                btn.onClick.AddListener(onClick);
            }

            go.AddComponent<PressScale>();
            return btn;
        }

        public static void WireTap(Button btn)
        {
            btn.onClick.AddListener(() => AudioManager.Play("sfx_tap", 0.7f));
        }

        public static Button CircleButton(Transform parent, string label, Color bg, Color fg,
            float diameter, int fontSize, UnityAction onClick)
        {
            GameObject go = Node(parent, "CircleButton_" + label);
            var rt = Rect(go);
            rt.sizeDelta = new Vector2(diameter, diameter);
            var img = go.AddComponent<Image>();
            img.sprite = UiSprites.Circle;
            img.color = bg;

            Text t = Label(go.transform, label, fontSize, fg, FontStyle.Bold);
            StretchInto(t.rectTransform, go.transform);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            SetPressTint(btn, bg);
            WireTap(btn);
            if (onClick != null)
            {
                btn.onClick.AddListener(onClick);
            }

            go.AddComponent<PressScale>();
            return btn;
        }

        public static void SetPressTint(Button btn, Color baseColor)
        {
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.97f, 0.97f, 0.97f, 1f);
            cb.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            cb.selectedColor = Color.white;
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
        }

        public static void StretchInto(RectTransform rt, Transform parent)
        {
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        public static void PlaceTopStretch(RectTransform rt, float top, float height, float leftPad = 0f, float rightPad = 0f)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2((leftPad - rightPad) * 0.5f, -top);
            rt.sizeDelta = new Vector2(-(leftPad + rightPad), height);
        }

        // Cover-fit UV rect so any photo aspect fills the target rect without distortion.
        public static void SetCoverUV(RawImage rawImage, Texture tex, float displayAspect)
        {
            if (tex == null)
            {
                return;
            }

            float texAspect = (float)tex.width / tex.height;
            float u = 1f;
            float v = 1f;
            if (texAspect > displayAspect)
            {
                u = displayAspect / texAspect;
            }
            else
            {
                v = texAspect / displayAspect;
            }

            rawImage.texture = tex;
            rawImage.uvRect = new UnityEngine.Rect((1f - u) * 0.5f, (1f - v) * 0.5f, u, v);
        }

        // Sub-tile UV inside the centered square crop of a photo.
        // row/col follow PuzzleCore indexing: x = row (top to bottom), y = col.
        public static UnityEngine.Rect TileUV(Texture tex, int row, int col, int gridSize)
        {
            float texAspect = (float)tex.width / tex.height;
            float uSpan = 1f;
            float vSpan = 1f;
            if (texAspect > 1f)
            {
                uSpan = 1f / texAspect;
            }
            else
            {
                vSpan = texAspect;
            }

            float uMin = (1f - uSpan) * 0.5f;
            float vMin = (1f - vSpan) * 0.5f;

            float u = uMin + col / (float)gridSize * uSpan;
            float v = vMin + (1f - (row + 1) / (float)gridSize) * vSpan;
            return new UnityEngine.Rect(u, v, uSpan / gridSize, vSpan / gridSize);
        }

        public static RawImage MaskedPhoto(Transform parent, string name, float cornerRadius)
        {
            GameObject holder = Node(parent, name);
            var maskImage = RoundedImage(holder, Color.white, cornerRadius);
            var mask = holder.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject photoGo = Stretch(holder.transform, "Photo");
            var raw = photoGo.AddComponent<RawImage>();
            raw.raycastTarget = false;
            return raw;
        }
    }

    public class PressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public float pressedScale = 0.96f;

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.localScale = Vector3.one * pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
        }
    }
}
