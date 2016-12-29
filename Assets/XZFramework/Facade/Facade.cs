/*
 * FileName:    Facade
 * Author:      熊哲
 * CreateTime:  12/22/2016 2:03:47 PM
 * Description:
 * 
*/
using UnityEngine;

namespace XZFramework
{
    public class Facade : MonoBehaviour
    {
        private static Facade instance;
        public static Facade Instance
        {
            get
            {
                // 判断当前场景中是否已有管理器，没有则新建一个管理器
                if (instance == null)
                {
                    instance = FindObjectOfType<Facade>(); // 指定HideAndDontSave之后就无效了
                    if (instance == null)
                    {
                        GameObject gameObj = new GameObject("Facade");
                        instance = gameObj.AddComponent<Facade>();
                    }
                    DontDestroyOnLoad(instance.gameObject);
                    instance.gameObject.hideFlags = instance.hideFacade ? HideFlags.HideAndDontSave : HideFlags.NotEditable | HideFlags.DontSave;
                    Debug.LogWarning("Facade Start.");
                }
                return instance;
            }
        }

        [SerializeField]
        private bool hideFacade;

        public Settings settings { get; private set; }

        /// <summary>
        /// 框架的开始命令
        /// </summary>
        /// <param name="hideFacade"></param>
        public void StartFacade()
        {
            if (!Commands.StartFacade) { return; }
            Commands.StartFacade = false;

            Instance.settings = Resources.Load<Settings>(Settings.AssetName);
            GameManager.Instance.Initialize();
            DownloadManager.Instance.Initialize();
            ResourceManager.Instance.Initialize();
            DatabaseManager.Instance.Initialize();
            UIManager.Instance.Initialize();
            SoundManager.Instance.Initialize();
            StartApplication();
        }
        /// <summary>
        /// 程序的开始命令
        /// </summary>
        public void StartApplication()
        {
            if (!Commands.StartApplication) { return; }
            Commands.StartApplication = false;
        }
        /// <summary>
        /// 程序的退出命令
        /// </summary>
        public void Exit()
        {
            if (!Commands.Exit) { return; }
            Commands.Exit = false;

            PlayerPrefs.Save();
        }

        #region MonoBehaviour Message
        void Awake()
        {
            if (instance != null) { Destroy(this); }
            StartFacade();
        }
        void OnApplicationFocus()
        {

        }
        void OnApplicationPause()
        {

        }
        void OnApplicationQuit()
        {
            Exit();
        }
        #endregion

        private static class Commands
        {
            private static bool startFacade = true;
            private static bool startApplication;
            private static bool exit;

            public static bool StartFacade
            {
                get { return startFacade; }
                set
                {
                    startFacade = false;
                    startApplication = true;
                }
            }
            public static bool StartApplication
            {
                get { return startApplication; }
                set
                {
                    startApplication = false;
                    exit = true;
                }
            }
            public static bool Exit
            {
                get { return exit; }
                set { exit = false; }
            }
        }
    }
}