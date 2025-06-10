using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace FireCubeBase
{
    /// <summary>
    /// 带网络属性的UI基类
    /// UI的位置同步不同于基类的位置同步，基类的位置同步是一个客户端同步到服务器，服务器再同步给观察者客户端 
    /// UI的位置同步是服务器同步给所有的客户端，任何一个客户端都没有权限同步自己的UI到服务器  
    /// </summary>
    public class BaseNetUI : BaseNet
    {

        /// <summary>
       

        public List<EventInfo> EventInfos = new List<EventInfo>();


        /// <summary>
        /// 外部同步变量，主要是用在驱动一些操作上的同步
        /// </summary>
        [SyncVar(hook = nameof(UpdateSyncExternal))]
        public string SyncExternal;

        /// <summary>
        /// 内部同步变量，主要是一些视觉效果上的同步
        /// </summary>
        [SyncVar(hook = nameof(UpdateSyncInterior))]
        public string SyncInterior;

        /// <summary>
        /// 内部同步变量队列
        /// </summary>
        private Queue<string> interiorQueue = new Queue<string>();

        private Queue<string> externalQueue = new Queue<string>();



        private WaitForEndOfFrame waitForEndOfFrame;



        /// <summary>
        /// 是否自动获取权限，仅在服务器端有效
        /// </summary>
        public bool IsAutoGetAuthorityInServer = false;


        #region 双端都可以可能调用的代码


        protected override void OnAwake()
        {
            base.OnAwake();

            StartCoroutine(SendMessage());

            //NumberPlayerImage.gameObject.SetActive(false);
        }

        public virtual void UpdateSyncInterior(string oldValue, string newValue)
        {
            Debug.Log($"{this.GetType()}按键数据提交在isServer:{isServer}  newValue:{newValue}");
        }

        public virtual void UpdateSyncExternal(string oldValue, string newValue)
        {
            Debug.Log($"{this.GetType()}完成选择数据提交在isServer:{isServer}  newValue:{newValue}");
        }


        /// <summary>
        /// 这个值起作用的前提是已经授权
        /// </summary>
        /// <param name="value"></param>
        public void SetSyncInterior(string value)
        {
           
           // this.SyncInterior = value;
           
           //放进队列
            interiorQueue.Enqueue(value);

           // UpdateSyncInterior(null,value);

        }

        /// <summary>
        /// 每2帧末尾更新同步变量，如果数据有变更的话
        /// </summary>
        /// <returns></returns>
        private IEnumerator SendMessage()
        {
            while (true)
            {
                yield return waitForEndOfFrame;
               

                if (externalQueue.Count > 0)
                    this.SyncExternal = externalQueue.Dequeue();

                if (interiorQueue.Count > 0)
                {
                    string value = interiorQueue.Dequeue();

                    if(value.Contains("平板电脑"))Debug.Log($"平板电脑放入同步变量中，上一个同步变量值为：{SyncInterior}" );

                    this.SyncInterior = value;
                }

              
            }
        }
        

       

        public void SetSyncExternal(string step)
        {
           // SyncExternal = step;
            externalQueue.Enqueue(step);
        }


     


        #endregion






        #region 客户端代码


       
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (NetworkClient.localPlayer != null)
            {
                if (!isOwned) //如果已经获得了授权，则不在进行碰撞触发,如果继承类需要多次碰撞的，可以重写该方法,覆盖此方法
                {
                    Debug.Log($"设置碰撞事件");
                    //设置碰撞体触发
                    NetworkClient.localPlayer.GetComponent<BaseNetPlayer>().GetColliderEvent().AddEnterEvent(OnTriggerEnterEvent);
                }
            }

        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            NetworkClient.localPlayer.GetComponent<BaseNetPlayer>().GetColliderEvent().RemoveEnterEvent(OnTriggerEnterEvent);
        }

        public virtual void OnTriggerEnterEvent(Collider otherCollider)
        {

        }

        /// <summary>
        /// 开始授权方法优先执行与onStartClient
        /// </summary>
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();

            Debug.Log($"该客户端获取到了授权");

            //netIdentity.RemoveClientAuthority();
        }


        /// <summary>
        /// 设置客户端的是否自动授权的信息
        /// </summary>
        /// <param name="isAuto"></param>
        [ClientRpc]
        public  void RpcSettAuthorityInfo(bool isAuto)
        {
            Debug.Log($"自动授权触发的  isAuto:{isAuto}");
            OnRpcSettAuthorityInfo(isAuto);
        }


        /// <summary>
        /// 通知客户端的是否自动授权的信息
        /// 如果isAuto是false，则是通过碰撞获取的授权
        /// </summary>
        /// <param name="isAuto"></param>
        public virtual void OnRpcSettAuthorityInfo(bool isAuto)
        {

        }

        public virtual void OnRpcReveiveMessage(string message)
        {

        }
        /// <summary>
        /// 获取权限信息
        /// </summary>
        [ClientRpc]
        public  void RpcColliderEvent(string colliderName)
        {
            Debug.Log($"RpcColliderEvent");
            OnRpcColliderEvent(colliderName);
        }

        /// <summary>
        /// 获取到的权限后的碰撞
        /// </summary>
        /// <param name="colliderName"></param>
        public virtual void OnRpcColliderEvent(string colliderName)
        {

        }
        public override void OnStopAuthority()
        {
            base.OnStopAuthority();

            Debug.Log($"该客户端停止了授权");
        }

        /// <summary>
        /// 获取权限
        /// </summary>
        protected void GetAuthority()
        {
            if (isOwned) return;//如果已经有授权，则不能进行授权
            Debug.Log($"获取权限 {this.GetType()}");
            NetworkClient.localPlayer.GetComponent<BaseNetPlayer>().CmdAssignClientAuthority(this.netIdentity);
        }

        /// <summary>
        /// 一个环节完成的方法
        /// </summary>
        public virtual void CompletedEvent(string eventName)
        {
           
        }

       
      
        #endregion


            /// <summary>
            /// UI不用追踪位置，直接拿到原始值固定就好，所以这里覆盖掉
            /// </summary>
        protected override void GetDir()
        {

        }
        /// <summary>
        /// 覆盖掉基类的 （同步位置 & 旋转），不用基类的算法
        /// </summary>
        protected override void OnUpDate()
        {
            if (isClient)//同步服务器的位置到客户端,启用该方法的前提是UI的位置参考场景的根物体
            {

                if (WorldCenterPoint != null)
                {
                    Transform rootTransform = BaseNetManager.FireCubeNetClient.ClientSceneNet.localBaseObject.transform;//sceneNet的localBaseObject就是场景的根物体

                    this.transform.position = WorldCenterPoint.position + rootTransform.TransformDirection(Pos_Dir);

                    if (Forward == Vector3.zero || Up == Vector3.zero) return;

                    this.transform.rotation = Quaternion.LookRotation(rootTransform.TransformDirection(Forward), rootTransform.TransformDirection(Up));
                }
            }
        }


        #region 服务端代码

        public override void OnStartServer()
        {
            base.OnStartServer();

            //服务器上的世界中心点是000点，所以这里不用API:InverseTransformDirection转换

            Pos_Dir = this.transform.localPosition;//把朝向拿回来,赋值给同步变量，以下同理
            Forward = this.transform.forward;
            Up = this.transform.up;
        }

       
        public virtual void UnSpawn()
        {
            BaseNetManager.FireCubeNetServer.UnSpawnGameObjectServer(this.gameObject);
        }



        /// <summary>
        /// 获取权限信息
        /// </summary>
        [Command]
        public void CmdGetAuthorityInfo()
        {
            RpcSettAuthorityInfo(IsAutoGetAuthorityInServer);
        }


        /// <summary>
        /// 在已经获得权限的状态下，触发了碰撞，传递碰撞信息到服务器
        /// </summary>
        [Command]
        public void CmdColliderEvent(string colliderName)
        {
            RpcColliderEvent(colliderName);
        }
        #endregion


    }

   
}
