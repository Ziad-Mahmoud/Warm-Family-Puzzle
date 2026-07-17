using System;
using System.Linq;
using UnityEngine;

namespace WarmFamilyPuzzle
{
    public static class PhotoLibrary
    {
        static readonly string[] Names =
        {
            "Morning Hugs", "Sunday Smiles", "Cozy Corner", "Together Time",
            "Sweet Moments", "Little Laughs", "Warm Hearts", "Golden Hour",
            "Home Sweet Home", "Playtime Joy", "Family Feast", "Snuggle Season",
            "Happy Days", "Love & Light"
        };

        static Texture2D[] photos;

        public static Texture2D[] Photos
        {
            get
            {
                if (photos == null)
                {
                    photos = Resources.LoadAll<Texture2D>("Photos")
                        .OrderBy(t => t.name, StringComparer.Ordinal)
                        .ToArray();
                }

                return photos;
            }
        }

        public static int Count => Photos.Length;

        public static Texture2D Photo(int index)
        {
            return Photos[Mod(index, Count)];
        }

        public static string PhotoName(int index)
        {
            int i = Mod(index, Count);
            return i < Names.Length ? Names[i] : "Family Photo " + (i + 1);
        }

        public static int DailyIndex
        {
            get
            {
                DateTime today = DateTime.Today;
                return Mod(today.Year * 366 + today.DayOfYear, Count);
            }
        }

        static int Mod(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }

            return ((value % count) + count) % count;
        }
    }
}
