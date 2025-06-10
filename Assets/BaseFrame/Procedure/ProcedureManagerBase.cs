using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


namespace FireCubeBase
{
    /// <summary>
    /// 流程管理器，管理工务上的每个一流程
    /// 
    /// </summary>
    public class ProcedureManagerBase : MonoBehaviour
    {
        /// <summary>
        /// 状态机触发步骤的事件
        /// </summary>
        public event Action<string> StepEvent; 

        public FsmStateMachine<ProcedureManagerBase> m_Machine;//状态机对象用来管理状态的

        public Dictionary<int, FsmState<ProcedureManagerBase>> StateDic = new Dictionary<int, FsmState<ProcedureManagerBase>>(); //状态字典，用来存放状态

        /// <summary>
        /// 流程信息的集合
        /// </summary>
        public List<ProcedureInfo> ProcedureInfos = new List<ProcedureInfo>();



        private void Start()
        {
        }

        /// <summary>
        /// 初始化流程管理器
        /// </summary>
        public virtual void InIt()
        {
          

            if (m_Machine == null)
                m_Machine = new FsmStateMachine<ProcedureManagerBase>(this);

            if (StateDic.Count == 0)
            {
                foreach (ProcedureInfo info in ProcedureInfos)
                {
                    ProcedureEntity entity = null;


                    StateDic.Add(info.ProcedureState, entity);
                }
            }



        }

        /// <summary>
        /// 设置当前状态
        /// </summary>
        /// <param name="state"></param>
        public virtual void SetCurrentState(int state)
        {
            m_Machine.SetCurrentState(StateDic[state]);
        }

        public void MachineUpdate()
        {
            if(m_Machine!=null)
             m_Machine.SmUpdate();
        }


      

        /// <summary>
        /// 改变流程
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(int state)
        {
            m_Machine.ChangeState(StateDic[state]);
        }

        public virtual void  ReceiveStep(string step)
        {

        }
    }

}
