/*
 * FileName:    ResourceManager
 * Author:      熊哲
 * CreateTime:  11/22/2016 2:38:16 PM
 * Description:
 * 
*/
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if USING_LUA
using LuaInterface;
#endif

namespace XZFramework
{
    public class ResourceManager : ManagerTemplate<ResourceManager>
    {
        /// <summary>
        /// 资源包的变体后缀名（优先级由高到低）
        /// </summary>
        public string[] variantExtensions = { };

        protected string BundleDirPath;
        protected string BundleExtension;
        /// <summary>
        /// 记录已加载过的资源包
        /// </summary>
        protected Dictionary<string, AssetBundle> bundleDict = new Dictionary<string, AssetBundle>();
        /// <summary>
        /// 资源清单（StreamingAssets的所有依赖）
        /// </summary>
        protected AssetBundleManifest manifest;

        /// <summary>
        /// 初始化资源管理器，读取StreamingAssets的所有依赖（即所有的资源包名）
        /// </summary>
        public override void Initialize()
        {
            BundleDirPath = Facade.Instance.settings.DebugMode
                            ? (Utility.dataDirPath + Facade.Instance.settings.BundleDirName + "/")
                            : (Utility.persistentDirPath);
            BundleExtension = Facade.Instance.settings.BundleExtension;
            string resourceInfoFilePath = BundleDirPath + "StreamingAssets";
            if (!File.Exists(resourceInfoFilePath)) return;
            AssetBundle bundle = AssetBundle.LoadFromFile(resourceInfoFilePath);
            manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        /// <summary>
        /// 加载资源包，返回资源，不存在会返回空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T LoadAsset<T>(string bundleName, string assetName) where T : Object
        {
            bundleName = bundleName.ToLower();
            AssetBundle bundle = LoadAssetBundle(bundleName);
            return bundle.LoadAsset<T>(assetName);
        }
        /// <summary>
        /// 加载资源包，返回资源数组（泛型数组）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public T[] LoadAssetArray<T>(string bundleName, string[] assetNames) where T : Object
        {
            bundleName = bundleName.ToLower();
            List<T> objects = new List<T>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                T o = LoadAsset<T>(bundleName, assetNames[i]);
                if (o != null) objects.Add(o);
            }
            return objects.ToArray();
        }
        /// <summary>
        /// 加载资源包，返回资源字典（泛型字典）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public Dictionary<string, T> LoadAssetDictionary<T>(string bundleName, string[] assetNames) where T : Object
        {
            bundleName = bundleName.ToLower();
            Dictionary<string, T> objects = new Dictionary<string, T>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                T o = LoadAsset<T>(bundleName, assetNames[i]);
                if (o != null) objects.Add(assetNames[i], o);
            }
            return objects;
        }

        /// <summary>
        /// 卸载资源包，布尔参数不太理解，默认为false
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="unloadAll"></param>
        public void UnloadBundle(string bundleName, bool unloadAll = false)
        {
            AssetBundle bundle;
            if (bundleDict.TryGetValue(bundleName, out bundle))
            {
                bundle.Unload(unloadAll);
                bundleDict.Remove(bundleName);
            }
        }

