using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace FireCubeBase
{
    /// <summary>
    /// 权限方面的控制，公有物品的基类
    /// </summary>
    public class BaseNetCommon : BaseNet
    {

        /// <summary>
        /// 当该物体在客户端活得授权时调用
        /// </summary>
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
        }

        /// <summary>
        /// 当该物体在客户端停止授权时调用
        /// </summary>
        public override void OnStopAuthority()
        {
            base.OnStopAuthority();
        }


    }
}

