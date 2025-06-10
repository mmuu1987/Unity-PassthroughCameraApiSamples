
using UnityEngine;
using Mirror;
using XHFrameWork;

namespace FireCubeBase
{
    /// <summary>
    /// 网络上玩家的player 
    /// </summary>

    public class BaseNetPlayer : NetworkBehaviour
    {

        public Transform QuestHeadTransform;

        public Transform QuestLeftTransform;

        public Transform QuestRightTransform;


        public Transform RootHeadTransform;

        public Transform RootQuestLeftTransform;

        public Transform RootQuestRightTransform;


        /// <summary>
        /// 本地的物体，不牵扯网络
        /// </summary>
        public LocalPlayObject localBaseObject { get; private set; }
        /// <summary>
        /// 头部跟参考点的偏移量
        /// </summary>
        [SyncVar]
        public Vector3 HeadDir;

        /// <summary>
        /// 头部的本地的前向向量
        /// </summary>
        [SyncVar]
        public Vector3 HeadForward;
        /// <summary>
        /// 头部的本地的向上向量
        /// </summary>
        [SyncVar]
        public Vector3 HeadUp;

        /// <summary>
        /// 左手跟参考点的偏移量
        /// </summary>
        [SyncVar]
        public Vector3 LeftHandDir;

        /// <summary>
        /// 左手的本地的前向向量
        /// </summary>
        [SyncVar]
        public Vector3 LeftHandForward;
        /// <summary>
        /// 左手的本地的向上向量
        /// </summary>
        [SyncVar]
        public Vector3 LeftHandUp;


        /// <summary>
        ///  右手跟参考点的偏移量
        /// </summary>
        [SyncVar]
        public Vector3 RightHandDir;

        /// <summary>
        /// 右手的本地的前向向量
        /// </summary>
        [SyncVar]
        public Vector3 RightHandForward;
        /// <summary>
        /// 右手的本地的向上向量  
        /// </summary>
        [SyncVar]
        public Vector3 RightHandUp;

        /// <summary>
        /// 步骤同步，关键变量
        /// </summary>
        [SyncVar(hook = nameof(SceneStep))]
        public string SyncSceneStep;




        #region 两端都可能调用的代码
        private void Awake()
        {
            localBaseObject = this.GetComponent<LocalPlayObject>();

            localBaseObject.LocalInit(QuestHeadTransform);
        }

         void Update()
        {
            if (isClient)
            {

                if (isLocalPlayer)
                {
                    //计算跟全局参考点的偏移量

                    GetDir();

                }
                else
                {

                    //Transform rootTransform = LocalManager.Instance.SceneRootTransform;
                    ////获取同步变量的值，设置头，头的位置  

                    //if (rootTransform != null)
                    //{
                    //    QuestHeadTransform.position = rootTransform.position + rootTransform.TransformDirection(HeadDir);

                    //    QuestLeftTransform.position = rootTransform.position + rootTransform.TransformDirection(LeftHandDir);

                    //    QuestRightTransform.position = rootTransform.position + rootTransform.TransformDirection(RightHandDir);

                    //    QuestHeadTransform.rotation = Quaternion.LookRotation(rootTransform.TransformDirection(HeadForward), rootTransform.TransformDirection(HeadUp));

                    //    QuestLeftTransform.rotation = Quaternion.LookRotation(rootTransform.TransformDirection(LeftHandForward), rootTransform.TransformDirection(LeftHandUp));

                    //    QuestRightTransform.rotation = Quaternion.LookRotation(rootTransform.TransformDirection(RightHandForward), rootTransform.TransformDirection(RightHandUp));
                    //}


                }
            }

        }

        /// <summary>
        /// 场景同步的步骤
        /// </summary>
        /// <param name="oldStr"></param>
        /// <param name="newStr"></param>
        protected virtual void SceneStep(string oldStr, string newStr)
        {
            if (isServer)
            {
                //-----------------------------------------------------------------跨域逻辑-----------------------------------------------
                //同步变量在服务器上 
                Debug.Log($"同步变量在(服务器)上更新：{newStr}");
                //在服务器这里进行指令上的转换，把该步骤赋值给scnenet以及他们的派生类
                // BaseNetManager.FireCubeNetServer.ScenenNet_Server.SetStepEvent_Server(newStr);
                MessageCenter.Instance.SendMessage(FireCubeBaseCommon.SceneStepUpdate, this, newStr);
                //-----------------------------------------------------------------跨域逻辑-----------------------------------------------
            }
            else
            {
                Debug.Log($"同步变量在(客户端)上更新：{newStr}");
            }
        }
        protected virtual void GetDir()
        {
            var rootTransform = LocalManager.Instance.SceneRootTransform;

            if (rootTransform != null)
            {
                RootHeadTransform.position = QuestHeadTransform.position;
                RootHeadTransform.rotation = QuestHeadTransform.rotation;

                RootQuestLeftTransform.position = QuestLeftTransform.position;
                RootQuestLeftTransform.rotation = QuestLeftTransform.rotation;

                RootQuestRightTransform.position = QuestRightTransform.position;
                RootQuestRightTransform.rotation = QuestRightTransform.rotation;
            }
        }

        #endregion

        #region 客户端代码


        /// <summary>
        /// 客户端发送指令，要求服务器重启
        /// </summary>
        public void ClientResetServer()
        {
            CleanEvent();
            ComResetGame();
        }
        public ColliderEvent GetColliderEvent()
        {

            if (isLocalPlayer)
            {
                GameObject colliderGameObject = localBaseObject.FootColliderObject;

                return colliderGameObject.GetComponent<ColliderEvent>();
            }

            return null;
        }

