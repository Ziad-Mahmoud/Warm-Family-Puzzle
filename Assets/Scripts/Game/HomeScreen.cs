using UnityEngine;
using UnityEngine.UI;

namespace WarmFamilyPuzzle
{
    // Home — logo, daily puzzle card, grid size picker, photo gallery.
    // Mirrors screen 1a of the PicSlide design.
    public class HomeScreen : MonoBehaviour
    {
        AppRoot app;
        Text dailyBestLabel;
        Image seg3Bg;
        Image seg4Bg;
        Text seg3Label;
        Text seg4Label;
        readonly Image[] modeBgs = new Image[4];
        readonly Text[] modeLabels = new Text[4];

        public static GameObject Build(AppRoot app)
        {
            GameObject go = Ui.Stretch(app.Column, "HomeScreen");
            go.AddComponent<HomeScreen>().Init(app);
            return go;
        }

        void Init(AppRoot appRoot)
        {
            app = appRoot;

            GameObject scrollGo = Ui.Stretch(transform, "Scroll");
            var scroll = scrollGo.AddComponent<ScrollRect>();

            GameObject viewportGo = Ui.Stretch(scrollGo.transform, "Viewport");
            viewportGo.AddComponent<RectMask2D>();
            var viewportHit = viewportGo.AddComponent<Image>();
            viewportHit.color = new Color(0f, 0f, 0f, 0f);

            GameObject contentGo = Ui.Node(viewportGo.transform, "Content");
            var contentRt = Ui.Rect(contentGo);
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = Vector2.zero;

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 14, 28);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRt;
            scroll.viewport = Ui.Rect(viewportGo);
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;

            BuildLogo(contentGo.transform);
            BuildDailyCard(contentGo.transform);
            BuildGridSizeRow(contentGo.transform);
            BuildModeRow(contentGo.transform);
            BuildGallery(contentGo.transform);
        }

        void BuildLogo(Transform parent)
        {
            GameObject logoRow = Ui.Node(parent, "Logo");
            logoRow.AddComponent<LayoutElement>().preferredHeight = 46f;
            Text logo = Ui.Label(logoRow.transform, "Warm <color=#E85D4A>Family</color> Puzzle", 34, Theme.Ink,
                FontStyle.Bold, TextAnchor.MiddleLeft);
            Ui.StretchInto(logo.rectTransform, logoRow.transform);

            GameObject tagRow = Ui.Node(parent, "Tagline");
            tagRow.AddComponent<LayoutElement>().preferredHeight = 20f;
            Text tag = Ui.Label(tagRow.transform, "Slide the tiles, rebuild the picture.", 16, Theme.Muted,
                FontStyle.Normal, TextAnchor.MiddleLeft);
            Ui.StretchInto(tag.rectTransform, tagRow.transform);
        }

