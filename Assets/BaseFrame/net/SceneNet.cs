using System.Collections;
using System.Collections.Generic;
using FireCubeBase;
using Mirror;
using UnityEngine;
using XHFrameWork;

namespace FireCubeBase
{
    /// <summary>
    /// 定义的网络场景，本质上就是一个网络物体，只是这个物体表现为场景，是多个场景里面的物体的组合
    /// </summary>
    public class SceneNet : BaseNet
    {

       

        public ProcedureManagerBase ProcedureManager;

        /// <summary>
        /// 步骤同步，关键变量
        /// 
        /// </summary>
        [SyncVar(hook = nameof(Step))]
        public string SyncStep;


        protected override void OnAwake()
        {
            base.OnAwake();

            ProcedureManager.InIt();
        }

        /// <summary>
        /// 轴向提示
        /// </summary>
        public GameObject LogGameObject;

       
        #region 客户端

        public override void OnStartClient()
        {
            base.OnStartClient();

            localBaseObject.transform.position = LocalManager.Instance.SceneRootTransform.position;

            localBaseObject.transform.rotation = LocalManager.Instance.SceneRootTransform.rotation;
        }

        /// <summary>
        /// 接收的步骤
        /// </summary>
        public virtual void SetStepEvent_Server(string step)
        {
            Debug.Log($"同步变量到服务器：{step}");
            //发送同步变量Step
            SyncStep = step;//同步到客户端

            //服务端执行
            Step(null,step);
        }

        #endregion


        protected override void OnUpDate()
        {
            //base.OnUpDate();todo 场景网络物体不需要同步，在各自的客户端定位

            if (ProcedureManager != null) ProcedureManager.MachineUpdate();

            if (isClient && LocalManager.Instance.IsAndroidPad)
            {
                UpdateARcoreAnchor();
            }
        }
        private float arcoreTimetemp = 0f;
        private void UpdateARcoreAnchor()
        {
            if (arcoreTimetemp >= 1f)//1秒更新一次位置旋转 
            {
                arcoreTimetemp = 0f;

                Transform myWorldCenterPoint = LocalManager.Instance.MyWorldCenterPoint;


                if (myWorldCenterPoint != null)
                {
                    if(LocalManager.Instance.IsAndroidPad)
                     localBaseObject.transform.position = myWorldCenterPoint.position + new Vector3(0f, -0.065f, 0f); 
                    else localBaseObject.transform.position = myWorldCenterPoint.position;

                    localBaseObject.transform.rotation = myWorldCenterPoint.rotation;

                   // Debug.Log($"锚点 {myWorldCenterPoint.name} 位置：{myWorldCenterPoint.transform.position}  旋转：{myWorldCenterPoint.eulerAngles}");
                }
            }
            else
            {
                arcoreTimetemp += Time.deltaTime;
            }

        }

        /// <summary>
        /// 接收同步变量SyncStep
        /// </summary>
        /// <param name="oldStep"></param>
        /// <param name="newStep"></param>
        public virtual void Step(string oldStep,string newStep)
        {
               // Debug.Log($"{this.GetType()} Step({newStep})");
        }

        /// <summary>
        /// 改变状态机的状态
        /// </summary>
        public virtual void ChangeState()
        {

        }

        //------------------------------------------------------------------------------服务端-----------------------------------------------------

        public override void OnStartServer()
        {
            base.OnStartServer();

            //服务器启动的时候场景物体的第一个状态必定是 开始状态的第一个状态



            BaseNetManager.FireCubeNetServer.ScenenNet_Server = this;

            SetStepEvent_Server(FireCubeBaseCommon.Step_Procedure_Start_Enter);

          
        }
    }

}

