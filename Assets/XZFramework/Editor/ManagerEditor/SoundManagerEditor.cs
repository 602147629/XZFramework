/*
 * FileName:    SoundManagerEditor
 * Author:      熊哲
 * CreateTime:  12/26/2016 4:54:18 PM
 * Description:
 * 
*/
using UnityEditor;
using UnityEngine;

namespace XZFramework
{
    [CustomEditor(typeof(SoundManager))]
    public class SoundManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SoundManager soundManager = target as SoundManager;
            soundManager.SpatialBlend = EditorGUILayout.Slider("Global Spatial Blend", soundManager.SpatialBlend, 0, 1);
            
            soundManager.BgmActive = EditorGUILayout.Toggle("BGM", soundManager.BgmActive);
            if (soundManager.BgmActive)
                soundManager.BgmVolume = EditorGUILayout.Slider(soundManager.BgmVolume, 0, 1);
            
            soundManager.EfxActive = EditorGUILayout.Toggle("EFX", soundManager.EfxActive);
            if (soundManager.EfxActive)
                soundManager.EfxVolume = EditorGUILayout.Slider(soundManager.EfxVolume, 0, 1);
        }
    }
}