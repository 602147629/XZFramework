/*
 * FileName:    XZEditorUtility
 * Author:      熊哲
 * CreateTime:  11/28/2016 12:02:02 PM
 * Description:
 * 
*/
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XZFramework
{
    public class XZEditorUtility
    {
        /// <summary>
        /// 自定义Asset的存放路径（相对于工程路径）
        /// </summary>
        private static string ADBPath = "Assets/Resources/";

        /// <summary>
        /// 构建一个文本的GUIStyle
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <returns></returns>
        public static GUIStyle NewFontStyle(int fontSize = 11, FontStyle fontStyle = FontStyle.Normal)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = fontSize;
            style.fontStyle = fontStyle;
            return style;
        }
        /// <summary>
        /// 构建一个文本的GUIStyle
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="fontStyle"></param>
        /// <returns></returns>
        public static GUIStyle NewFontStyle(FontStyle fontStyle = FontStyle.Normal)
        {
            return NewFontStyle(11, fontStyle);
        }

        /// <summary>
        /// 加载一个ScriptableAsset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="adbFilePath"></param>
        /// <param name="createDefault"></param>
        /// <returns></returns>
        public static T LoadScriptableAsset<T>(string adbFileName, bool createDefault = true) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(GetADBPath(adbFileName));
            if (asset == null && createDefault)
            {
                CreateScriptableAsset<T>(adbFileName);
                asset = AssetDatabase.LoadAssetAtPath<T>(GetADBPath(adbFileName));
            }
            return asset;
        }
        /// <summary>
        /// 创建一个ScriptableAsset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="adbFilePath"></param>
        public static void CreateScriptableAsset<T>(string adbFileName, T obj = null) where T : ScriptableObject
        {
            if (obj == null) obj = ScriptableObject.CreateInstance<T>();
            if (!Directory.Exists(ADBPath)) { Directory.CreateDirectory(ADBPath); }
            AssetDatabase.CreateAsset(obj, GetADBPath(adbFileName));
        }
        /// <summary>
        /// 根据Asset的名称得到其的路径（相对于工程路径）
        /// </summary>
        /// <param name="adbFileName"></param>
        /// <returns></returns>
        public static string GetADBPath(string adbFileName)
        {
            return ADBPath + adbFileName + ".asset";
        }

        /// <summary>
        /// 显示进度条
        /// </summary>
        /// <param name="info"></param>
        /// <param name="current"></param>
        /// <param name="amount"></param>
        public static void ShowProgress(string info, int current, int amount)
        {
            string title = "Processing... [" + current + " - " + amount + "]";
            float progress = (float)current / amount;
            EditorUtility.DisplayProgressBar(title, info, progress);
        }
        /// <summary>
        /// 清除进度条
        /// </summary>
        public static void CloseProgress()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}