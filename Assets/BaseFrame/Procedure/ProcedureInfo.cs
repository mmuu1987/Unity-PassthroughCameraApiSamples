using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace FireCubeBase
{
    /// <summary>
    /// 每个流程需要的信息
    /// </summary>
    public class ProcedureInfo : MonoBehaviour
    {
        /// <summary>
        /// 该流程的名字
        /// </summary>
        public string ProcedureName;
        /// <summary>
        /// 该流程的名字
        /// </summary>
        public int ProcedureState;
        /// <summary>
        /// 该流程的Timelines
        /// </summary>
        public List<PlayableDirector> PlayableDirectors;

        /// <summary>
        /// 该步骤需要到的物体
        /// </summary>
        public List<GameObject> NeedObjects = new List<GameObject>();

        /// <summary>
        /// 进入该状态下，需要显示的激活的物体
        /// </summary>
        public List<GameObject> ShowObjectList = new List<GameObject>();


    }

}
