/*
 * FileName:    SettingsInspector
 * Author:      熊哲
 * CreateTime:  12/26/2016 1:45:05 PM
 * Description:
 * 
*/
using UnityEditor;
using UnityEngine;

namespace XZFramework
{
    [CustomEditor(typeof(Settings))]
    public class SettingsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            DrawDefaultInspector();
            GUI.enabled = true;
        }
    }
}