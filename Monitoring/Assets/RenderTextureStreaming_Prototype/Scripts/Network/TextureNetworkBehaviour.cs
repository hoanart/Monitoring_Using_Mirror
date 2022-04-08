using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class TextureNetworkBehaviour : NetworkBehaviour {
    
    public Texture2D source;
     
    
    public  Texture2D SetTexutre(Texture2D src)
    {
        source = src;
        return source;
    }

    
    public bool IsClient()
    {
        return isClient;
    }

}
