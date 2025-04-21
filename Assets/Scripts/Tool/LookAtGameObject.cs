using UnityEngine;

public class LookAtGameObject : MonoBehaviour
{
    public Transform go;
    private Transform cam;
    void Awake()
    {
        cam = Camera.main.transform;
    }
    void Update()
    {
        if (go != null)
        {
            transform.LookAt(go.transform.position);
        }
        else
        {
            transform.LookAt(cam.position);
        }
    }
}
