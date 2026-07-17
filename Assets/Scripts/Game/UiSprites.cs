using UnityEngine;

namespace WarmFamilyPuzzle
{
    // Procedural 9-slice sprites so the whole warm rounded look needs no art assets.
    public static class UiSprites
    {
        public const float BaseRadius = 48f;
        const int TexSize = 128;

        static Sprite rounded;
        static Sprite circle;
        static Sprite ring;

        public static Sprite Rounded
        {
            get
            {
                if (rounded == null)
                {
                    rounded = BuildRoundedRect(borderOnly: false);
                }

                return rounded;
            }
        }

        public static Sprite Ring
        {
            get
            {
                if (ring == null)
                {
                    ring = BuildRoundedRect(borderOnly: true);
                }

                return ring;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle == null)
                {
                    circle = BuildCircle();
                }

                return circle;
            }
        }

        static Sprite BuildRoundedRect(bool borderOnly)
        {
            int size = TexSize;
            float radius = BaseRadius;
            float half = size * 0.5f;
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                name = borderOnly ? "WFP_Ring" : "WFP_Rounded",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            const float borderThickness = 6f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f - half;
                    float py = y + 0.5f - half;
                    float d = RoundedRectDistance(px, py, half - 1.5f, half - 1.5f, radius);
                    float alpha = Mathf.Clamp01(0.5f - d);
                    if (borderOnly)
                    {
                        float inner = Mathf.Clamp01(0.5f - (d + borderThickness));
                        alpha -= inner;
                    }

                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);

            float border = radius + 4f;
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        static Sprite BuildCircle()
        {
            int size = TexSize;
            float half = size * 0.5f;
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, false)
            {
                name = "WFP_Circle",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = x + 0.5f - half;
                    float py = y + 0.5f - half;
                    float d = Mathf.Sqrt(px * px + py * py) - (half - 1.5f);
                    float alpha = Mathf.Clamp01(0.5f - d);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static float RoundedRectDistance(float px, float py, float halfW, float halfH, float radius)
        {
            float qx = Mathf.Abs(px) - (halfW - radius);
            float qy = Mathf.Abs(py) - (halfH - radius);
            float ax = Mathf.Max(qx, 0f);
            float ay = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }
    }
}
