using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnablingXR;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class Test : MonoBehaviour
{

    private void Start()
    {
#if UNITY_EDITOR

        //Debug.Log($"{SceneManager.GetActiveScene().name}");

        if (SceneManager.GetActiveScene().name != "ThirdCam")
        {
            //这段代码的作用是解决Failed to set DeveloperMode on Start.
            //引用网址：https://www.anton.website/enable-unity-xr-in-runtime/
            StartCoroutine(XRController.EnableXRCoroutine());


        }

#endif
    }
}