        /// <summary>
        /// 接收服务器过来的重置消息
        /// TargetRpc保证本地玩家拥有者才有权限改写
        /// </summary>
        [TargetRpc]
        public void RPCResetGame()
        {
            BaseNetManager.FireCubeNetClient.EnableResetGame();
        }

        /// <summary>
        /// 接收服务器发送过来的偏移量
        /// </summary>
        /// <param name="localOffest"></param>
        [TargetRpc] // [TargetRpc]  没有NetworkConnection参数，因此该脚本的拥有者对象执行该方法
        public void RpcSetOffest(Vector3 localOffest)
        {
            Debug.Log($"获取服务器上的偏移量：{localOffest}");
            LocalManager.Instance.SetOffest(localOffest);
        }


        /// <summary>
        /// 本地设置步骤，同步到服务器,这个方法用在场景的步骤同步
        /// </summary>
        public virtual void LocalSceneSetStep(string step)
        {
            SyncSceneStep = step;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            this.name = this.name + $" {this.netIdentity.netId} ：{this.netIdentity}";


            //绑定到根物体
            Transform rootTransform = LocalManager.Instance.SceneRootTransform;

            if (rootTransform != null)
            {

                RootHeadTransform.parent = rootTransform;
                RootHeadTransform.position = QuestHeadTransform.position;
                RootHeadTransform.rotation = QuestHeadTransform.rotation;

                RootQuestLeftTransform.parent = rootTransform;
                RootQuestLeftTransform.position = QuestLeftTransform.position;
                RootQuestLeftTransform.rotation = QuestLeftTransform.rotation;

                RootQuestRightTransform.parent = rootTransform;
                RootQuestRightTransform.position = QuestRightTransform.position;
                RootQuestRightTransform.rotation = QuestRightTransform.rotation;

            }
            else Debug.LogError($"根物体为null");

        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            if (!LocalManager.Instance.IsPCThird)
            {
                //绑定到本地的头，左手，右手
                QuestHeadTransform.parent = LocalManager.Instance.HeadTransform;
                QuestHeadTransform.localPosition = Vector3.zero;
                QuestHeadTransform.localRotation = Quaternion.identity;

                QuestLeftTransform.parent = LocalManager.Instance.LeftHandTransform;
                QuestLeftTransform.localPosition = Vector3.zero;
                QuestLeftTransform.localRotation = Quaternion.identity;

                QuestRightTransform.parent = LocalManager.Instance.RightHandTransform;
                QuestRightTransform.localPosition = Vector3.zero;
                QuestRightTransform.localRotation = Quaternion.identity;
            }


            //主动获取偏移量
            //CmdGetOffest();

        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            Destroy(QuestHeadTransform.gameObject);
            Destroy(QuestLeftTransform.gameObject);
            Destroy(QuestRightTransform.gameObject);
        }

        public void CleanEvent()
        {
            if (isLocalPlayer)
            {
                GameObject colliderGameObject = localBaseObject.FootColliderObject;

                colliderGameObject.GetComponent<ColliderEvent>().CleanEvent();
            }

        }

        #endregion

        #region 服务端代码


        /// <summary>
        /// 从服务器获取偏移量
        /// </summary>
        [Command]
        public void CmdGetOffest()
        {
            RpcSetOffest(BaseNetManager.FireCubeNetServer.ServerWorldOffest);
        }


        /// <summary>
        /// 设置偏移量
        /// </summary>
        /// <param name="localOffest"></param>
        [Command]
        public void CmdSetOffest(Vector3 localOffest)
        {
            BaseNetManager.FireCubeNetServer.SetOffest(localOffest, this.netIdentity);
        }



        /// <summary>
        /// 授权服务器的物体给该客户端
        /// </summary>
        /// <param name="item"></param>
        [Command]
        public void CmdAssignClientAuthority(NetworkIdentity item)
        {
            GetClientAuthority(item, false);
        }
        /// <summary>
        /// 暴力重置服务器
        /// </summary>
        [Command]
        public void ComResetGame()
        {

            //广播所有的客户端，强制重新启动

            BaseNetManager.FireCubeNetServer.BroadcastResetGame();


            //1秒后关闭服务器
            LocalManager.Instance.WaitDo(1f, (() =>
            {
                NetworkManager.singleton.StopServer();

            }));
            //2秒后重新开始服务器
            LocalManager.Instance.WaitDo(2f, (() =>
            {
                NetworkManager.singleton.StartServer();
            }));
        }

        /// <summary>
        /// 给某个网络物体授权
        /// </summary>
        /// <param name="item"></param>
        public void GetClientAuthority(NetworkIdentity item, bool isAuto)
        {
            if (item.connectionToClient == null) //如果没有授权给客户端对象
            {
                item.AssignClientAuthority(connectionToClient);
                BaseNetManager.FireCubeNetServer.AuthorityDic.Add(item, this.netIdentity);


                BaseNetUI baseNet = item.GetComponent<BaseNetUI>();

                if (baseNet != null)
                {
                    baseNet.IsAutoGetAuthorityInServer = isAuto;
                    baseNet.ClientGetAuthority();

                    Debug.Log($"在服务器端，该物体：{baseNet.name}授权给了{this.name}玩家");
                }

            }
            else
            {
                Debug.LogError($"已经授权该对象给{item.connectionToClient} {item.name}玩家");
            }
        }

        #endregion












    }
}