        /// <summary>
        /// 加载资源包及其依赖
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string bundleName)
        {
            if (!bundleName.EndsWith(BundleExtension))
            {
                bundleName += BundleExtension;
            }
            AssetBundle bundle;
            if (bundleDict.TryGetValue(bundleName, out bundle)) { }
            else
            {
                string bundlePath = BundleDirPath + bundleName;
                LoadBundleDependencies(bundleName);
                bundle = AssetBundle.LoadFromFile(bundlePath);
                bundleDict.Add(bundleName, bundle);
            }
            return bundle;
        }
        /// <summary>
        /// 加载资源包依赖
        /// </summary>
        /// <param name="bundleName"></param>
        protected void LoadBundleDependencies(string bundleName)
        {
            if (manifest == null)
            {
                Debug.LogError("AssetBundleManifest is null!");
            }
            string[] dependencies = manifest.GetAllDependencies(bundleName);
            if (dependencies.Length == 0) return;
            // 加载所有的依赖（如果有变体会加载依赖的变体）
            for (int i = 0; i < dependencies.Length; i++)
            {
                dependencies[i] = RemapBundleVariant(dependencies[i]);
                LoadAssetBundle(dependencies[i]);
            }
        }
        /// <summary>
        /// 获取资源包的最优变体
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        protected string RemapBundleVariant(string bundleName)
        {
            string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();
            if (System.Array.IndexOf(bundlesWithVariant, bundleName) < 0) return bundleName;

            string[] bundleOrigin = bundleName.Split('.');
            int bundleIndex = -1;
            int variantIndex = int.MaxValue;
            for (int i = 0; i < bundlesWithVariant.Length; i++)
            {
                string[] bundleVariant = bundlesWithVariant[i].Split('.');
                if (bundleVariant[0] == bundleOrigin[0])
                {
                    int found = System.Array.IndexOf(variantExtensions, bundleVariant[1]);
                    if (found != -1 && found < variantIndex)
                    {
                        bundleIndex = i;
                        variantIndex = found;
                    }
                }
            }
            if (bundleIndex != -1)
                return bundlesWithVariant[bundleIndex];
            else
                return bundleName;
        }

#if USING_LUA
        /// <summary>
        /// 加载预制体，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public void LoadPrefab(string bundleName, string assetName, LuaFunction func)
        {
            func.Call(LoadAsset<GameObject>(bundleName, assetName));
        }
        /// <summary>
        /// 加载预制体数组，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public void LoadPrefabArray(string bundleName, string[] assetNames, LuaFunction func)
        {
            func.Call(LoadAssetArray<GameObject>(bundleName, assetNames));
        }
        /// <summary>
        /// 加载预制体字典，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public void LoadPrefabDictionary(string bundleName, string[] assetNames, LuaFunction func)
        {
            func.Call(LoadAssetDictionary<GameObject>(bundleName, assetNames));
        }

        /// <summary>
        /// 加载音频，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public void LoadAudioClip(string bundleName, string assetName, LuaFunction func)
        {
            func.Call(LoadAsset<AudioClip>(bundleName, assetName));
        }
        /// <summary>
        /// 加载音频数组，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public void LoadAudioClipArray(string bundleName, string[] assetNames, LuaFunction func)
        {
            func.Call(LoadAssetArray<AudioClip>(bundleName, assetNames));
        }
        /// <summary>
        /// 加载音频字典，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public void LoadAudioClipDictionary(string bundleName, string[] assetNames, LuaFunction func)
        {
            func.Call(LoadAssetDictionary<AudioClip>(bundleName, assetNames));
        }

        /// <summary>
        /// 加载精灵，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public void LoadSprite(string bundleName, string assetName, LuaFunction func)
        {
            func.Call(LoadAsset<Sprite>(bundleName, assetName));
        }
        /// <summary>
        /// 加载精灵数组，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public void LoadSpriteArray(string bundleName, string[] assetNames, LuaFunction func)
        {
            func.Call(LoadAssetArray<Sprite>(bundleName, assetNames));
        }
        /// <summary>
        /// 加载精灵词典，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public void LoadSpriteDictionary(string bundleName, string[] assetNames, LuaFunction func)
        {
            func.Call(LoadAssetDictionary<Sprite>(bundleName, assetNames));
        }

        /// <summary>
        /// 加载资源，执行回调
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public void LoadAsset(string bundleName, string assetName, LuaFunction func)
        {
            func.Call(LoadAsset<Object>(bundleName, assetName));
        }
        /// <summary>
        /// 加载资源数组，执行回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public void LoadAssetArray(string bundleName, string[] assetNames, LuaFunction func)
        {
            func.Call(LoadAssetArray<Object>(bundleName, assetNames));
        }
        /// <summary>
        /// 加载资源词典，执行回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bundleName"></param>
        /// <param name="assetNames"></param>
        /// <returns></returns>
        public void LoadAssetDictionary(string bundleName, string[] assetNames, LuaFunction func)
        {
            func.Call(LoadAssetDictionary<Object>(bundleName, assetNames));
        }
#endif
    }
}