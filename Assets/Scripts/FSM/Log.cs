using UnityEngine;

namespace CHANG
{
    /// <summary>
    /// 輸出
    /// </summary>
    public class Log


    {
        public static string Text(string message, string color = "#f77")
        {
            string result = $"<color={color}>{message}</color>";
            Debug.Log(result);
            return result;

        }


    }

}
