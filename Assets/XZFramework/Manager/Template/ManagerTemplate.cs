/*
 * FileName:    ManagerTemplate
 * Author:      熊哲
 * CreateTime:  11/21/2016 5:14:45 PM
 * Description:
 * 
*/
using UnityEngine;

namespace XZFramework
{
    public abstract class ManagerTemplate<T> : MonoBehaviour
        where T :Component
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                    if (instance == null)
                    {
                        GameObject gameObj = new GameObject(typeof(T).Name);
                        instance = gameObj.AddComponent<T>();
                        gameObj.transform.SetParent(Facade.Instance.transform);
                    }
                    instance.gameObject.hideFlags = Facade.Instance.gameObject.hideFlags;
                    Debug.LogWarning("Manager: " + typeof(T).Name + " Start.");
                }
                return instance;
            }
        }

        /// <summary>
        /// 管理器初始化，不用MonoBehaviour的生命周期是为了方便控制管理器的加载顺序
        /// （比如热更新之后才能加载ResourceManager)
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 管理器开始时输出提示
        /// </summary>
        protected virtual void Awake()
        {
            if (instance != null) { Destroy(this); }
        }
    }
}