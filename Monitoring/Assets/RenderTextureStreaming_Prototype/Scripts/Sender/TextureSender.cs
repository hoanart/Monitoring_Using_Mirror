using UnityEngine;
public class TextureSender : MonoBehaviour {
    private SendTexture sender;

    private RenderTexture rt;

    private Texture2D sendTex;

    private Texture2D compressTex;

    private struct SizeStruct
    {
        public int width;
        public int height;
    }
    SizeStruct size;
    
    // Start is called before the first frame update
    void Start()
    {
        sender = GetComponent<SendTexture>();
    }

    // Update is called once per frame
    void Update()
    {
       // Debug.Log($"sender.HasRenderTexture : " + sender.HasRenderTexture());
        if (sender.HasRenderTexture() && sender.IsClient())
        {
            if (sendTex == null)
            {
                rt = sender.rt;
                Debug.Log("rt.format : "+rt.format);
                sendTex = new Texture2D(rt.width,rt.height, TextureFormat.RGBA32, false);
            }
            else
            {
                RenderTexture.active = rt;
               
                sendTex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0,false);
                sendTex.Apply();
                sender.SetTexutre(sendTex);
            }
        }
    }
}
