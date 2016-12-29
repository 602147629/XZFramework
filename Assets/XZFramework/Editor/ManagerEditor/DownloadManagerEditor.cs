/*
 * FileName:    DownloadManagerEditor
 * Author:      熊哲
 * CreateTime:  12/27/2016 3:57:26 PM
 * Description:
 * 
*/
using UnityEditor;

namespace XZFramework
{
    [CustomEditor(typeof(DownloadManager))]
    public class DownloadManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DownloadManager downloadManager = target as DownloadManager;
            downloadManager.MaxThread = EditorGUILayout.IntSlider("Max Thread", downloadManager.MaxThread, 1, 10);
        }
    }
}