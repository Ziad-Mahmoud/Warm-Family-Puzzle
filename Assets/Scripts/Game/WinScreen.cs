using UnityEngine;
using UnityEngine.UI;

namespace WarmFamilyPuzzle
{
    // Win — celebration with confetti, stats and next actions.
    // Mirrors screen 1c of the PicSlide design.
    public class WinScreen : MonoBehaviour
    {
        AppRoot app;
        WinInfo info;

        public static GameObject Build(AppRoot app, WinInfo info)
        {
            GameObject go = Ui.Stretch(app.Column, "WinScreen");
            var screen = go.AddComponent<WinScreen>();
            screen.app = app;
            screen.info = info;
            screen.BuildUi();
            return go;
        }

        void BuildUi()
        {
            AudioManager.Play("sfx_win", 0.9f);

            Text title = Ui.Label(transform, "You did it!", 34, Theme.Ink, FontStyle.Bold);
            Ui.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 306f), new Vector2(320f, 44f));

            string sub = PhotoLibrary.PhotoName(info.photoIndex) + " · " + info.gridSize + "×" + info.gridSize
                + " · " + info.mode.FullName();
            if (info.assisted)
            {
                sub += "  ·  auto-solved";
            }

            Text subtitle = Ui.Label(transform, sub, 16, Theme.Muted);
            Ui.Place(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 268f), new Vector2(360f, 24f));

            if (info.newBest)
            {
                GameObject badge = Ui.Panel(transform, "NewBest", Theme.Hex("#FFF3D6"), 16f);
                Ui.Place(Ui.Rect(badge), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 228f), new Vector2(170f, 32f));
                Text badgeText = Ui.Label(badge.transform, "★ NEW BEST!", 14, Theme.Gold, FontStyle.Bold);
                Ui.StretchInto(badgeText.rectTransform, badge.transform);
            }

            RawImage solved = Ui.MaskedPhoto(transform, "SolvedPhoto", 22f);
            var solvedHolder = (RectTransform)solved.transform.parent;
            Ui.Place(solvedHolder, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), new Vector2(280f, 280f));
            Ui.SetCoverUV(solved, PhotoLibrary.Photo(info.photoIndex), 1f);

            BuildStats();

            Button next = Ui.PillButton(transform, "Next puzzle", Theme.Coral, Color.white, 320f, 54f, 18,
                () => app.StartGame(info.photoIndex + 1, info.gridSize));
            Ui.Place((RectTransform)next.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -268f), new Vector2(320f, 54f));

            Button again = Ui.PillButton(transform, "Play again", Theme.ChipBg, Theme.Ink, 320f, 54f, 18,
                () => app.StartGame(info.photoIndex, info.gridSize));
            Ui.Place((RectTransform)again.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -334f), new Vector2(320f, 54f));

            GameObject confetti = Ui.Stretch(transform, "Confetti");
            confetti.AddComponent<Confetti>();
        }

        void BuildStats()
        {
            GameObject stats = Ui.Node(transform, "Stats");
            var rt = Ui.Rect(stats);
            Ui.Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -152f), new Vector2(380f, 74f));

            var layout = stats.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            BuildStatChip(stats.transform, SaveData.FormatTime(info.time), "TIME",
                info.newBest ? Theme.Teal : Theme.Ink);
            BuildStatChip(stats.transform, info.moves.ToString(), "MOVES", Theme.Ink);

            string delta = "—";
            Color deltaColor = Theme.Muted;
            if (info.assisted)
            {
                delta = "—";
            }
            else if (info.hadPrevBest)
            {
                float diff = info.time - info.prevBestTime;
                int seconds = Mathf.RoundToInt(Mathf.Abs(diff));
                delta = (diff <= 0f ? "−" : "+") + seconds + "s";
                deltaColor = diff <= 0f ? Theme.Teal : Theme.Muted;
            }
            else
            {
                delta = "FIRST!";
                deltaColor = Theme.Teal;
            }

            BuildStatChip(stats.transform, delta, "VS BEST", deltaColor);
        }

        void BuildStatChip(Transform parent, string value, string caption, Color valueColor)
        {
            GameObject chip = Ui.Panel(parent, "Stat_" + caption, Theme.Card, 16f);

            Text v = Ui.Label(chip.transform, value, 23, valueColor, FontStyle.Bold);
            Ui.Place(v.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(120f, 28f));

            Text c = Ui.Label(chip.transform, caption, 11, Theme.Muted, FontStyle.Bold);
            Ui.Place(c.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(120f, 14f));
        }
    }
}
