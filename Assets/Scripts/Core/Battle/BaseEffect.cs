

using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class BaseEffect
{
    public Sprite icon;
    public string effectName;
    public string description;
    [Tooltip("持续回合数, 小于0表示一直存在,如果是立即触发的效果，这个值无效")]
    public int stayCount;
    [Tooltip("是否需要计算命中率")]
    public bool needCaculateHitrate = false;
    [Tooltip("基础命中率"), ShowIf("needCaculateHitrate"), Range(0, 1)]
    public float baseHitRate = 1;
    [HideInInspector]
    public int StayCount
    {
        get => stayCount;
        set
        {
            stayCount = value;
            OnStayCountChange?.Invoke(stayCount);
        }
    }
    [HideInInspector]
    public Action<int> OnStayCountChange;
}
