using System;
using UnityEngine;

namespace WarmFamilyPuzzle
{
    public static class SaveData
    {
        public const int PeeksPerDay = 3;

        public static int GridSize
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt("grid_size", 3), 3, 4);
            set
            {
                PlayerPrefs.SetInt("grid_size", value);
                PlayerPrefs.Save();
            }
        }

        public static GameMode GameMode
        {
            get => (GameMode)Mathf.Clamp(PlayerPrefs.GetInt("game_mode", (int)WarmFamilyPuzzle.GameMode.Sliding), 0, 3);
            set
            {
                PlayerPrefs.SetInt("game_mode", (int)value);
                PlayerPrefs.Save();
            }
        }

        public static bool ShowTileNumbers
        {
            get => PlayerPrefs.GetInt("show_numbers", 1) == 1;
            set
            {
                PlayerPrefs.SetInt("show_numbers", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool TryGetBest(int photoIndex, int gridSize, GameMode mode, out float time, out int moves)
        {
            string key = BestKey(photoIndex, gridSize, mode);
            if (!PlayerPrefs.HasKey(key + "_t"))
            {
                time = 0f;
                moves = 0;
                return false;
            }

            time = PlayerPrefs.GetFloat(key + "_t");
            moves = PlayerPrefs.GetInt(key + "_m");
            return true;
        }

        // Best run = fastest time; the move count of that run is stored with it.
        public static bool SubmitScore(int photoIndex, int gridSize, GameMode mode, float time, int moves)
        {
            bool had = TryGetBest(photoIndex, gridSize, mode, out float bestTime, out _);
            if (had && time >= bestTime)
            {
                return false;
            }

            string key = BestKey(photoIndex, gridSize, mode);
            PlayerPrefs.SetFloat(key + "_t", time);
            PlayerPrefs.SetInt(key + "_m", moves);
            PlayerPrefs.Save();
            return true;
        }

        public static int PeeksLeftToday
        {
            get => Mathf.Max(0, PeeksPerDay - PlayerPrefs.GetInt(PeekKey, 0));
        }

        public static void ConsumePeek()
        {
            PlayerPrefs.SetInt(PeekKey, PlayerPrefs.GetInt(PeekKey, 0) + 1);
            PlayerPrefs.Save();
        }

        static string PeekKey => "peeks_" + DateTime.Today.ToString("yyyyMMdd");

        static string BestKey(int photoIndex, int gridSize, GameMode mode)
        {
            return $"best_{photoIndex}_{gridSize}_{(int)mode}";
        }

        public static string FormatTime(float seconds)
        {
            int total = Mathf.FloorToInt(seconds);
            return $"{total / 60}:{total % 60:D2}";
        }
    }
}
