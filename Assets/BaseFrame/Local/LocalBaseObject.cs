using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FireCubeBase
{
    public class LocalBaseObject : MonoBehaviour
    {
        public event Action<string> SetStepEvent;


        /// <summary>
        /// 同步步骤到每个客户端
        /// </summary>
        /// <param name="step"></param>
        public void SetStep(string step)
        {
            SetStepEvent?.Invoke(step);
        }



        /// <summary>
        /// 接收服务器发送过来的步骤
        /// </summary>
        /// <param name="step"></param>
        public void SetStepNet(string step)
        {

        }
    }
}
