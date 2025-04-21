using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonEventCenter<T> : BaseEventCenter where T : new()
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
