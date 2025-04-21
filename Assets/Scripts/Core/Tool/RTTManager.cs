using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTTManager : Singleton<RTTManager>
{
    private const string prefabPath = "Core/Prefabs/RTTObject";
    private Dictionary<object, RTTObject> rttObjects = new();

    /// <summary> 创建RTT对象，传入一个key和prefab，key用于后续删除RttObject </summary>
    public RTTObject CreateRttObject(object key, GameObject prefab, int width = 1024, int height = 1024)
    {
        foreach (var i in rttObjects)
        {
            if (i.Key == null && i.Value != null)
            {
                GameObject.Destroy(i.Value.gameObject);
                rttObjects.Remove(i.Key);
            }
            if (i.Value == null)
            {
                rttObjects.Remove(i.Key);
            }
        }
        RTTObject rttObject = ResourceManager.Instance.LoadAndInstantiate<RTTObject>(prefabPath);
        rttObject.InitRtt(prefab, width, height);
        rttObjects.Add(key, rttObject);
        return rttObject;
    }
    public void DestroyRttObject(object key)
    {
        if (rttObjects.TryGetValue(key, out RTTObject rttObject))
        {
            if (rttObject != null)
            {
                GameObject.Destroy(rttObject.gameObject);
            }
            rttObjects.Remove(key);
        }
    }
    public RTTObject GetRttObject(object key)
    {
        if (rttObjects.TryGetValue(key, out RTTObject rttObject))
        {
            return rttObject;
        }
        return null;
    }
}
