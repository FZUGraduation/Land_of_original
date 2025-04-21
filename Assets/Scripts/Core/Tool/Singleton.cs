using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//非 MonoBehaviour 泛型单例
public class Singleton<T> where T : new()
{
    private static readonly object _lock = new object();
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    //如果 _instance 不为null 再new一个实例
                    _instance ??= new T();
                }
            }
            return _instance;
        }
    }
}
//MonoBehaviour 泛型单例
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";
                            DontDestroyOnLoad(singletonObject);
                        }
                    }
                }
            }
            return _instance;
        }
    }
    /// <summary>
    /// 是否已经初始化这个单例
    /// </summary>
    public static bool IsInitialized
    {
        get
        {
            return _instance != null;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (gameObject.transform.parent == null)
            {
                //DontDestroyOnLoad 方法只能用于根 GameObject 或根 GameObject 上的组件
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
