using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarmFamilyPuzzle
{
    // Lightweight UI confetti burst — no particle system needed.
    public class Confetti : MonoBehaviour
    {
        class Piece
        {
            public RectTransform rect;
            public float fallSpeed;
            public float swayAmplitude;
            public float swayPhase;
            public float spinSpeed;
            public float baseX;
        }

        readonly List<Piece> pieces = new List<Piece>();

        void OnEnable()
        {
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            var area = (RectTransform)transform;
            float width = Mathf.Max(area.rect.width, 430f);
            float height = Mathf.Max(area.rect.height, 932f);

            const int count = 70;
            float spawnWindow = 0.9f;
            float spawned = 0f;
            float elapsed = 0f;

            while (elapsed < 8f && (pieces.Count > 0 || spawned < count))
            {
                elapsed += Time.deltaTime;

                while (spawned < count && spawned / count * spawnWindow < elapsed)
                {
                    SpawnPiece(width, height);
                    spawned++;
                }

                for (int i = pieces.Count - 1; i >= 0; i--)
                {
                    Piece p = pieces[i];
                    Vector2 pos = p.rect.anchoredPosition;
                    pos.y -= p.fallSpeed * Time.deltaTime;
                    pos.x = p.baseX + Mathf.Sin(Time.time * 2.2f + p.swayPhase) * p.swayAmplitude;
                    p.rect.anchoredPosition = pos;
                    p.rect.localRotation = Quaternion.Euler(0f, 0f, p.rect.localEulerAngles.z + p.spinSpeed * Time.deltaTime);

                    if (pos.y < -height * 0.5f - 40f)
                    {
                        Destroy(p.rect.gameObject);
                        pieces.RemoveAt(i);
                    }
                }

                yield return null;
            }

            foreach (Piece p in pieces)
            {
                if (p.rect != null)
                {
                    Destroy(p.rect.gameObject);
                }
            }

            pieces.Clear();
        }

        void SpawnPiece(float width, float height)
        {
            GameObject go = new GameObject("Confetto", typeof(RectTransform));
            go.transform.SetParent(transform, false);

            var img = go.AddComponent<Image>();
            img.sprite = Random.value < 0.35f ? UiSprites.Circle : UiSprites.Rounded;
            img.type = Image.Type.Simple;
            img.color = Theme.ConfettiColors[Random.Range(0, Theme.ConfettiColors.Length)];
            img.raycastTarget = false;

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(Random.Range(8f, 13f), Random.Range(12f, 18f));
            float x = Random.Range(-width * 0.5f + 10f, width * 0.5f - 10f);
            rt.anchoredPosition = new Vector2(x, height * 0.5f + Random.Range(10f, 120f));
            rt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            pieces.Add(new Piece
            {
                rect = rt,
                fallSpeed = Random.Range(240f, 520f),
                swayAmplitude = Random.Range(8f, 30f),
                swayPhase = Random.Range(0f, Mathf.PI * 2f),
                spinSpeed = Random.Range(-220f, 220f),
                baseX = x
            });
        }
    }
}
