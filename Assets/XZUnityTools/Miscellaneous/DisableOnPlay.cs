/*
 * FileName:    DisableOnPlay
 * Author:      熊哲
 * CreateTime:  9/2/2016 11:31:03 AM
 * Description:
 * 挂载到测试对象上，运行时自动隐藏该对象
*/
using UnityEngine;

namespace XZUnityTools
{
    public class DisableOnPlay : MonoBehaviour
    {
        void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}