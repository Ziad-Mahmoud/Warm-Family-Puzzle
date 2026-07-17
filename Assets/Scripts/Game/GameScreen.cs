using System.Collections;
using System.Collections.Generic;
using PuzzleCore;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace WarmFamilyPuzzle
{
    // Game — stats chips, photo tile board, hint peek, shuffle & auto-solve.
    // Mirrors screen 1b of the PicSlide design; tile moves are validated by
    // the ported PuzzleCore rules.
    public class GameScreen : MonoBehaviour
    {
        AppRoot app;
        int photoIndex;
        int gridSize;
        GameMode mode;
        GameSession session;
        Texture2D photo;
        GridElement selectedTile;
        RectTransform selectionRing;

        readonly Dictionary<GridElement, RectTransform> tileViews = new Dictionary<GridElement, RectTransform>();
        readonly List<GameObject> badges = new List<GameObject>();
        RectTransform tilesRoot;
        RectTransform emptyMarker;
        RectTransform boardRt;

        Text movesValue;
        Text timeValue;
        Text peeksLabel;
        Button peekButton;
        Image numbersToggleBg;
        Text numbersToggleText;
        CanvasGroup peekOverlay;
        GameObject pauseOverlay;

        float elapsed;
        bool running;
        bool paused;
        bool inputLocked;
        bool finished;
        float cellSize;
        const float TileGap = 8f;
        const float BoardPad = 12f;

        public static GameObject Build(AppRoot app, int photoIndex, int gridSize)
        {
            Canvas.ForceUpdateCanvases();
            GameObject go = Ui.Stretch(app.Column, "GameScreen");
            go.AddComponent<GameScreen>().Init(app, photoIndex, gridSize);
            return go;
        }

        void Init(AppRoot appRoot, int photoIdx, int size)
        {
            app = appRoot;
            photoIndex = photoIdx;
            gridSize = size;
            mode = SaveData.GameMode;
            photo = PhotoLibrary.Photo(photoIndex);

            session = new GameSession(gridSize, mode);
            session.Shuffle();

            BuildHeader();
            BuildStats();
            BuildBoard();
            BuildHintCard();
            BuildActionRow();
            BuildPeekOverlay();

            running = true;
        }

        void Update()
        {
            if (running && !paused && !finished)
            {
                elapsed += Time.deltaTime;
                timeValue.text = SaveData.FormatTime(elapsed);
            }

            HandleKeyboard();
        }

        // --- construction -------------------------------------------------

        void BuildHeader()
        {
            GameObject header = Ui.Node(transform, "Header");
            Ui.PlaceTopStretch(Ui.Rect(header), 10f, 46f, 12f, 12f);

            Button back = Ui.CircleButton(header.transform, "‹", Theme.ChipBg, Theme.Ink, 44f, 26, () => app.ShowHome());
            Ui.Place((RectTransform)back.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(44f, 44f));

            Text title = Ui.Label(header.transform, PhotoLibrary.PhotoName(photoIndex), 16, Theme.Ink, FontStyle.Bold);
            Ui.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 9f), new Vector2(220f, 20f));
            title.horizontalOverflow = HorizontalWrapMode.Wrap;

            Text subtitle = Ui.Label(header.transform, gridSize + "×" + gridSize + " · " + mode.FullName(), 12, Theme.Muted, FontStyle.Bold);
            Ui.Place(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -11f), new Vector2(220f, 16f));

            Button pause = Ui.CircleButton(header.transform, "II", Theme.ChipBg, Theme.Ink, 44f, 15, TogglePause);
            Ui.Place((RectTransform)pause.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-22f, 0f), new Vector2(44f, 44f));

            Button numbers = Ui.CircleButton(header.transform, "#", Theme.ChipBg, Theme.Muted, 44f, 20, ToggleNumbers);
            Ui.Place((RectTransform)numbers.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-74f, 0f), new Vector2(44f, 44f));
            numbersToggleBg = numbers.targetGraphic as Image;
            numbersToggleText = numbers.GetComponentInChildren<Text>();
            RefreshNumbersToggle();
        }

        void BuildStats()
        {
            GameObject stats = Ui.Node(transform, "Stats");
            Ui.PlaceTopStretch(Ui.Rect(stats), 66f, 74f, 12f, 12f);
            var layout = stats.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            movesValue = BuildStatChip(stats.transform, "0", "MOVES", Theme.Ink);
            timeValue = BuildStatChip(stats.transform, "0:00", "TIME", Theme.Ink);

            string bestText = SaveData.TryGetBest(photoIndex, gridSize, mode, out float bestTime, out _)
                ? SaveData.FormatTime(bestTime)
                : "—";
            BuildStatChip(stats.transform, bestText, "BEST", Theme.Teal);
        }

        Text BuildStatChip(Transform parent, string value, string caption, Color valueColor)
        {
            GameObject chip = Ui.Panel(parent, "Stat_" + caption, Theme.Card, 16f);

            Text v = Ui.Label(chip.transform, value, 23, valueColor, FontStyle.Bold);
            Ui.Place(v.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(120f, 28f));

            Text c = Ui.Label(chip.transform, caption, 11, Theme.Muted, FontStyle.Bold);
            Ui.Place(c.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(120f, 14f));
            return v;
        }

        void BuildBoard()
        {
            float columnWidth = app.Column.rect.width > 1f ? app.Column.rect.width : 430f;
            float columnHeight = app.Column.rect.height > 1f ? app.Column.rect.height : 932f;
            const float topReserve = 156f;
            const float bottomReserve = 184f;
            float available = columnHeight - topReserve - bottomReserve;
            float side = Mathf.Min(columnWidth - 24f, available);

            GameObject board = Ui.Panel(transform, "Board", Theme.BoardBg, 26f);
            boardRt = Ui.Rect(board);
            boardRt.anchorMin = new Vector2(0.5f, 1f);
            boardRt.anchorMax = new Vector2(0.5f, 1f);
            boardRt.pivot = new Vector2(0.5f, 1f);
            boardRt.anchoredPosition = new Vector2(0f, -(topReserve + Mathf.Max(0f, (available - side) * 0.5f)));
            boardRt.sizeDelta = new Vector2(side, side);

            GameObject tiles = Ui.Node(board.transform, "Tiles");
            tilesRoot = Ui.Rect(tiles);
            tilesRoot.anchorMin = Vector2.zero;
            tilesRoot.anchorMax = Vector2.one;
            tilesRoot.offsetMin = new Vector2(BoardPad, BoardPad);
            tilesRoot.offsetMax = new Vector2(-BoardPad, -BoardPad);

            float inner = side - BoardPad * 2f;
            cellSize = (inner - TileGap * (gridSize - 1)) / gridSize;

            if (mode == GameMode.Sliding)
            {
                BuildEmptyMarker();
            }

            for (int x = 0; x < gridSize; x++)
            {
                for (int y = 0; y < gridSize; y++)
                {
                    GridElement element = session.Puzzle.GridElements[x][y];
                    if (element is EmptySpot)
                    {
                        continue;
                    }

                    BuildTile(element);
                }
            }

            SyncEmptyMarker();
            BuildSelectionRing();
        }

        void BuildEmptyMarker()
        {
            GameObject marker = Ui.Panel(tilesRoot, "EmptyCell", Theme.EmptyCell.WithAlpha(0.55f), 14f);
            emptyMarker = Ui.Rect(marker);
            SetupCellRect(emptyMarker);
            marker.GetComponent<Image>().raycastTarget = false;

            GameObject ring = Ui.Stretch(marker.transform, "Ring");
            Image ringImg = Ui.RingImage(ring, Theme.Muted.WithAlpha(0.35f), 14f);
            ringImg.raycastTarget = false;
        }

        void BuildTile(GridElement element)
        {
            RawImage raw = Ui.MaskedPhoto(tilesRoot, "Tile_" + element.valuePiece, 14f);
            var holder = (RectTransform)raw.transform.parent;
            SetupCellRect(holder);
            holder.anchoredPosition = CellPosition(element.currentIndex.x, element.currentIndex.y);

            raw.texture = photo;
            raw.uvRect = Ui.TileUV(photo, element.solutionIndex.x, element.solutionIndex.y, gridSize);

            float badgeSize = Mathf.Clamp(cellSize * 0.26f, 20f, 28f);
            GameObject badge = Ui.Node(holder, "Badge");
            var badgeImg = badge.AddComponent<Image>();
            badgeImg.sprite = UiSprites.Circle;
            badgeImg.color = Theme.Card.WithAlpha(0.95f);
            badgeImg.raycastTarget = false;
            Ui.Place(Ui.Rect(badge), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(7f + badgeSize * 0.5f, -7f - badgeSize * 0.5f), new Vector2(badgeSize, badgeSize));
            Text num = Ui.Label(badge.transform, element.valuePiece.ToString(), Mathf.RoundToInt(badgeSize * 0.5f), Theme.Ink, FontStyle.Bold);
            Ui.StretchInto(num.rectTransform, badge.transform);
            badge.SetActive(SaveData.ShowTileNumbers);
            badges.Add(badge);

            var btn = holder.gameObject.AddComponent<Button>();
            btn.targetGraphic = holder.GetComponent<Image>();
            GridElement captured = element;
            btn.onClick.AddListener(() => OnTileTapped(captured));

            tileViews[element] = holder;
        }

        void SetupCellRect(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(cellSize, cellSize);
        }

        Vector2 CellPosition(int row, int col)
        {
            return new Vector2(col * (cellSize + TileGap), -row * (cellSize + TileGap));
        }

        void BuildSelectionRing()
        {
            GameObject ring = Ui.Node(tilesRoot, "SelectionRing");
            Image img = Ui.RingImage(ring, Theme.Coral, 16f);
            img.raycastTarget = false;
            selectionRing = Ui.Rect(ring);
            selectionRing.anchorMin = new Vector2(0f, 1f);
            selectionRing.anchorMax = new Vector2(0f, 1f);
            selectionRing.pivot = new Vector2(0f, 1f);
            selectionRing.sizeDelta = new Vector2(cellSize, cellSize);
            ring.SetActive(false);
        }

        void SetSelected(GridElement tile)
        {
            selectedTile = tile;
            if (selectionRing == null)
            {
                return;
            }

            if (tile == null)
            {
                selectionRing.gameObject.SetActive(false);
            }
            else
            {
                selectionRing.gameObject.SetActive(true);
                selectionRing.anchoredPosition = CellPosition(tile.currentIndex.x, tile.currentIndex.y);
            }
        }

        void BuildHintCard()
        {
            GameObject card = Ui.Panel(transform, "HintCard", Theme.Card, 20f);
            var rt = Ui.Rect(card);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 70f);
            rt.offsetMin = new Vector2(12f, 70f);
            rt.offsetMax = new Vector2(-12f, 70f + 90f);

            RawImage thumb = Ui.MaskedPhoto(card.transform, "HintThumb", 12f);
            var thumbHolder = (RectTransform)thumb.transform.parent;
            Ui.Place(thumbHolder, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(56f, 56f));
            Ui.SetCoverUV(thumb, photo, 1f);

            Text title = Ui.Label(card.transform, "Hint peek", 16, Theme.Ink, FontStyle.Bold, TextAnchor.MiddleLeft);
            Ui.Place(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(86f, 12f), new Vector2(160f, 22f));

            peeksLabel = Ui.Label(card.transform, PeeksText(), 13, Theme.Muted, FontStyle.Normal, TextAnchor.MiddleLeft);
            Ui.Place(peeksLabel.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(86f, -12f), new Vector2(180f, 18f));

            peekButton = Ui.PillButton(card.transform, "Peek", Theme.Yellow, Theme.Ink, 92f, 44f, 16, OnPeek);
            Ui.Place((RectTransform)peekButton.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(92f, 44f));
            RefreshPeekButton();
        }

        void BuildActionRow()
        {
            GameObject row = Ui.Node(transform, "Actions");
            var rt = Ui.Rect(row);
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.offsetMin = new Vector2(12f, 12f);
            rt.offsetMax = new Vector2(-12f, 12f + 48f);

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            BuildActionButton(row.transform, "Shuffle", () =>
            {
                AudioManager.Play("sfx_shuffle", 0.8f);
                app.StartGame(photoIndex, gridSize);
            });
            BuildActionButton(row.transform, "Auto-solve", OnAutoSolve);
        }

        void BuildActionButton(Transform parent, string caption, UnityEngine.Events.UnityAction onClick)
        {
            GameObject go = Ui.Panel(parent, "Action_" + caption, Theme.ChipBg, 24f);
            Text t = Ui.Label(go.transform, caption, 15, Theme.Ink, FontStyle.Bold);
            Ui.StretchInto(t.rectTransform, go.transform);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();
            Ui.WireTap(btn);
            btn.onClick.AddListener(onClick);
            go.AddComponent<PressScale>();
        }

        void BuildPeekOverlay()
        {
            RawImage full = Ui.MaskedPhoto(boardRt, "PeekOverlay", 22f);
            var holder = (RectTransform)full.transform.parent;
            holder.anchorMin = Vector2.zero;
            holder.anchorMax = Vector2.one;
            holder.offsetMin = new Vector2(6f, 6f);
            holder.offsetMax = new Vector2(-6f, -6f);
            Ui.SetCoverUV(full, photo, 1f);

            peekOverlay = holder.gameObject.AddComponent<CanvasGroup>();
            peekOverlay.alpha = 0f;
            peekOverlay.blocksRaycasts = false;
            peekOverlay.interactable = false;
        }

        // --- gameplay ------------------------------------------------------

        void OnTileTapped(GridElement element)
        {
            if (inputLocked || paused || finished)
            {
                return;
            }

            if (mode == GameMode.Sliding)
            {
                if (!session.TryMoveTile(element))
                {
                    return;
                }

                AudioManager.Play("sfx_slide", 0.9f);
                movesValue.text = session.Moves.ToString();
                StartCoroutine(SlideTile(element));
                return;
            }

            // Swap modes: first tap selects, second tap swaps if the rules allow.
            if (selectedTile == null)
            {
                SetSelected(element);
                return;
            }

            if (selectedTile == element)
            {
                SetSelected(null);
                return;
            }

            GridElement from = selectedTile;
            if (!session.TrySwap(from, element))
            {
                SetSelected(element);
                return;
            }

            SetSelected(null);
            AudioManager.Play("sfx_slide", 0.9f);
            movesValue.text = session.Moves.ToString();
            StartCoroutine(SwapTiles(from, element));
        }

        IEnumerator SwapTiles(GridElement a, GridElement b)
        {
            inputLocked = true;
            RectTransform rtA = tileViews[a];
            RectTransform rtB = tileViews[b];
            Vector2 fromA = rtA.anchoredPosition;
            Vector2 fromB = rtB.anchoredPosition;
            Vector2 toA = CellPosition(a.currentIndex.x, a.currentIndex.y);
            Vector2 toB = CellPosition(b.currentIndex.x, b.currentIndex.y);

            float t = 0f;
            const float duration = 0.15f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / duration), 2f);
                rtA.anchoredPosition = Vector2.LerpUnclamped(fromA, toA, k);
                rtB.anchoredPosition = Vector2.LerpUnclamped(fromB, toB, k);
                yield return null;
            }

            rtA.anchoredPosition = toA;
            rtB.anchoredPosition = toB;
            inputLocked = false;

            if (session.IsSolved)
            {
                yield return Finish();
            }
        }

        IEnumerator SlideTile(GridElement element)
        {
            inputLocked = true;
            RectTransform rt = tileViews[element];
            Vector2 from = rt.anchoredPosition;
            Vector2 to = CellPosition(element.currentIndex.x, element.currentIndex.y);
            SyncEmptyMarker();

            float t = 0f;
            const float duration = 0.12f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / duration), 2f);
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
                yield return null;
            }

            rt.anchoredPosition = to;
            inputLocked = false;

            if (session.IsSolved)
            {
                yield return Finish();
            }
        }

        IEnumerator Finish()
        {
            finished = true;
            running = false;
            inputLocked = true;

            bool hadPrev = SaveData.TryGetBest(photoIndex, gridSize, mode, out float prevBest, out _);
            bool newBest = !session.Assisted && SaveData.SubmitScore(photoIndex, gridSize, mode, elapsed, session.Moves);

            yield return new WaitForSeconds(0.45f);

            app.ShowWin(new WinInfo
            {
                photoIndex = photoIndex,
                gridSize = gridSize,
                mode = mode,
                moves = session.Moves,
                time = elapsed,
                newBest = newBest,
                hadPrevBest = hadPrev,
                prevBestTime = prevBest,
                assisted = session.Assisted
            });
        }

        void HandleKeyboard()
        {
            if (inputLocked || paused || finished)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
                return;
            }

            if (mode != GameMode.Sliding)
            {
                return;
            }

            GridElement empty = session.EmptySpot;
            int ex = empty.currentIndex.x;
            int ey = empty.currentIndex.y;

            // Arrow = direction the tile slides: Left slides the tile right of
            // the empty spot to the left, and so on.
            GridElement target = null;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                target = session.TileAt(ex, ey + 1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                target = session.TileAt(ex, ey - 1);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                target = session.TileAt(ex + 1, ey);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                target = session.TileAt(ex - 1, ey);
            }

            if (target != null)
            {
                OnTileTapped(target);
            }
        }

        void SyncEmptyMarker()
        {
            GridElement empty = session.EmptySpot;
            if (empty != null && emptyMarker != null)
            {
                emptyMarker.anchoredPosition = CellPosition(empty.currentIndex.x, empty.currentIndex.y);
            }
        }

        void SyncTile(GridElement element)
        {
            if (element is EmptySpot)
            {
                SyncEmptyMarker();
            }
            else if (tileViews.TryGetValue(element, out RectTransform rt))
            {
                rt.anchoredPosition = CellPosition(element.currentIndex.x, element.currentIndex.y);
            }
        }

        // --- hint peek -----------------------------------------------------

        string PeeksText()
        {
            int left = SaveData.PeeksLeftToday;
            return left == 1 ? "1 peek left today" : left + " peeks left today";
        }

        void RefreshPeekButton()
        {
            bool available = SaveData.PeeksLeftToday > 0;
            peekButton.interactable = available;
            var img = peekButton.targetGraphic as Image;
            if (img != null)
            {
                img.color = available ? Theme.Yellow : Theme.Yellow.WithAlpha(0.4f);
            }
        }

        void OnPeek()
        {
            if (SaveData.PeeksLeftToday <= 0 || inputLocked || paused || finished)
            {
                return;
            }

            SaveData.ConsumePeek();
            AudioManager.Play("sfx_peek", 0.8f);
            peeksLabel.text = PeeksText();
            RefreshPeekButton();
            StartCoroutine(PeekRoutine());
        }

        IEnumerator PeekRoutine()
        {
            inputLocked = true;
            yield return Fade(peekOverlay, 0f, 1f, 0.22f);
            yield return new WaitForSeconds(2f);
            yield return Fade(peekOverlay, 1f, 0f, 0.35f);
            inputLocked = false;
        }

        IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }

            group.alpha = to;
        }

        // --- auto-solve ----------------------------------------------------

        void OnAutoSolve()
        {
            if (inputLocked || paused || finished)
            {
                return;
            }

            StartCoroutine(AutoSolveRoutine());
        }

        IEnumerator AutoSolveRoutine()
        {
            inputLocked = true;
            running = false;
            SetSelected(null);

            AudioManager.Play("sfx_shuffle", 0.8f);
            List<Move> moves = session.TakeAutoSolveMoves();
            float interval = Mathf.Min(0.05f, 6f / Mathf.Max(moves.Count, 1));

            foreach (Move move in moves)
            {
                session.ApplyAutoSolveMove(move);
                SyncTile(move.From);
                SyncTile(move.To);
                yield return new WaitForSeconds(interval);
            }

            yield return Finish();
        }

        // --- pause & toggles -------------------------------------------------

        void TogglePause()
        {
            if (finished)
            {
                return;
            }

            paused = !paused;
            if (paused && pauseOverlay == null)
            {
                BuildPauseOverlay();
            }

            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(paused);
            }
        }

        void BuildPauseOverlay()
        {
            pauseOverlay = Ui.Stretch(transform, "PauseOverlay");
            var img = pauseOverlay.AddComponent<Image>();
            img.color = Theme.Bg.WithAlpha(0.96f);

            Text title = Ui.Label(pauseOverlay.transform, "Paused", 32, Theme.Ink, FontStyle.Bold);
            Ui.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 90f), new Vector2(240f, 44f));

            Button resume = Ui.PillButton(pauseOverlay.transform, "Resume", Theme.Coral, Color.white, 240f, 54f, 18, TogglePause);
            Ui.Place((RectTransform)resume.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(240f, 54f));

            Button home = Ui.PillButton(pauseOverlay.transform, "Home", Theme.ChipBg, Theme.Ink, 240f, 54f, 18, () => app.ShowHome());
            Ui.Place((RectTransform)home.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -58f), new Vector2(240f, 54f));
        }

        void ToggleNumbers()
        {
            SaveData.ShowTileNumbers = !SaveData.ShowTileNumbers;
            foreach (GameObject badge in badges)
            {
                if (badge != null)
                {
                    badge.SetActive(SaveData.ShowTileNumbers);
                }
            }

            RefreshNumbersToggle();
        }

        void RefreshNumbersToggle()
        {
            bool on = SaveData.ShowTileNumbers;
            if (numbersToggleBg != null)
            {
                numbersToggleBg.color = on ? Theme.Coral : Theme.ChipBg;
            }

            if (numbersToggleText != null)
            {
                numbersToggleText.color = on ? Color.white : Theme.Muted;
            }
        }
    }
}
