/*
 * FileName:    AssetBundleBuilder
 * Author:      熊哲
 * CreateTime:  12/26/2016 4:28:25 PM
 * Description:
 * 
*/
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XZFramework
{
    public class AssetBundleBuilder
    {
        [MenuItem(MenuList.BuildAssetBundleAndroid.name, priority = MenuList.BuildAssetBundleAndroid.priority)]
        public static void BuildAssetBundleAndroid() { BuildAssetBundle(BuildTarget.Android); }
        [MenuItem(MenuList.BuildAssetBundleIOS.name, priority = MenuList.BuildAssetBundleIOS.priority)]
        public static void BuildAssetBundleIOS() { BuildAssetBundle(BuildTarget.iOS); }
        [MenuItem(MenuList.BuildAssetBundleWindows.name, priority = MenuList.BuildAssetBundleWindows.priority)]
        public static void BuildAssetBundleWindows() { BuildAssetBundle(BuildTarget.StandaloneWindows); }

        [MenuItem(MenuList.BuildBaseBundleAndroid.name, priority = MenuList.BuildBaseBundleAndroid.priority)]
        public static void BuildBaseBundleAndroid() { BuildBaseBundle(BuildTarget.Android); }
        [MenuItem(MenuList.BuildBaseBundleIOS.name, priority = MenuList.BuildBaseBundleIOS.priority)]
        public static void BuildBaseBundleIOS() { BuildBaseBundle(BuildTarget.iOS); }
        [MenuItem(MenuList.BuildBaseBundleWindows.name, priority = MenuList.BuildBaseBundleWindows.priority)]
        public static void BuildBaseBundleWindows() { BuildBaseBundle(BuildTarget.StandaloneWindows); }

        protected const char DELIMITER = '|';

        protected static Settings settings = XZEditorUtility.LoadScriptableAsset<Settings>(Settings.AssetName);
        
        protected static string bundleDirPath = Application.dataPath + "/" + settings.BundleDirName + "/";
        protected static string luaDirPath = bundleDirPath + settings.LuaDirName + "/";

        protected static string bundleListFileName = "files.txt";
        protected static string bundleExtension = settings.BundleExtension;

        protected static List<AssetBundleBuild> buildList = new List<AssetBundleBuild>();
        protected static List<string> fileList = new List<string>();

        /// <summary>
        /// Build指定平台的基础资源
        /// </summary>
        /// <param name="target"></param>
        protected static void BuildBaseBundle(BuildTarget target)
        {
            if (Directory.Exists(bundleDirPath)) Directory.Delete(bundleDirPath, true);
            Directory.CreateDirectory(bundleDirPath);
            AssetDatabase.Refresh();
            buildList.Clear();

            HandleLuaFile();

            CreateBaseBuildList();

            BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle |
                BuildAssetBundleOptions.UncompressedAssetBundle;
            BuildPipeline.BuildAssetBundles(bundleDirPath, buildList.ToArray(), options, target);

            BuildFileIndex();
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 在这里添加需要Build的基础资源
        /// </summary>
        protected static void CreateBaseBuildList()
        {

        }

        /// <summary>
        /// Build指定平台的资源
        /// </summary>
        /// <param name="target"></param>
        protected static void BuildAssetBundle(BuildTarget target)
        {
            if (Directory.Exists(bundleDirPath)) Directory.Delete(bundleDirPath, true);
            Directory.CreateDirectory(bundleDirPath);
            AssetDatabase.Refresh();
            buildList.Clear();

            HandleLuaFile();

            CreateBuildList();

            BuildAssetBundleOptions options = BuildAssetBundleOptions.DeterministicAssetBundle |
                BuildAssetBundleOptions.UncompressedAssetBundle;
            BuildPipeline.BuildAssetBundles(bundleDirPath, buildList.ToArray(), options, target);

            BuildFileIndex();
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 在这里添加所有需要Build的资源
        /// </summary>
        protected static void CreateBuildList()
        {
            CreateBaseBuildList();
        }

        /// <summary>
        /// 添加一个Build任务
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="pattern"></param>
        /// <param name="path"></param>
        protected static void AddBuildList(string bundleName, string pattern, string path)
        {
            string[] files = Directory.GetFiles(path, pattern);
            if (files.Length == 0)
            {
                return;
            }
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace('\\', '/');
            }
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = bundleName + bundleExtension;
            build.assetNames = files;
            buildList.Add(build);
        }
        /// <summary>
        /// 复制所有的Lua文件
        /// </summary>
        protected static void HandleLuaFile()
        {
            if (!Directory.Exists(luaDirPath))
            {
                Directory.CreateDirectory(luaDirPath);
            }
            string[] luaSources =
            {
                Application.dataPath + "/Lua/",
                Application.dataPath + "/ToLua/Lua/",
            };
            for (int i = 0; i < luaSources.Length; i++)
            {
                fileList.Clear();
                DirTraverse(luaSources[i]);
                int n = 0;
                foreach (string filePath in fileList)
                {
                    if (filePath.EndsWith(".meta")) continue;
                    string newPath = luaDirPath + filePath.Replace(luaSources[i], "");
                    string newDir = Path.GetDirectoryName(newPath);
                    if (!Directory.Exists(newDir)) Directory.CreateDirectory(newDir);
                    if (File.Exists(newPath)) File.Delete(newPath);
                    File.Copy(filePath, newPath, true);
                    XZEditorUtility.ShowProgress(newPath, n++, fileList.Count);
                }
            }
            XZEditorUtility.CloseProgress();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 遍历所有文件，记录文件的相对路径和MD5值
        /// </summary>
        protected static void BuildFileIndex()
        {
            string bundleListFilePath = bundleDirPath + bundleListFileName;
            if (File.Exists(bundleListFilePath))
            {
                File.Delete(bundleListFilePath);
            }

            fileList.Clear();
            DirTraverse(bundleDirPath);

            FileStream fileStream = new FileStream(bundleListFilePath, FileMode.CreateNew);
            StreamWriter streamWriter = new StreamWriter(fileStream);
            foreach (string filePath in fileList)
            {
                if (filePath.EndsWith(".meta") || filePath.Contains(".DS_Stroe")) continue;
                string md5Data = Utility.MD5File(filePath);
                string relativePath = filePath.Replace(bundleDirPath, string.Empty);
                streamWriter.WriteLine(relativePath + DELIMITER + md5Data);
            }
            streamWriter.Close();
            fileStream.Close();
        }
        /// <summary>
        /// 遍历一个目录，把文件添加到文件列表
        /// </summary>
        /// <param name="dirPath"></param>
        protected static void DirTraverse(string dirPath)
        {
            if (!Directory.Exists(dirPath)) return;
            string[] files = Directory.GetFiles(dirPath);
            string[] dirs = Directory.GetDirectories(dirPath);
            foreach (string file in files)
            {
                string extension = Path.GetExtension(file);
                if (extension.Equals(".meta")) continue;
                fileList.Add(file.Replace('\\', '/'));
            }
            foreach (string dir in dirs)
            {
                DirTraverse(dir);
            }
        }
    }
}