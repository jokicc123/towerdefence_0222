using UnityEngine;
namespace CHANG
{
    /// <summary>
    /// 單例模式
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<T>();
                return instance;

            }
        }

    }
}
