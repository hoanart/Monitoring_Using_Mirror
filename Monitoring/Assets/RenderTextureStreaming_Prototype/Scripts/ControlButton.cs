using UnityEngine;

public class ControlButton : MonoBehaviour {
    [Tooltip("버튼 오브젝트 배열")]
    public GameObject[] buttonObjects;
    
    // Start is called before the first frame update
    void Awake()
    {
        buttonObjects = new GameObject[transform.childCount];
        
        for (int i = 0; i < transform.childCount; i++)
        {
            buttonObjects[i] = transform.GetChild(i).gameObject;
        }
    }
}
