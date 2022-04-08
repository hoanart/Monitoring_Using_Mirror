using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SyncVarDemo : NetworkBehaviour {
     [SyncVar(hook = nameof(SetColor))]
    private Color32 _color = Color.red;

    
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocalPlayer)
        Hola();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(_RandomizeColor());
    }

    private void SetColor(Color32 oldColor, Color32 newColor)
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = newColor;
        
    }

    IEnumerator _RandomizeColor()
    {
        WaitForSeconds wait = new WaitForSeconds(2f);
       
        while (true)
        {
            yield return wait;
            Renderer renderer = GetComponent<Renderer>();
            _color = Random.ColorHSV(0f, 1f, 1f, 1f, 0f, 1f, 1f, 1f);
            renderer.material.color = _color;
        }
       
    }
    [Command]
    void Hola()
    {
        Debug.Log("Hola");
    }
}