        void BuildDailyCard(Transform parent)
        {
            int dailyIndex = PhotoLibrary.DailyIndex;

            GameObject card = Ui.Panel(parent, "DailyCard", Theme.Card, 24f);
            card.AddComponent<LayoutElement>().preferredHeight = 386f;

            // Photo with faint grid lines and the DAILY PUZZLE chip.
            RawImage photo = Ui.MaskedPhoto(card.transform, "DailyPhoto", 16f);
            var photoHolder = (RectTransform)photo.transform.parent;
            photoHolder.anchorMin = new Vector2(0f, 1f);
            photoHolder.anchorMax = new Vector2(1f, 1f);
            photoHolder.pivot = new Vector2(0.5f, 1f);
            photoHolder.anchoredPosition = new Vector2(0f, -12f);
            photoHolder.offsetMin = new Vector2(12f, photoHolder.offsetMin.y);
            photoHolder.offsetMax = new Vector2(-12f, photoHolder.offsetMax.y);
            photoHolder.sizeDelta = new Vector2(photoHolder.sizeDelta.x, 264f);
            Ui.SetCoverUV(photo, PhotoLibrary.Photo(dailyIndex), 374f / 264f);

            for (int i = 1; i <= 2; i++)
            {
                GameObject v = Ui.Node(photoHolder, "GridV" + i);
                var vImg = v.AddComponent<Image>();
                vImg.color = Color.white.WithAlpha(0.45f);
                vImg.raycastTarget = false;
                var vRt = Ui.Rect(v);
                vRt.anchorMin = new Vector2(i / 3f, 0f);
                vRt.anchorMax = new Vector2(i / 3f, 1f);
                vRt.sizeDelta = new Vector2(2f, 0f);

                GameObject h = Ui.Node(photoHolder, "GridH" + i);
                var hImg = h.AddComponent<Image>();
                hImg.color = Color.white.WithAlpha(0.45f);
                hImg.raycastTarget = false;
                var hRt = Ui.Rect(h);
                hRt.anchorMin = new Vector2(0f, i / 3f);
                hRt.anchorMax = new Vector2(1f, i / 3f);
                hRt.sizeDelta = new Vector2(0f, 2f);
            }

            GameObject chip = Ui.Panel(card.transform, "DailyChip", Theme.Ink.WithAlpha(0.85f), 14f);
            Ui.Place(Ui.Rect(chip), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -22f), new Vector2(118f, 28f));
            Text chipText = Ui.Label(chip.transform, "DAILY PUZZLE", 12, Color.white, FontStyle.Bold);
            Ui.StretchInto(chipText.rectTransform, chip.transform);

