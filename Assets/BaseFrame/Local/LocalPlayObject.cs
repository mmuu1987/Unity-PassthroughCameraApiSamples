using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace FireCubeBase
{
    /// <summary>
    /// 本地层面上的玩家对象，作用->接收网络层给的数据，提交本地的交互数据给网络
    /// </summary>
    public class LocalPlayObject : MonoBehaviour
    {
        /// <summary>
        /// 角色脚下的碰撞体
        /// </summary>
        [SerializeField]
        private GameObject footCollider;

        public event Action<string> SetStepEvent;

        public GameObject FootColliderObject
        {
            get
            {
                return footCollider;
            }
        }

        private Transform _headTransform;
        // Start is called before the first frame update
        void Start()
        {
            //footCollider = this.transform.Find("LocalObject/角色脚下的碰撞体").gameObject;
        }

        // Update is called once per frame
        void Update()
        {


            //设置_fllowHeadObject位置
            footCollider.transform.position = new Vector3(_headTransform.position.x, 0f, _headTransform.position.z);
            
        }

        public virtual void LocalInit(Transform head)
        {
            _headTransform = head;
        }

        /// <summary>
        /// 接收服务器发送过来的步骤
        /// </summary>
        /// <param name="step"></param>
        public void SetStepNet( string step)
        {

        }


        /// <summary>
        /// 同步步骤到每个客户端
        /// </summary>
        /// <param name="step"></param>
        public void SetStep(string step)
        {
            SetStepEvent?.Invoke(step);
        }

    }

}
