/*
 * FileName:    Settings
 * Author:      熊哲
 * CreateTime:  12/22/2016 2:08:09 PM
 * Description:
 * 
*/
using UnityEngine;

namespace XZFramework
{
    public class Settings : ScriptableObject
    {
        public const string AssetName = "XZFrameworkSettings";

        public int FrameRate = 60;
        public int SleepTimeout = UnityEngine.SleepTimeout.NeverSleep;

        public bool DebugMode = true;
        public bool UpdateMode = false;
        
        public string ServerUrl = "http://192.168.31.64/";
        
        public string BundleDirName = "StreamingAssets";
        public string BundleExtension = ".unity3d";

        public string LuaDirName = "Lua";
    }
}