using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace FireCubeBase
{
    /// <summary>
    /// 配合RayInteractable  和 ColliderSurface 两个脚本使用
    /// 自动把父物体的碰撞体，赋值给ColliderSurface
    /// 并且承担事件转发工作
    /// </summary>
    public class AutoSelectCollider : MonoBehaviour
    {


        /// <summary>
        /// 射线经过的事件
        /// </summary>
        public event Action<GameObject, bool> HoverEvent;

        /// <summary>
        /// 手柄射线选中该物体的事件
        /// </summary>
        public event Action<GameObject, PointerEventType> SelectEvent;


        /// <summary>
        /// 是否已经激活该类的RayEvent功能
        /// </summary>
        public bool IsActiveRayEvent = false;

        private ColliderSurface _colliderSurface;

        private  Collider _collider;



        private List<Material> _originalMaterials = new List<Material>();

        public List<MeshRenderer> MeshRenderers { get; private set; }

        
        private void Awake()
        {
            _colliderSurface = this.GetComponent<ColliderSurface>();

            //父物体的碰撞体
            _collider = this.transform.GetComponent<Collider>();

            if (_collider != null)
            {
                _colliderSurface.InjectCollider(_collider);
            }
            else
            {
                throw new UnityException("没有找到碰撞体");
            }

            RayInteractable rayInteractable = this.GetComponent<RayInteractable>();

            rayInteractable.WhenPointerEventRaised += RayInteractable_WhenPointerEventRaised;



            MeshRenderers = _collider.gameObject.GetComponentsInChildren<MeshRenderer>().ToList();//拿到第一个meshrenderz作为该变量



            foreach (MeshRenderer meshRenderer in MeshRenderers)
            {
                _originalMaterials.Add(meshRenderer.material);
            }

            if (MeshRenderers == null) throw new UnityException("获取的渲染网格列表为null");
        }

        private void RayInteractable_WhenPointerEventRaised(PointerEvent obj)
        {

            if (!IsActiveRayEvent) return;

            if (obj.Type == PointerEventType.Hover)
            {
                HoverEvent?.Invoke(_collider.gameObject, true);

                foreach (MeshRenderer meshRenderer in MeshRenderers)
                {
                    meshRenderer.material = LocalManager.Instance.SelectMaterial;
                }
                SelectEvent?.Invoke(_collider.gameObject, PointerEventType.Hover);

            }
            else if (obj.Type == PointerEventType.Unhover)
            {
                HoverEvent?.Invoke(_collider.gameObject, false);

                for (int i = 0; i < _originalMaterials.Count; i++)
                {
                    MeshRenderers[i].material = _originalMaterials[i];
                }
                SelectEvent?.Invoke(_collider.gameObject, PointerEventType.Unhover);
            }
            else if (obj.Type == PointerEventType.Select)
            {
                SelectEvent?.Invoke(_collider.gameObject, PointerEventType.Select);
            }

        }


        public void SetPointerEvent(PointerEventType type)
        {
            if (type == PointerEventType.Hover)
            {
                if (MeshRenderers != null)
                    foreach (MeshRenderer meshRenderer in MeshRenderers)
                    {
                        meshRenderer.material = LocalManager.Instance.SelectMaterial;
                    }
                else
                {
                    Debug.Log($"该网络数组为null {this.transform.name}");
                }
            }
            else if (type == PointerEventType.Unhover)
            {
                if (MeshRenderers != null)
                    for (int i = 0; i < _originalMaterials.Count; i++)
                    {
                        MeshRenderers[i].material = _originalMaterials[i];
                    }
                else
                {
                    Debug.Log($"该网络数组为null {this.transform.name}");
                }

            }
        }
    }

}

