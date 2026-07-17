using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WarmFamilyPuzzle
{
    public struct WinInfo
    {
        public int photoIndex;
        public int gridSize;
        public GameMode mode;
        public int moves;
        public float time;
        public bool newBest;
        public bool hadPrevBest;
        public float prevBestTime;
        public bool assisted;
    }

    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindFirstObjectByType<AppRoot>() != null)
            {
                return;
            }

            var go = new GameObject("WarmFamilyPuzzle");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<AppRoot>();
        }
    }

    // Builds the whole app (camera, canvas, screens) from code — the scene stays empty.
    public class AppRoot : MonoBehaviour
    {
        public RectTransform Column { get; private set; }

        CanvasScaler scaler;
        GameObject currentScreen;

        void Awake()
        {
            Application.targetFrameRate = 60;
            BuildCamera();
            BuildEventSystem();
            BuildCanvas();
            AudioManager.Init(transform);
        }

        void Start()
        {
            ShowHome();
        }

        void Update()
        {
            float match = Screen.width > Screen.height ? 1f : 0.5f;
            if (!Mathf.Approximately(scaler.matchWidthOrHeight, match))
            {
                scaler.matchWidthOrHeight = match;
            }
        }

        void BuildCamera()
        {
            var camGo = new GameObject("Main Camera");
            camGo.transform.SetParent(transform, false);
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Theme.Bg;
            cam.cullingMask = 0;
            cam.orthographic = true;
        }

        void BuildEventSystem()
        {
            var esGo = new GameObject("EventSystem");
            esGo.transform.SetParent(transform, false);
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        void BuildCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(430f, 932f);
            scaler.matchWidthOrHeight = Screen.width > Screen.height ? 1f : 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject bg = Ui.Stretch(canvasGo.transform, "Background");
            var bgImage = bg.AddComponent<Image>();
            bgImage.color = Theme.Bg;
            bgImage.raycastTarget = false;

            GameObject safe = Ui.Stretch(canvasGo.transform, "SafeArea");
            safe.AddComponent<SafeAreaFitter>();

            GameObject column = Ui.Node(safe.transform, "Column");
            Column = Ui.Rect(column);
            Column.anchorMin = new Vector2(0.5f, 0f);
            Column.anchorMax = new Vector2(0.5f, 1f);
            Column.pivot = new Vector2(0.5f, 0.5f);
            Column.sizeDelta = new Vector2(430f, 0f);
        }

        void SetScreen(GameObject screen)
        {
            if (currentScreen != null)
            {
                Destroy(currentScreen);
            }

            currentScreen = screen;
            StartCoroutine(FadeIn(screen));
        }

        IEnumerator FadeIn(GameObject screen)
        {
            var group = screen.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = screen.AddComponent<CanvasGroup>();
            }

            group.alpha = 0f;
            float t = 0f;
            while (t < 0.16f && screen != null)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(t / 0.16f);
                yield return null;
            }

            if (screen != null)
            {
                group.alpha = 1f;
            }
        }

        public void ShowHome()
        {
            SetScreen(HomeScreen.Build(this));
        }

        public void StartGame(int photoIndex, int gridSize)
        {
            SetScreen(GameScreen.Build(this, photoIndex, gridSize));
        }

        public void ShowWin(WinInfo info)
        {
            SetScreen(WinScreen.Build(this, info));
        }
    }
}
