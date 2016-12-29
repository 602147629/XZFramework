/*
 * FileName:    UIManager
 * Author:      熊哲
 * CreateTime:  11/28/2016 2:25:36 PM
 * Description:
 * 
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if USING_LUA
using LuaInterface;
#endif

namespace XZFramework
{
    public class UIManager : ManagerTemplate<UIManager>
    {
        public GameObject UICanvas;

        protected Dictionary<string, GameObject> panelDict = new Dictionary<string, GameObject>();
        protected LinkedList<string> panelList = new LinkedList<string>();

        public override void Initialize()
        {

        }

        /// <summary>
        /// 生成一个Panel
        /// </summary>
        /// <param name="panelName"></param>
        /// <param name="panelObject"></param>
        /// <returns></returns>
        public GameObject CreatePanel(string panelName, GameObject panelPrefab)
        {
            if (panelList.Contains(panelName))
            {
                return ShowPanel(panelName);
            }
            GameObject panel = Instantiate(panelPrefab);
            panel.transform.SetParent(UICanvas.transform, false);
            panel.transform.SetAsLastSibling();
            panel.name = panelName;
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.localScale = Vector3.one;
            panel.transform.localPosition = Vector3.zero;
            panelDict.Add(panelName, panel);
            panelList.AddFirst(panelName);
            return panel;
        }
        /// <summary>
        /// 置顶显示一个Panel
        /// </summary>
        /// <param name="panelName"></param>
        /// <returns></returns>
        public GameObject ShowPanel(string panelName)
        {
            if (!panelList.Contains(panelName))
            {
                Debug.LogError("The panel you want to show is null, you must create it first: CreatePanel(panelName, panelObject)'");
                return null;
            }
            panelList.Remove(panelName);
            panelList.AddFirst(panelName);
            panelDict[panelName].transform.SetAsLastSibling();
            panelDict[panelName].SetActive(true);
            return panelDict[panelName];
        }
        /// <summary>
        /// 隐藏一个Panel
        /// </summary>
        /// <param name="panelName"></param>
        public void HidePanel(string panelName)
        {
            if (panelList.Contains(panelName))
            {
                panelList.Remove(panelName);
                panelList.AddLast(panelName);
                panelDict[panelName].SetActive(false);
            }
        }
        /// <summary>
        /// 关闭一个Panel
        /// </summary>
        /// <param name="panelName"></param>
        public void ClosePanel(string panelName)
        {
            if (panelList.Contains(panelName))
            {
                Destroy(panelDict[panelName]);
                panelDict.Remove(panelName);
                panelList.Remove(panelName);
            }
        }

        /// <summary>
        /// 为Button控件添加事件
        /// </summary>
        /// <param name="button"></param>
        /// <param name="func"></param>
        public void AddListener(Button button, UnityAction action)
        {
            button.onClick.AddListener(action);
        }
        /// <summary>
        /// 为Toggle控件添加事件
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="func"></param>
        public void AddListener(Toggle toggle, UnityAction<bool> action)
        {
            toggle.onValueChanged.AddListener(action);
        }
        /// <summary>
        /// 为Slider控件添加事件
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="func"></param>
        public void AddListener(Slider slider, UnityAction<float> action)
        {
            slider.onValueChanged.AddListener(action);
        }

        /// <summary>
        /// 移除Button控件的事件绑定
        /// </summary>
        /// <param name="button"></param>
        public void RemoveListener(Button button)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
        }
        /// <summary>
        /// 移除Toggle控件的事件绑定
        /// </summary>
        /// <param name="toggle"></param>
        public void RemoveListener(Toggle toggle)
        {
            if (toggle == null) return;
            toggle.onValueChanged.RemoveAllListeners();
        }
        /// <summary>
        /// 移除Slider控件的事件绑定
        /// </summary>
        /// <param name="slider"></param>
        public void RemoveListener(Slider slider)
        {
            if (slider == null) return;
            slider.onValueChanged.RemoveAllListeners();
        }

#if USING_LUA
        public void CreatePanel(string panelName, string bundleName, string assetName, LuaFunction func)
        {
            GameObject panelPrefab = Instantiate(ResourceManager.Instance.LoadAsset<GameObject>(bundleName, assetName));
            func.Call(CreatePanel(panelName, panelPrefab));
        }
        public void CreatePanel(string panelName, GameObject panelPrefab, LuaFunction func)
        {
            func.Call(CreatePanel(panelName, panelPrefab));
        }
        public void ShowPanel(string panelName, LuaFunction func)
        {
            func.Call(ShowPanel(panelName));
        }

        /// <summary>
        /// 为Button控件添加事件
        /// </summary>
        /// <param name="button"></param>
        /// <param name="func"></param>
        public void AddListener(Button button, LuaFunction func)
        {
            if (button == null || func == null) return;
            button.onClick.AddListener(delegate { func.Call(button); });
        }
        /// <summary>
        /// 为Toggle控件添加事件
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="func"></param>
        public void AddListener(Toggle toggle, LuaFunction func)
        {
            if (toggle == null || func == null) return;
            toggle.onValueChanged.AddListener(delegate { func.Call(toggle); });
        }
        /// <summary>
        /// 为Slider控件添加事件
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="func"></param>
        public void AddListener(Slider slider, LuaFunction func)
        {
            slider.onValueChanged.AddListener(delegate { func.Call(slider); });
        }
#endif
    }
}