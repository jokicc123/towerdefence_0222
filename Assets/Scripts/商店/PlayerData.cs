using UnityEngine;

namespace CHANG
{
    public static class PlayerData
    {
        private const string CrystalKey = "水晶";

        public static int Crystal
        {
            get => PlayerPrefs.GetInt(CrystalKey, 0);
            set => PlayerPrefs.SetInt(CrystalKey, value);
        }

        public static void AddCrystal(int amount)
        {
            Crystal += amount;
            PlayerPrefs.Save();
        }

        public static bool SpendCrystal(int amount)
        {
            if (Crystal < amount)
                return false;

            Crystal -= amount;
            PlayerPrefs.Save();
            return true;
        }
    }
}