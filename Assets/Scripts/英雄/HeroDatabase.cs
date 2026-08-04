using UnityEngine;

namespace CHANG
{
    [CreateAssetMenu(
        fileName = "HeroDatabase",
        menuName = "CHANG/Hero Database"
    )]
    public class HeroDatabase : ScriptableObject
    {
        public HeroData[] heroes;
    }
}