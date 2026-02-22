using UnityEngine;
using System.Collections;
using System;
namespace CHANG 
{
    /// <summary>
    /// 淡入淡出系統
    /// </summary>
    public class FadeSystem
    {
        /// <summary>
        /// 淡入淡出
        /// </summary>
        /// <param name="group"></param>
        /// <param name="fadIn"></param>
        /// <param name="interval"></param>
        /// <param name="delay"></param>
        /// <param name="finish"></param>
        /// <returns></returns>
        public static IEnumerator Fade(CanvasGroup group, bool fadIn = true, float interval = 0.03f, float delay = 0, Action finish = null)
        {

            yield return new WaitForSeconds(delay);

            float increase = fadIn ? +0.1f : -0.1f;
            for (int i = 0; i < 10; i++)

            {

                group.alpha += increase;
                yield return new WaitForSeconds(interval);


            }
            group.interactable = fadIn;
            group.blocksRaycasts = fadIn;
            finish?.Invoke();
        }
    }
}
