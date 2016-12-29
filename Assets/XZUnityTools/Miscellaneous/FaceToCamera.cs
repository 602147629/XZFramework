/*
 * FileName:    FaceToCamera
 * Author:      熊哲
 * CreateTime:  10/13/2016 2:28:55 PM
 * Description:
 * 挂在到物件上，运行时物件会保持面向摄像机
*/
using UnityEngine;

namespace XZUnityTools
{
    public class FaceToCamera : MonoBehaviour
    {
        public Transform cameraObject;

        void Update()
        {
            transform.LookAt(cameraObject);
            transform.Rotate(new Vector3(0, 180, 0));
        }
    }
}