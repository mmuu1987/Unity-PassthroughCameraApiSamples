using System.Collections;
using System.Collections.Generic;
using FireCubeBase;
using TMPro;
using UnityEngine;


public class ShowTime : MonoBehaviour
{

    public TextMeshProUGUI textMeshProUgui;

    public TextMeshProUGUI DisMeshProUgui;

    public LineRenderer lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (textMeshProUgui != null)
        {
            float time = LocalManager.Instance.TrackingTime;

            int minutes = (int)time / 60;

            int seconds = (int)time % 60;

            textMeshProUgui.text = $"{minutes}: {seconds}";
        }
        if (DisMeshProUgui != null)
        {
            Vector3 beginPos = LocalManager.Instance.MyWorldCenterPoint.position;

            Vector3 endPos = LocalManager.Instance.SceneRootTransform.position;

            float dis = Vector3.Distance(beginPos, endPos);

            DisMeshProUgui.text = $"offset:{dis.ToString("n2")}m";

            lineRenderer.SetPosition(0,beginPos);
            lineRenderer.SetPosition(1,endPos);
        }
    }
}

