using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class MoveTest : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {

        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
        
    }
    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
        float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;
        
        transform.Rotate(0, moveX, 0);
        transform.Translate(0, 0, moveZ);
        
    }
}
