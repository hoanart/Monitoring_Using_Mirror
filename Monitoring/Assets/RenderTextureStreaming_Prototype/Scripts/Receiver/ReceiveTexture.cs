using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReceiveTexture : TextureNetworkBehaviour {
    public string senderName = "Sender";
    
    private SendTexture sender;
    
    //private Texture2D targetTex;

    private bool mbReceive;

    public bool isStartServer;
    
    // Start is called before the first frame update
    void Start()
    {
        sender = GameObject.Find(senderName).GetComponent<SendTexture>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!sender.isActiveRenderCam)
        {
            return;
        }

        if (!sender.isSend)
        {
            return;
        }
        
        if (isServer)
        {
            if (source.width == 1)
            {
                source.Resize(sender.width, sender.height);
            }
            
            sender.isSend = false;
            
            source.LoadRawTextureData(CompressHelper.Decompress(sender.DisplayCompressByte()));
            source.Apply();
        }

    }
}
