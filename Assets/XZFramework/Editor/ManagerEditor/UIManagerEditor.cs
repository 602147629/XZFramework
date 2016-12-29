/*
 * FileName:    UIManagerEditor
 * Author:      熊哲
 * CreateTime:  12/27/2016 4:35:27 PM
 * Description:
 * 
*/
using UnityEditor;

namespace XZFramework
{
    [CustomEditor(typeof(UIManager))]
    public class UIManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}