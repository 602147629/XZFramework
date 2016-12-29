/*
 * FileName:    MenuList
 * Author:      熊哲
 * CreateTime:  12/26/2016 5:25:19 PM
 * Description:
 * 
*/
namespace XZFramework
{
    public class MenuList
    {
        const int MENUGROUPSIZE = 20;

        #region 0.Settings
        public struct Settings
        {
            public const string name = "XZFramework/Settings";
            public const int priority = MENUGROUPSIZE * 0 + 1;
        }
        #endregion

        #region 2.BuildAssetBundle
        public struct BuildAssetBundleWindows
        {
            public const string name = "XZFramework/Build Asset Bundle Windows";
            public const int priority = MENUGROUPSIZE * 2 + 1;
        }
        public struct BuildAssetBundleAndroid
        {
            public const string name = "XZFramework/Build Asset Bundle Android";
            public const int priority = MENUGROUPSIZE * 2 + 2;
        }
        public struct BuildAssetBundleIOS
        {
            public const string name = "XZFramework/Build Asset Bundle IOS";
            public const int priority = MENUGROUPSIZE * 2 + 3;
        }
        public struct BuildBaseBundleWindows
        {
            public const string name = "XZFramework/Build Base Bundle Windows";
            public const int priority = MENUGROUPSIZE * 2 + 11;
        }
        public struct BuildBaseBundleAndroid
        {
            public const string name = "XZFramework/Build Base Bundle Android";
            public const int priority = MENUGROUPSIZE * 2 + 12;
        }
        public struct BuildBaseBundleIOS
        {
            public const string name = "XZFramework/Build Base Bundle IOS";
            public const int priority = MENUGROUPSIZE * 2 + 13;
        }
        #endregion
    }
}