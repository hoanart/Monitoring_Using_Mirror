using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Terra;
using UnityEngine;
using UnityEngine.XR;

public class AutomaticConnectionToServer : MonoBehaviour {
    [Tooltip("데스크탑에서 실행되었을 때의 메인카메라")]
    public GameObject mainCamera;
    [Tooltip("접속할 서버의 IP주소")]
    public string address = "192.168.0.174";
    
    private NetworkManager manager;
    private GameObject netUI;
    
    // Start is called before the first frame update
    void Start()
    {
        manager = NetworkManager.singleton;
        
        netUI = GameObject.Find("NetUIPanel");
    }

    private void Update()
    {
        DeviceIsNotDesktop();
    }

    public void DeviceIsNotDesktop()
    {       
        if (SystemInfo.deviceType != DeviceType.Desktop && !NetworkClient.active)
        {
            manager.networkAddress = address;
            manager.StartClient();
            mainCamera.SetActive(false);
            netUI.SetActive(false);
        }
    }
}
