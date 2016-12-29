/*
 * FileName:    ResourceManagerEditor
 * Author:      熊哲
 * CreateTime:  12/27/2016 4:45:11 PM
 * Description:
 * 
*/
using UnityEditor;

namespace XZFramework
{
    [CustomEditor(typeof(ResourceManager))]
    public class ResourceManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}