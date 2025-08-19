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


        private bool _isTracking  = false;

        private void Update()
        {

#if UNITY_ANDROID && !UNITY_EDITOR_WIN


            if (_isTracking && !gameObject.activeInHierarchy)
            {
                _isTracking = false;
            }

            if (!_isTracking && gameObject.activeInHierarchy)
            {
                _isTracking = true;
            }


            if (_isTracking)
            {

                if (timeTemp >= 0.1f)
                {
                    Debug.Log($"识别成功");
                    _isTracking = false;
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
         
            _isTracking = false;
            TrackSuccessEvent?.Invoke(transform,false);
            timeTemp = 0f;
            this.gameObject.SetActive(false);
#endif


        }

        public void CheckTracking(bool isTracking)
        {
           
            timeTemp = 0f;
            this.gameObject.SetActive(isTracking);
            _isTracking = isTracking;

        }
        public void Reset()
        {
            timeTemp = 0f;
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


#if UNITY_EDITOR_WIN
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(0f, 200f, 100f, 100f), "anchor"))
        //    {

        //    }
        //}
#endif
    }
}
