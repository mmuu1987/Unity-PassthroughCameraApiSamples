using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FireCubeBase
{
    /// <summary>
    /// openCV识别到图片后的一些处理
    /// 
    /// </summary>
    public class OpenCVAnchor : MonoBehaviour
    {


        public event Action<Transform,bool> TrackSuccessEvent;
        private float timeTemp = 0f;


        private bool isTrackSuccess = false;

        private void Update()
        {

#if UNITY_ANDROID && !UNITY_EDITOR_WIN

            if (!isTrackSuccess)
            {

                if (timeTemp >= 2f)
                {
                    Debug.Log($"识别成功");
                    isTrackSuccess = true;
                    TrackSuccessEvent?.Invoke(transform,true);
                    timeTemp = 0f;
                    this.gameObject.SetActive(false);//隐藏自身
                }
                else
                {
                    timeTemp += Time.deltaTime;

                }
            }
#else

            Debug.Log($"Pc端的无需识别，直接传输原地000的变换");
         
            isTrackSuccess = true;
            TrackSuccessEvent?.Invoke(transform,false);
            timeTemp = 0f;
            this.gameObject.SetActive(false);
#endif


        }

        public void Reset()
        {
            isTrackSuccess = false;
            this.gameObject.SetActive(true);
            this.transform.position = Vector3.zero;
        }
        /// <summary>
        /// 识别后调用
        /// </summary>
        private void OnEnable()
        {
            Debug.Log($"onEnable:{this.GetType().Name}");
            timeTemp = 0f;
        }

        /// <summary>
        /// 失去识别后调用
        /// </summary>
        private void OnDisable()
        {
            timeTemp = 0f;
        }
    }
}
