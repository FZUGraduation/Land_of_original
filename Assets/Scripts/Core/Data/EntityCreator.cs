using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
[System.Serializable]
public class EntityCreator : SerializedScriptableObject
{
    [ReadOnly]
    public Type type;

    public List<ConfigData> datas = new List<ConfigData>();

    public EntityCreator(Type type)
    {
        this.type = type;
    }

    public void Add(ConfigData obj)
    {
        datas.Add(obj);
    }

    public void Remove(ConfigData obj)
    {
        datas.Remove(obj);
    }

    public void Sort()
    {
        datas.Sort((a, b) => string.Compare(a.key, b.key, StringComparison.Ordinal));
    }

}
