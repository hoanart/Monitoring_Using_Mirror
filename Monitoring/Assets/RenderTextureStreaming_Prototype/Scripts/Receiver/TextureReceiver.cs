using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureReceiver : MonoBehaviour {

    public RawImage rawImg; 
        
    private Texture2D tex;

    private ReceiveTexture receiver;
    
    // Start is called before the first frame update
    void Start()
    {
        
        receiver = GetComponent<ReceiveTexture>();

        tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        receiver.SetTexutre(tex);
        rawImg.texture = tex;

      //  receiver.SetTexutre(rawImg.texture);
    }

    private void Update()
    {
        if (rawImg.enabled != receiver.isServer)
        {
            rawImg.enabled = receiver.isServer;    
        }
    }
}
