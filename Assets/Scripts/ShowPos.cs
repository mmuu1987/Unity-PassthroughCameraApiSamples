using System.Collections;
using System.Collections.Generic;
using FireCubeBase;
using TMPro;
using UnityEngine;


public class ShowPos : MonoBehaviour
{

    public TextMeshProUGUI textMeshPro;

  

    private float timeTemp = 0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (timeTemp >= 0.1f)
        {
            timeTemp = 0f;

            textMeshPro.text = this.transform.position.ToString();
        }
        else
        {
            timeTemp += Time.deltaTime;
        }

      
    }
}

