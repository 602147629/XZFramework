/*
 * FileName:    DatabaseManagerEditor
 * Author:      熊哲
 * CreateTime:  12/27/2016 3:52:49 PM
 * Description:
 * 
*/
using UnityEditor;

namespace XZFramework
{
    [CustomEditor(typeof(DatabaseManager))]
    public class DatabaseManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DatabaseManager databaseManager = target as DatabaseManager;
            databaseManager.cacheSize = EditorGUILayout.IntSlider("Cache Size", databaseManager.cacheSize, 10, 1000);
        }
    }
}