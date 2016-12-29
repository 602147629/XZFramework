/*
 * FileName:    GameManagerEditor
 * Author:      熊哲
 * CreateTime:  12/27/2016 4:34:07 PM
 * Description:
 * 
*/
using UnityEditor;

namespace XZFramework
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}