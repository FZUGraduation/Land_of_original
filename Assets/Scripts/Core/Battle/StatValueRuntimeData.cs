
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;


//MemberSerialization.OptIn 指定只有带有 [JsonProperty] 属性的成员才会被序列化
// [JsonObject(MemberSerialization.OptIn)]
/// <summary>
/// 用于战斗时的数值计算，例如生命值、攻击力等
/// </summary>
public class StatValueRuntimeData : RuntimeData
{

    // public StatValueConfigData ConfigData
    // {
    //     get; private set;
    // }

    // [DefaultValue("default")]
    // public string ConfigKey
    // {
    //     get
    //     {
    //         return ConfigData.key;
    //     }
    //     set
    //     {
    //         ConfigData = Datalib.Instance.GetData<StatValueConfigData>(value);
    //     }
    // }

    public int ownerBattleID;

    protected bool isDirty = true;
    // [SerializeField]
    // [JsonProperty]
    // [DefaultValue(0f)]
    private float baseValue;
    private float lastBaseValue;
    private float modifiedValue;
    private float maxValue;
    private float minValue;
    bool notificationLock = false;//用于防止递归调用 NotifyValueModified 方法

    // [JsonIgnore]
    public bool silence = false;

    // [JsonIgnore]
    // [SerializeField]
    private List<StatModifierEffect> statModifiers = new();

    /// <summary> 修改值时触发的事件 </summary>
    public event Action<StatValueRuntimeData> OnValueModified;
    public event Action<int> OnBaseValueChanged;//参数为变化的值



    /// <summary> Return modified value with all StatsModifier applied </summary>
    public float ModifiedValue
    {
        get
        {
            if (isDirty || lastBaseValue != baseValue)
            {
                lastBaseValue = baseValue;
                var _modifiedValue = CalculateFinalValue();
                // if (_modifiedValue > ConfigData.modifieValueMax) _modifiedValue = ConfigData.modifieValueMax;
                // if (_modifiedValue < ConfigData.modifieValueMin) _modifiedValue = ConfigData.modifieValueMin;
                if (_modifiedValue > maxValue) _modifiedValue = maxValue;
                if (_modifiedValue < minValue) _modifiedValue = minValue;
                modifiedValue = _modifiedValue;
                isDirty = false;
            }
            return modifiedValue;
        }
    }

    /// <summary> Return unmodified value </summary>
    public float BaseValue
    {
        get
        {
            return baseValue;
        }
        set
        {
            // if (value > ConfigData.baseValueMax) value = ConfigData.baseValueMax;
            // if (value < ConfigData.baseValueMin) value = ConfigData.baseValueMin;
            float oldValue = baseValue;
            if (value > maxValue) value = maxValue;
            if (value < minValue) value = minValue;
            if (value != baseValue)
            {
                isDirty = true;
                baseValue = value;
                NotifyValueModified();
            }
            if (oldValue != baseValue)
            {
                OnBaseValueChanged?.Invoke((int)(baseValue - oldValue));
            }
        }
    }
    public float MaxValue
    {
        get
        {
            return maxValue;
        }
        set
        {
            maxValue = value;
            if (baseValue > maxValue) baseValue = maxValue;
            if (ModifiedValue > maxValue) isDirty = true;
            NotifyValueModified();
        }
    }
    public float MinValue
    {
        get
        {
            return minValue;
        }
        set
        {
            minValue = value;
            if (baseValue < minValue) baseValue = minValue;
            if (ModifiedValue < minValue) isDirty = true;
            NotifyValueModified();
        }
    }

    public float GetPercentage() => ModifiedValue / maxValue;


    // public StatValueRuntimeData(string key)
    // {
    //     ConfigKey = key;
    // }
    public StatValueRuntimeData(int ownerID = -1)
    {
        this.ownerBattleID = ownerID;
    }
    public StatValueRuntimeData(float value, float minValue = 0, float maxValue = 9999)
    {
        // ConfigKey = key;
        this.maxValue = maxValue;
        this.minValue = minValue;
        baseValue = value;
    }

    public void InitValue(float value, float minValue = 0, float maxValue = 9999)
    {
        this.maxValue = maxValue;
        this.minValue = minValue;
        BaseValue = value;
    }

