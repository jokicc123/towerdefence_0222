using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 泛型 Singleton 基底類別。
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance =
                        FindFirstObjectByType<T>();
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}