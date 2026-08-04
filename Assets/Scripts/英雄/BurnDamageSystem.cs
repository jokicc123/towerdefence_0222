using UnityEngine;

namespace CHANG
{
    public static class BurnDamageSystem
    {
        private static int activeSunCount;
        private static float currentMultiplier = 1f;

        public static float CurrentMultiplier =>
            activeSunCount > 0
                ? currentMultiplier
                : 1f;

        public static void AddSun(float multiplier)
        {
            activeSunCount++;

            currentMultiplier =
                Mathf.Max(
                    currentMultiplier,
                    multiplier,
                    1f
                );
        }

        public static void RemoveSun()
        {
            activeSunCount =
                Mathf.Max(0, activeSunCount - 1);

            if (activeSunCount == 0)
            {
                currentMultiplier = 1f;
            }
        }

        public static void Reset()
        {
            activeSunCount = 0;
            currentMultiplier = 1f;
        }
    }
}