using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FireCubeBase
{
    public class ImageInfoPrefab : MonoBehaviour
    {



        // Start is called before the first frame update
        void Start()
        {
            // Debug.Log($"Start ImageInfoPrefab");

            //MyARcoreManager.Instance.AddAnchor(this.transform);

            //StartCoroutine(waitTime(1f, (() =>
            //{
            //    Debug.Log($"Ãªµã {this.gameObject.name} Î»ÖÃ£º{this.transform.position} Ðý×ª£º{this.transform.eulerAngles}");
            //})));
        }

        private IEnumerator waitTime(float time, Action action)
        {
            while (true)
            {
                yield return new WaitForSeconds(time);

                action?.Invoke();
            }

        }
        // Update is called once per frame
        void Update()
        {

        }


    }

}
