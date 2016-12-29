/*
 * FileName:    SettingsEditor
 * Author:      熊哲
 * CreateTime:  12/22/2016 2:13:31 PM
 * Description:
 * 
*/
using UnityEditor;
using UnityEngine;

namespace XZFramework
{
    public class SettingsEditor : EditorWindow
    {
        [MenuItem(MenuList.Settings.name, priority = MenuList.Settings.priority)]
        static void Init()
        {
            SettingsEditor window = GetWindow<SettingsEditor>("Settings");
            window.Show();
        }

        private static GUIStyle GUIStyle_Title = XZEditorUtility.NewFontStyle(12);
        private static GUIStyle GUIStyle_SectionTitle = XZEditorUtility.NewFontStyle(FontStyle.Bold);

        Settings settings;

        private enum SleepTimeout
        {
            NeverSleep = UnityEngine.SleepTimeout.NeverSleep,
            SystemSetting = UnityEngine.SleepTimeout.SystemSetting,
        }

        void OnHierarchyChange() { Repaint(); }

        void OnGUI()
        {
            settings = XZEditorUtility.LoadScriptableAsset<Settings>(Settings.AssetName);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("XZFramework Settings", GUIStyle_Title);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quality", GUIStyle_SectionTitle);
            settings.FrameRate = EditorGUILayout.IntSlider("Frame Rate", settings.FrameRate, 30, 120);
            settings.SleepTimeout = (int)(SleepTimeout)EditorGUILayout.EnumPopup("Sleep Timeout", (SleepTimeout)settings.SleepTimeout);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mode", GUIStyle_SectionTitle);
            settings.DebugMode = EditorGUILayout.Toggle("Debug Mode", settings.DebugMode);
            settings.UpdateMode = EditorGUILayout.Toggle("Update Mode", settings.UpdateMode);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Network", GUIStyle_SectionTitle);
            EditorGUILayout.PrefixLabel("Server URL");
            settings.ServerUrl = EditorGUILayout.TextField(settings.ServerUrl);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Asset Bundle", GUIStyle_SectionTitle);
            settings.BundleDirName = EditorGUILayout.TextField("Dir Name", settings.BundleDirName);
            settings.BundleExtension = EditorGUILayout.TextField("Extension", settings.BundleExtension);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Lua", GUIStyle_SectionTitle);
            settings.LuaDirName = EditorGUILayout.TextField("Dir Name", settings.LuaDirName);

            if (GUI.changed) EditorUtility.SetDirty(settings);
        }
    }
}