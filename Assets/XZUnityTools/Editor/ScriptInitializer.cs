/*
 * FileName:    ScriptInitializer
 * Author:      熊哲
 * CreateTime:  2016/08/08 10:49
 * Description:
 * 用于初始化脚本文件，使用前请先修改Unity的脚本模板
 * 文件请放置于Editor文件夹下
*/
using System.IO;

namespace XZUnityTools
{
    public class ScriptInitializer : UnityEditor.AssetModificationProcessor
    {
        public static void OnWillCreateAsset(string path)
        {
            path = path.Replace(".meta", "");
            if (path.ToLower().EndsWith(".cs") || path.ToLower().EndsWith(".lua"))
            {
                string content = File.ReadAllText(path);

                content = content.Replace("#AUTHORNAME#", "熊哲");
                content = content.Replace("#CREATETIME#", System.DateTime.Now.ToString());

                File.WriteAllText(path, content);
            }
        }
    }
}