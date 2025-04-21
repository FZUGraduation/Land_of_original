
using UnityEngine;

public class RTTObject : MonoBehaviour
{
    public Camera rttCamera;
    public RenderTexture renderTexture;
    public GameObject rttObjectRoot;
    private static int rttCount = 0;
    private static int rttCurrentCount = 0;
    void Start()
    {
        rttCount++;
        rttCurrentCount++;
        transform.position = new Vector3(0, 0, rttCount * 500);
    }

    void OnDestroy()
    {
        rttCurrentCount--;
        if (rttCurrentCount == 0)
        {
            rttCount = 0;
        }
        renderTexture.Release();
    }

    public void InitRtt(GameObject prefab, int width = 1024, int height = 1024)
    {
        GameObject obj = Instantiate(prefab, rttObjectRoot.transform);
        ReplaceRttPrefab(obj);
        renderTexture = new RenderTexture(width, height, 24);
        rttCamera.targetTexture = renderTexture;
    }
    public void ReplaceRttPrefab(GameObject prefab)
    {
        foreach (Transform child in rttObjectRoot.transform)
        {
            Destroy(child.gameObject);
        }
        rttObjectRoot.transform.rotation = Quaternion.identity;
        GameObject obj = Instantiate(prefab, rttObjectRoot.transform);
        obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        obj.transform.localScale = Vector3.one;

        //不知道为什么第一次animator会被禁用，这里强制启用
        var ani = obj.GetComponentInChildren<Animator>();
        if (ani != null)
        {
            ani.enabled = true;
        }
        obj.layer = LayerMask.NameToLayer("RTT");
    }
}
