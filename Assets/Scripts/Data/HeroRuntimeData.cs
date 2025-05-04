using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Sirenix.OdinInspector.Demos.RPGEditor;
using UnityEngine;

[JsonObject(MemberSerialization.OptIn)]
public class HeroRuntimeData : RuntimeData
{
    public HeroConfigData ConfigData
    {
        get;
        protected set;
    }
    [JsonProperty]
    public virtual string ConfigKey
    {
        get//序列化时,ConfigKey 的 get 访问器会被调用，返回 ConfigData.key 的值。
        {
            return ConfigData.key;
        }
        set//反序列化时,ConfigKey 的 set 访问器会被调用，使用反序列化后的值来设置 ConfigData。
        {
            ConfigData = Datalib.Instance.GetData<HeroConfigData>(value);
        }
    }
    [JsonProperty]
    public Dictionary<EquipmentType, string> equipmentData = new();
    public HeroRuntimeData() { }
    public HeroRuntimeData(string key)
    {
        ConfigKey = key;
        //装备默认武器
        if (!string.IsNullOrEmpty(ConfigData.defaultWeapon.key))
        {
            equipmentData.Add(EquipmentType.Weapon, ConfigData.defaultWeapon.key);
        }
    }

    private List<OutSideGrowthConfigData> outSideGrowth = null;

    public string SwitchEquipment(EquipmentType type, string key)
    {
        if (equipmentData.ContainsKey(type))
        {
            var oldKey = equipmentData[type];
            equipmentData[type] = key;
            return oldKey;
        }
        else
        {
            equipmentData.Add(type, key);
            return null;
        }
    }
    /// <summary> 计算BaseValue + 天赋数值 </summary>
    public float GetOutSideValue(StatType statType)
    {
        float baseValue = GetBaseValue(statType);
        //计算局外成长数值
        outSideGrowth ??= Datalib.Instance.GetDatas<OutSideGrowthConfigData>();
        float flatValue = 0;
        float percentValue = 1;
        foreach (var growth in outSideGrowth)
        {
            if (growth.excuteStat == statType && growth.talentConfigData != null)
            {
                foreach (var talent in growth.talentConfigData)
                {
                    if (SaveSlotData.Instance.CheckTalent(talent.key))
                    {
                        switch (growth.statCalculatetype)
                        {
                            case BasicModifierType.Flat:
                                flatValue += growth.value;
                                break;
                            case BasicModifierType.Percent:
                                percentValue *= growth.value;
                                break;
                        }
                    }
                }
            }
        }
        return baseValue * percentValue + flatValue;
    }

    // <summary> 计算装备数值 </summary>
    public float GetEquipmentValue(StatType statType)
    {
        float value = 0;
        foreach (var equipment in equipmentData)
        {
            var configData = Datalib.Instance.GetData<EquipmentConfigData>(equipment.Value);
            if (configData == null) continue;
            foreach (var statChange in configData.statChanges)
            {
                if (statChange.statType == statType)
                {
                    value += statChange.value;
                }
            }
        }
        return value;
    }

    public float GetFinalValue(StatType statType)
    {
        return GetOutSideValue(statType) + GetEquipmentValue(statType);
    }

    public float GetBaseValue(StatType statType)
    {
        switch (statType)
        {
            case StatType.HP:
                return ConfigData.hp;
            case StatType.MP:
                return ConfigData.mp;
            case StatType.ATK:
                return ConfigData.atk;
            case StatType.DEF:
                return ConfigData.def;
            case StatType.SPEED:
                return ConfigData.speed;
            case StatType.CRITRATE:
                return ConfigData.critRate;
            case StatType.CRITDAMAGE:
                return ConfigData.critDamage;
            case StatType.HITRATE:
                return ConfigData.hitRate;
        }
        return 0f;
    }

    public List<SkillConfigData> GetSkillList()
    {
        string weaponKey = equipmentData[EquipmentType.Weapon];
        HeroSkillType skillType = Datalib.Instance.GetData<EquipmentConfigData>(weaponKey).heroskillType;
        return ConfigData.skills.FindAll(s => s.heroskillType == skillType);
    }
}