    /// <summary> 用于通知值已被修改 </summary>
    public void NotifyValueModified()
    {
        if (notificationLock == false && silence == false)
        {
            notificationLock = true;
            OnValueModified?.Invoke(this);
            notificationLock = false;
        }
    }

    [SerializeField]
    private bool updateRunning;

    /// <summary> Add modifier to this value </summary>
    public virtual void AddModifier(StatModifierEffect mod)
    {
        if (mod.canMulti)
        {
            statModifiers.Add(mod);
        }
        else
        {
            var index = statModifiers.FindIndex((item) => item.effectName == mod.effectName);
            if (index == -1)
            {
                statModifiers.Add(mod);//如果没有找到相同的modifier，则添加
                BattleData.Instance.Emit(BattleData.AddEffect, ownerBattleID, mod);
            }
            else
            {
                // BattleData.Instance.Emit(BattleData.RemoveEffect, ownerBattleID, statModifiers[index]);
                statModifiers[index].stayCount = mod.stayCount;//如果找到相同的modifier，则替换,相当于刷新回合数
            }
        }
        isDirty = true;
        NotifyValueModified();
    }

    /// <summary> Remove a modifier from this value </summary>
    public virtual void RemoveModifier(StatModifierEffect mod)
    {
        if (statModifiers.Remove(mod))
        {
            isDirty = true;
        }
        NotifyValueModified();
    }

    /// <summary> Remove all modifiers </summary>
    public void RemoveAllModifiers()
    {
        statModifiers.Clear();
        isDirty = true;
    }

    public virtual bool RemoveAllModifiersFromSource(object _source)
    {
        int numRemovals = statModifiers.RemoveAll(mod => mod.sourceData == _source);

        if (numRemovals > 0)
        {
            isDirty = true;
            return true;
        }
        return false;
    }

    protected virtual int CompareModifierOrder(StatModifierEffect a, StatModifierEffect b)
    {
        if (a.statCalculatetype < b.statCalculatetype)
            return -1;
        else if (a.statCalculatetype > b.statCalculatetype)
            return 1;
        return 0; //if (a.type == b.type)
    }

    /// <summary> 计算最终值 </summary>
    protected virtual float CalculateFinalValue()
    {
        float finalValue = baseValue;
        float sumPercentAdd = 0;
        float flatValue = 0;
        // float sumPercentMult = 0;

        statModifiers.Sort(CompareModifierOrder);

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifierEffect mod = statModifiers[i];

            if (mod.statCalculatetype == BasicModifierType.Flat)
            {
                flatValue += mod.value;
            }
            else if (mod.statCalculatetype == BasicModifierType.Percent)
            {
                sumPercentAdd += mod.value;
            }
            // else if (mod.type == BasicModifierType.PercentMult)
            // {
            //     sumPercentMult *= 1 + mod.value;
            // }
        }
        finalValue *= 1 + sumPercentAdd;
        finalValue += flatValue;
        // finalValue *= 1 + sumPercentMult;
        // 浮点计算错误的解决方法，例如显示 12.00001 而不是 12: 将一个双精度浮点数四舍五入到小数点后四位，并将结果转换为单精度浮点数
        return (float)Math.Round(finalValue, 4);
    }

    public List<StatModifierEffect> GetAllModifiers()
    {
        return statModifiers;
    }
    /// <summary>
    /// 在行动结束时清除持续时间为0的modifier
    /// </summary>
    public void ActionEnd()
    {
        //使用反向遍历集合，这样可以安全地移除元素而不会影响遍历过程
        for (int i = statModifiers.Count - 1; i >= 0; i--)
        {
            var mod = statModifiers[i];
            if (mod.StayCount > 0)
            {
                mod.StayCount--;
                if (mod.StayCount == 0)
                {
                    RemoveModifier(mod);
                    Debug.Log($"{ownerBattleID}:效果消失！！！:{mod.effectName}");
                    BattleData.Instance.Emit(BattleData.RemoveEffect, ownerBattleID, mod);
                }
            }
            else if (mod.StayCount == 0)
            {
                RemoveModifier(mod);
            }
        }
    }
}
