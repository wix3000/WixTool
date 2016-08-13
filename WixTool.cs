using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class WixTools {

    #region 公共方法

    /// <summary>
    /// 取得系統資訊
    /// </summary>
    /// <returns>返回的字串</returns>
    public static string GetSystemInfo() {
        string info = string.Format(@"
CPU型號：{0}
核心數：{1}
記憶體：{2}
顯示晶片：{3}
解析度：{4} × {5}
畫面更新率：{6}
顯示記憶體：{7}
",
            SystemInfo.processorType,
            SystemInfo.processorCount,
            SystemInfo.systemMemorySize,
            SystemInfo.graphicsDeviceName,
            Screen.width,
            Screen.height,
            Screen.currentResolution.refreshRate,
            SystemInfo.graphicsMemorySize);

        return info;
    }

    /// <summary>
    /// 取得MD5碼
    /// </summary>
    /// <param name="originString">來源字串</param>
    /// <param name="isShort">位數 True: 16位  False: 32位</param>
    /// <param name="isToUpper">大小寫 True: 大寫  False: 小寫</param>
    /// <returns>轉換後的MD5碼</returns>
    public static string MD5convert(string originString, bool isShort = false, bool isToUpper = false) {

        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        string output = (isShort) ?
            BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(originString)), 4, 8) :
            BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(originString)));

        output.Replace("-", "");

        return (isToUpper) ?
            output.ToUpper() :
            output.ToLower();
    }

    /// <summary>
    /// 實例化子物件。
    /// </summary>
    /// <param name="parent">父物件</param>
    /// <param name="prefab">實例化預置物</param>
    /// <returns>實例化物件</returns>
    public static GameObject InstantiateChild(Transform parent, MonoBehaviour prefab) {

        Transform t = UnityEngine.Object.Instantiate(prefab).transform;
        Transform c = prefab.transform;

        if(parent != null) {
            t.SetParent(parent);
            RectTransform rect = t as RectTransform;
            if (rect) {
                RectTransform cRect = c as RectTransform;
                rect.anchoredPosition = cRect.anchoredPosition;
                rect.sizeDelta = cRect.sizeDelta;
                rect.localScale = cRect.localScale;
                return rect.gameObject;
            } else {
                t.localPosition = c.localPosition;
                t.localScale = c.localScale;
            }
        }
        return t.gameObject;
    }

    /// <summary>
    /// 實例化子物件
    /// </summary>
    /// <typeparam name="T">回傳類型</typeparam>
    /// <param name="parent">父物件</param>
    /// <param name="prefab">實例化預置物</param>
    /// <returns>實例化物件</returns>
    public static T InstantiateChild<T>(Transform parent, MonoBehaviour prefab) where T : MonoBehaviour {

        Transform t = UnityEngine.Object.Instantiate(prefab).transform;
        Transform c = prefab.transform;

        if (parent != null) {
            t.SetParent(parent);
            RectTransform rect = t as RectTransform;
            if (rect) {
                RectTransform cRect = c as RectTransform;
                rect.anchoredPosition = cRect.anchoredPosition;
                rect.sizeDelta = cRect.sizeDelta;
                rect.localScale = cRect.localScale;
                return rect.GetComponent<T>();
            }
            else {
                t.localPosition = c.localPosition;
                t.localScale = c.localScale;
            }
        }
        return t.GetComponent<T>();
    }

    /// <summary>
    /// 將群組內的其中一個物件特定屬性開啟，其他物件關閉。
    /// </summary>
    /// <param name="target">要開啟的目標</param>
    /// <param name="setter">要設定的屬性</param>
    /// <param name="group">群組</param>
    public static void SetObjectToggle(object target, Action<object, bool> setter, params object[] group) {
        foreach(object member in group) {
            setter(member, (member == target));
        }
    }

    /// <summary>
    /// 變更Vector3的快捷方法
    /// </summary>
    /// <param name="origin">原始向量</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static Vector3 Change(this Vector3 origin, float? x = null, float? y = null, float? z = null) {
        if (x.HasValue) origin.x = x.Value;
        if (y.HasValue) origin.y = y.Value;
        if (z.HasValue) origin.z = z.Value;
        return origin;
    }

    /// <summary>
    /// 變更Vector2的快捷方法
    /// </summary>
    /// <param name="origin">原始向量</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector2 Change(this Vector2 origin, float? x = null, float? y = null) {
        if (x.HasValue) origin.x = x.Value;
        if (y.HasValue) origin.y = y.Value;
        return origin;
    }

    /// <summary>
    /// 將數字轉為中文數字
    /// </summary>
    /// <param name="input"></param>
    /// <param name="toUpper"></param>
    /// <returns></returns>
    public static string ChineseNumbic(string input, bool toUpper = false) {
        string zh_numbic = (toUpper) ?
            "零壹貳參肆伍陸柒捌玖" :
            "Ｏ一二三四五六七八九" ;

        string output = Regex.Replace(input, "[\\d]", (Match m) =>
        {
            return zh_numbic[m.Value[0] - '0'].ToString();
        });
        return output;
    }

    /// <summary>
    /// 將遊戲物件的可用性反轉。
    /// </summary>
    /// <param name="gameObject">要反轉的物件</param>
    public static void SetActiveRelatively(this GameObject gameObject) {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    #endregion

    // UI相關
    public struct UI {

        /// <summary>
        /// 判斷滑鼠游標是否位於UI上方
        /// </summary>
        /// <returns></returns>
        public static bool IsPointerOverUIObject() {
            // Referencing this code for GraphicRaycaster https://gist.github.com/stramit/ead7ca1f432f3c0f181f
            // the ray cast appears to require only eventData.position.
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            return (results.Count > 0);
        }

        /// <summary>
        /// 判斷特定點是否位於UI上方
        /// </summary>
        /// <param name="canvas">目標畫布</param>
        /// <param name="screenPoint">待判定點</param>
        /// <returns></returns>
        public static bool IsPointOverUIObject(Canvas canvas, Vector2 screenPoint) {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = screenPoint;

            GraphicRaycaster uiRaycaster = canvas.GetComponent<GraphicRaycaster>();
            List<RaycastResult> results = new List<RaycastResult>();
            uiRaycaster.Raycast(eventDataCurrentPosition, results);

            return (results.Count > 0);
        }

        /// <summary>
        /// 將世界座標轉換為畫布座標。
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="worldPos"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static Vector2 WorldPosToCanvas(Canvas canvas, Vector3 worldPos, Camera camera = null) {
            if (!camera) camera = Camera.main;

            Vector3 viewportPos = camera.WorldToViewportPoint(worldPos);
            RectTransform canvasRect = canvas.transform as RectTransform;

            return new Vector2(viewportPos.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
                               viewportPos.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);
        }

    }

}
