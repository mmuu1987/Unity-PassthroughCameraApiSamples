using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FireCubeBase
{
    public  static class FireCubeBaseCommon 
    {
        #region 状态机相关的步骤指令

        public const string Step_Procedure_Start_Enter = "开始状态_进入该状态";

        #endregion



        /// <summary>
        /// 场景步骤更新改动
        /// </summary>
        public const string SceneStepUpdate = "SceneStepUpdate";
    }
    public enum ToolEventType:int
    {
        None=0,
        Point,
      
        Trigger,
        GetAuthority,
        DisAuthority,
        ToolsUI
    }
    /// <summary>
    /// 同步变量传输的数据
    /// </summary>
    public struct ToolEventData
    {
        public string Name;
        public int Index;
        public ToolEventType EventType;
        public bool isDirty;
    }

    /// <summary>
    /// 事件信息类
    /// </summary>
    [Serializable]

    public class EventInfo
    {
        public string EventName;

        public int State;

    }
}

