/*
 * FileName:    Utility
 * Author:      #AUTHORNAME#
 * CreateTime:  #CREATETIME#
 * Description:
 * 
*/
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace XZFramework
{
    public class Utility
    {
        public static string projectDirPath { get; private set; }
        public static string dataDirPath { get; private set; }
        public static string streamingDirPath { get; private set; }
        public static string persistentDirPath { get; private set; }

        static Utility()
        {
            projectDirPath = PathNormalize(Application.dataPath).Substring(0, Application.dataPath.LastIndexOf("/") + 1);
            dataDirPath = PathNormalize(Application.dataPath) + "/";
            streamingDirPath = PathNormalize(Application.streamingAssetsPath) + "/";
#if UNITY_EDITOR
            persistentDirPath = "C:/UnityPersistent/";
#else
            persistentDirPath = PathNormalize(Application.persistentDataPath) + "/";
#endif
        }

        /// <summary>
        /// 标准化路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string PathNormalize(string path)
        {
            return path.Replace("\\", "/");
        }

        /// <summary>
        /// 获取范围内的随机整数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }
        /// <summary>
        /// 获取范围内的随机浮点数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string MD5String(string content)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] md5Data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(content));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < md5Data.Length; i++)
            {
                sBuilder.Append(md5Data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string MD5File(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
                byte[] md5Data = md5Hasher.ComputeHash(fs);
                StringBuilder sBuilder = new StringBuilder();
                for (int i = 0; i < md5Data.Length; i++)
                {
                    sBuilder.Append(md5Data[i].ToString("x2"));
                }
                fs.Close();
                return sBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("MD5File() fail, error: " + ex.Message);
            }
        }

        /// <summary>
        /// 以1970年为基准，获取毫秒时间差
        /// </summary>
        public static long TimeSpan
        {
            get
            {
                TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
                return (long)ts.TotalMilliseconds;
            }
        }
        /// <summary>
        /// 网络可用返回true，否则返回false
        /// </summary>
        public static bool IsNetAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }
        /// <summary>
        /// 本地连接（有线或无线，非手机网络）返回true，否则返回false
        /// </summary>
        public static bool IsNetLocal
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }
    }
}