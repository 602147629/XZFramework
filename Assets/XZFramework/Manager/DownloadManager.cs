/*
 * FileName:    DownloadManager
 * Author:      熊哲
 * CreateTime:  12/27/2016 2:16:43 PM
 * Description:
 * 
*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace XZFramework
{
    public class DownloadManager : ManagerTemplate<DownloadManager>
    {
        /// <summary>
        /// 最大线程数量
        /// </summary>
        public int MaxThread = 5;
        /// <summary>
        /// 线程表：记录当前正在运行的线程
        /// </summary>
        private Dictionary<string, Thread> threadDict = new Dictionary<string, Thread>();

        /// <summary>
        /// 回调表：记录所有下载的回调方法
        /// </summary>
        private Dictionary<WebClient, Action<string>> callbackDict = new Dictionary<WebClient, Action<string>>();
        /// <summary>
        /// 回调参数表：记录所有下载的回调方法需要的参数
        /// </summary>
        private Dictionary<WebClient, string> callbackArgDict = new Dictionary<WebClient, string>();

        /// <summary>
        /// 正在排队的任务，当一个线程完成时从该队列中取任务
        /// </summary>
        private Queue<List<object>> taskQueue = new Queue<List<object>>();

        public override void Initialize()
        {

        }

        /// <summary>
        /// 开启线程下载文件到本地，需要传入回调函数，回调时会传入本地路径
        /// </summary>
        /// <param name="urlPath"></param>
        /// <param name="localPath"></param>
        /// <param name="callback"></param>
        public void DownLoad(string urlPath, string localPath, Action<string> callback)
        {
            List<object> args = new List<object> { urlPath, localPath, callback };
            lock (threadDict)
            {
                if (threadDict.Count >= MaxThread)
                {
                    lock (taskQueue) { taskQueue.Enqueue(args); }
                }
                else
                {
                    Thread t = new Thread(DownloadThread);
                    threadDict.Add(localPath, t);
                    t.Start(args);
                }
            }
        }
        /// <summary>
        /// 使用DownloadClient进行下载，并记录回调方法到回调表；
        /// </summary>
        /// <param name="args"></param>
        private void DownloadThread(object args)
        {
            List<object> argList = (List<object>)args;
            string urlPath = argList[0].ToString();
            string localPath = argList[1].ToString();
            Action<string> callback = (Action<string>)argList[2];

            using (WebClient client = new WebClient())
            {
                callbackDict.Add(client, callback);
                callbackArgDict.Add(client, localPath);
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgressChanged);
                client.DownloadFileAsync(new Uri(urlPath), localPath);
            }
        }
        /// <summary>
        /// 下载完成后的回调，在该回调中回调回调表中的方法，然后从回调表中删除，检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100 && e.BytesReceived == e.TotalBytesToReceive)
            {
                WebClient client = (WebClient)sender;

                Action<string> callback = callbackDict[client]; string arg = callbackArgDict[client];
                callback(arg);
                lock (threadDict)
                {
                    threadDict.Remove(arg);
                    callbackDict.Remove(client);
                    callbackArgDict.Remove(client);
                }
                lock (taskQueue)
                {
                    if (taskQueue.Count > 0)
                    {
                        List<object> args = taskQueue.Dequeue();
                        Thread t = new Thread(DownloadThread);
                        lock (threadDict) { threadDict.Add(args[1].ToString(), t); }
                        t.Start(args);
                    }
                }
            }
        }

        /// <summary>
        /// 退出时一定要销毁所有的线程
        /// </summary>
        void OnApplicationQuit()
        {
            foreach (var thread in threadDict)
            {
                thread.Value.Abort();
            }
        }
    }
}