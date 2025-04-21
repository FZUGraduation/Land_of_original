using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : SingletonMono<ResourceManager>
{
    // 资源缓存
    private Dictionary<string, Object> _resourceCache = new();

    /// <summary>
    /// 同步加载资源
    /// </summary>
    public T Load<T>(string path) where T : Object
    {
        if (_resourceCache.TryGetValue(path, out Object resource))
        {
            return resource as T;
        }

        T loadedResource = Resources.Load<T>(path);
        if (loadedResource != null)
        {
            _resourceCache[path] = loadedResource;
        }
        else
        {
            Debug.LogError($"Failed to load resource at path: {path}");
        }

        return loadedResource;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    public void LoadAsync<T>(string path, System.Action<T> onLoaded) where T : Object
    {
        if (_resourceCache.TryGetValue(path, out Object resource))
        {
            onLoaded?.Invoke(resource as T);
            return;
        }

        StartCoroutine(LoadAsyncCoroutine(path, onLoaded));
    }

    private IEnumerator LoadAsyncCoroutine<T>(string path, System.Action<T> onLoaded) where T : Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);
        yield return request;

        if (request.asset != null)
        {
            _resourceCache[path] = request.asset;
            onLoaded?.Invoke(request.asset as T);
        }
        else
        {
            Debug.LogError($"Failed to load resource at path: {path}");
            onLoaded?.Invoke(null);
        }
    }

    /// <summary> 同步加载并实例化资源 </summary>
    public T LoadAndInstantiate<T>(string path, Transform parent = null, System.Action<GameObject> onInstantiated = null) where T : Object
    {
        GameObject resource = Load<GameObject>(path);
        GameObject instantiatedObject = Instantiate(resource, parent);
        onInstantiated?.Invoke(instantiatedObject);
        return instantiatedObject.GetComponent<T>();
    }

    /// <summary> 异步加载并实例化资源 </summary>
    public void LoadAndInstantiateAsync(string path, Transform parent, System.Action<GameObject> onInstantiated)
    {
        LoadAsync<GameObject>(path, (resource) =>
        {
            GameObject instantiatedObject = Instantiate(resource, parent);
            onInstantiated?.Invoke(instantiatedObject);
        });
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Unload(string path)
    {
        if (_resourceCache.ContainsKey(path))
        {
            Object resource = _resourceCache[path];
            Resources.UnloadAsset(resource);
            _resourceCache.Remove(path);
        }
    }

    /// <summary>
    /// 释放所有资源
    /// </summary>
    public void UnloadAll()
    {
        foreach (var resource in _resourceCache.Values)
        {
            Resources.UnloadAsset(resource);
        }
        _resourceCache.Clear();
    }
}