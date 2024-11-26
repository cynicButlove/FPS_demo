using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetUICamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //添加事件摄像机
        Camera uiCamera = GameObject.Find("PlayerParent").transform.Find("Player").Find("Assult_Rife_Arm").Find("Main Camera").GetComponent<Camera>();
        GetComponent<Canvas>().worldCamera = uiCamera;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