            Text name = Ui.Label(card.transform, PhotoLibrary.PhotoName(dailyIndex), 21, Theme.Ink,
                FontStyle.Bold, TextAnchor.MiddleLeft);
            Ui.Place(name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -290f), new Vector2(260f, 26f));

            dailyBestLabel = Ui.Label(card.transform, DailyBestText(dailyIndex), 14, Theme.Muted,
                FontStyle.Normal, TextAnchor.MiddleLeft);
            Ui.Place(dailyBestLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -320f), new Vector2(260f, 20f));

            Button play = Ui.PillButton(card.transform, "Play", Theme.Coral, Color.white, 116f, 50f, 19,
                () => app.StartGame(dailyIndex, SaveData.GridSize));
            Ui.Place((RectTransform)play.transform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-14f, 14f), new Vector2(116f, 50f));
        }

        string DailyBestText(int dailyIndex)
        {
            if (SaveData.TryGetBest(dailyIndex, SaveData.GridSize, SaveData.GameMode, out float t, out int m))
            {
                return "Best " + SaveData.FormatTime(t) + " · " + m + " mv";
            }

            return "No best yet — be the first!";
        }

        void BuildGridSizeRow(Transform parent)
        {
            GameObject row = Ui.Node(parent, "GridSizeRow");
            row.AddComponent<LayoutElement>().preferredHeight = 50f;

            Text label = Ui.Label(row.transform, "Grid size", 19, Theme.Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            Ui.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(140f, 30f));

            GameObject seg = Ui.Panel(row.transform, "Segmented", Theme.ChipBg, 23f);
            Ui.Place(Ui.Rect(seg), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-4f, 0f), new Vector2(168f, 46f));

            (seg3Bg, seg3Label) = BuildSegOption(seg.transform, "3×3", 5f, () => SelectGrid(3));
            (seg4Bg, seg4Label) = BuildSegOption(seg.transform, "4×4", 85f, () => SelectGrid(4));
            RefreshSegmented();
        }

        (Image, Text) BuildSegOption(Transform parent, string caption, float x, UnityEngine.Events.UnityAction onClick)
        {
            GameObject option = Ui.Panel(parent, "Seg_" + caption, Color.clear, 19f);
            Ui.Place(Ui.Rect(option), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(78f, 38f));

            Text t = Ui.Label(option.transform, caption, 15, Theme.Muted, FontStyle.Bold);
            Ui.StretchInto(t.rectTransform, option.transform);

            var btn = option.AddComponent<Button>();
            btn.targetGraphic = option.GetComponent<Image>();
            Ui.WireTap(btn);
            btn.onClick.AddListener(onClick);
            return (option.GetComponent<Image>(), t);
        }

        void SelectGrid(int size)
        {
            SaveData.GridSize = size;
            RefreshSegmented();
            if (dailyBestLabel != null)
            {
                dailyBestLabel.text = DailyBestText(PhotoLibrary.DailyIndex);
            }
        }

        void RefreshSegmented()
        {
            bool is3 = SaveData.GridSize == 3;
            seg3Bg.color = is3 ? Color.white : Color.clear;
            seg4Bg.color = is3 ? Color.clear : Color.white;
            seg3Label.color = is3 ? Theme.Ink : Theme.Muted;
            seg4Label.color = is3 ? Theme.Muted : Theme.Ink;
        }

        void BuildModeRow(Transform parent)
        {
            GameObject row = Ui.Node(parent, "ModeRow");
            row.AddComponent<LayoutElement>().preferredHeight = 50f;

            Text label = Ui.Label(row.transform, "Mode", 19, Theme.Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            Ui.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(90f, 30f));

            GameObject seg = Ui.Panel(row.transform, "ModeSegmented", Theme.ChipBg, 23f);
            Ui.Place(Ui.Rect(seg), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-4f, 0f), new Vector2(302f, 46f));

            for (int i = 0; i < 4; i++)
            {
                GameMode mode = (GameMode)i;
                GameObject option = Ui.Panel(seg.transform, "Mode_" + mode.ShortName(), Color.clear, 19f);
                Ui.Place(Ui.Rect(option), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(5f + i * 73f, 0f), new Vector2(71f, 38f));

                Text t = Ui.Label(option.transform, mode.ShortName(), 13, Theme.Muted, FontStyle.Bold);
                Ui.StretchInto(t.rectTransform, option.transform);

                var btn = option.AddComponent<Button>();
                btn.targetGraphic = option.GetComponent<Image>();
                Ui.WireTap(btn);
                btn.onClick.AddListener(() => SelectMode(mode));

                modeBgs[i] = option.GetComponent<Image>();
                modeLabels[i] = t;
            }

            RefreshModes();
        }

        void SelectMode(GameMode mode)
        {
            SaveData.GameMode = mode;
            RefreshModes();
            if (dailyBestLabel != null)
            {
                dailyBestLabel.text = DailyBestText(PhotoLibrary.DailyIndex);
            }
        }

        void RefreshModes()
        {
            int selected = (int)SaveData.GameMode;
            for (int i = 0; i < 4; i++)
            {
                modeBgs[i].color = i == selected ? Color.white : Color.clear;
                modeLabels[i].color = i == selected ? Theme.Ink : Theme.Muted;
            }
        }

        void BuildGallery(Transform parent)
        {
            GameObject header = Ui.Node(parent, "GalleryHeader");
            header.AddComponent<LayoutElement>().preferredHeight = 30f;
            Text title = Ui.Label(header.transform, "Your puzzles", 19, Theme.Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            Ui.StretchInto(title.rectTransform, header.transform);

            GameObject grid = Ui.Node(parent, "Gallery");
            var glg = grid.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(126f, 156f);
            glg.spacing = new Vector2(10f, 14f);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;

            for (int i = 0; i < PhotoLibrary.Count; i++)
            {
                int index = i;
                GameObject cell = Ui.Node(grid.transform, "Puzzle_" + index);

                RawImage thumb = Ui.MaskedPhoto(cell.transform, "Thumb", 18f);
                var thumbHolder = (RectTransform)thumb.transform.parent;
                Ui.Place(thumbHolder, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(126f, 126f));
                Ui.SetCoverUV(thumb, PhotoLibrary.Photo(index), 1f);

                Text name = Ui.Label(cell.transform, PhotoLibrary.PhotoName(index), 13, Theme.Ink,
                    FontStyle.Normal, TextAnchor.MiddleCenter);
                Ui.Place(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(126f, 20f));

                var btn = cell.AddComponent<Button>();
                btn.targetGraphic = thumbHolder.GetComponent<Image>();
                Ui.WireTap(btn);
                btn.onClick.AddListener(() => app.StartGame(index, SaveData.GridSize));
                cell.AddComponent<PressScale>();
            }
        }
    }
}
