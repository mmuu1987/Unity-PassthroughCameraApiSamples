using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FireCubeBase
{/// <summary>
    /// 物体的碰撞事件
    /// </summary>
    public class ColliderEvent : MonoBehaviour
    {
        public event Action<Collider> OnTriggerEnterEvent;

        public event Action<Collider> OnTriggerStayEvent;

        public event Action<Collider> OnTriggerExitEvent;

        void OnTriggerEnter(Collider other)
        {
            Debug.Log($"该物体：{this.name} 碰撞触发到了：{other.name}");

            OnTriggerEnterEvent?.Invoke(other);
        }

        public void AddEnterEvent(Action<Collider> action)
        {

            //Debug.Log($"添加事件{action.Target.GetType()}");

            OnTriggerEnterEvent += action;
        }
        public void RemoveEnterEvent(Action<Collider> action)
        {

           // Debug.Log($"移除事件{action.Target.GetType()}");

            OnTriggerEnterEvent -= action;

            //if (OnTriggerEnterEvent != null)
            //{
            //    foreach (Delegate @delegate in OnTriggerEnterEvent.GetInvocationList())
            //    {
            //        Debug.Log($"{@delegate.Method.Name} {@delegate.Target.GetType()}");
            //    }
            //}
        }


        public void CleanEvent()
        {
            OnTriggerEnterEvent = null;
        }
        public void GetEventInfo()
        {
            if (OnTriggerEnterEvent != null)
            {
                foreach (Delegate @delegate in OnTriggerEnterEvent.GetInvocationList())
                {
                    Debug.Log($"{@delegate.Method.Name} {@delegate.Target.GetType()} ");
                }
            }
        }
        void OnTriggerStay(Collider other) { OnTriggerStayEvent?.Invoke(other); }//当触发器停留在当前物体时触发的回调函数
        void OnTriggerExit(Collider other) { OnTriggerExitEvent?.Invoke(other); }//当触发器离开当前物体时触发的回调函数
    }

}
