# Warm Family Puzzle

A warm, family-photo sliding puzzle built with Unity 6 (6000.3.8f1) for **Web (WebGL), PC (Windows), Android and iPhone/iPad**.

- **Gameplay engine** ported from `D:\C#\PuzzleGame` (`PuzzleCore` — grid elements, move rules, solution checker). The rule chain used in-game is the `SlidingPuzzle` composition: `NotSameRule + NearRule + NearEdgeRule + ToEmptyRule`.
- **Visual design** follows the "PicSlide" Claude design (Sliding Puzzle.dc.html): warm cream background `#F3EDE2`, coral `#E85D4A`, teal `#2EA79B`, rounded cards, stat chips, numbered tile badges, hint peek, win confetti.
- **Photos**: the 14 family photos in `Assets/Resources/Photos` (from Downloads, newest 14). Replace or add JPGs there to change the puzzle gallery.

## What's in the game

| Screen | Features |
|---|---|
| Home | Logo + tagline, Daily Puzzle card (photo of the day + best score + Play), 3×3 / 4×4 grid-size picker, game-mode picker, "Your puzzles" gallery of all 14 photos |
| Game | MOVES / TIME / BEST chips, photo tile board with optional number badges (# toggle), tap or arrow-keys/WASD to slide (tap two tiles in swap modes), Hint peek (3/day), Shuffle, Auto-solve, pause |
| Win | Confetti, solved photo, TIME / MOVES / VS BEST chips, ★ NEW BEST! badge, Next puzzle / Play again |

**Game modes** — the four rule sets from the original engine, easiest → hardest:

| Mode | Original class | Rules |
|---|---|---|
| Free | `DistantMovePuzzleGame` | swap any two tiles |
| Near | `DiagonalMovePuzzleGame` | swap neighbours incl. diagonals |
| Cross | `HorizontalVerticalPuzzleGame` | swap horizontal/vertical neighbours |
| Slide | `SlidingPuzzle` | classic sliding puzzle with empty spot |

Best times are saved per photo + grid size + mode (PlayerPrefs, works on all platforms including WebGL).

## Notes

- The whole UI is built from code (`Assets/Scripts/Game`) — the scene is intentionally empty; `GameBootstrap` constructs everything at startup. No art assets are required: rounded corners, circles and confetti are procedural.
- The engine port fixed two things from the original console project: `PuzzleShuffler` now shuffles using only rule-valid moves (arbitrary swaps can create unsolvable boards — see `PuzzleEngineTest/PuzzleSolvabilityTest.cs` in the original repo), and the console-only classes (`ConsoleInputHandler`, `PuzzleLogger`, `PuzzleGameManager`) were not ported since Unity replaces them.
- **Font**: currently uses Unity's built-in font. For the exact design look, download [Nunito](https://fonts.google.com/specimen/Nunito) (OFL license), drop the TTF in `Assets/Fonts`, and assign it in `Ui.DefaultFont`.

## Building

Menu: **Warm Family Puzzle ▸ Build ▸ …**

| Target | Output | Requirement |
|---|---|---|
| Windows x64 | `Builds/Windows/WarmFamilyPuzzle.exe` | installed ✔ |
| WebGL | `Builds/WebGL` (host the folder anywhere; decompression fallback is on) | WebGL module for 6000.3.8f1 |
| Android APK | `Builds/Android/WarmFamilyPuzzle.apk` | installed ✔ |
| iOS | `Builds/iOS` Xcode project | iOS module ✔ (compiling the Xcode project needs a Mac) |

`Warm Family Puzzle ▸ Configure Project` re-applies player settings (product name, portrait orientation, `com.zizoelzoz.warmfamilypuzzle`, photo import settings).
