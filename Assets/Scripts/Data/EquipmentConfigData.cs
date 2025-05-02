using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EquipmentConfigData : ItemConfigData
{
    EquipmentConfigData()
    {
        category = ItemCategory.Equipment;
    }

    [VerticalGroup("TDSplit/LRSplit/Left/General Settings/Split/Right")]
    [LabelText("模型")]
    public GameObject modelPrefab;
    [BoxGroup(STATS_BOX, LabelText = "装备类型")]
    public EquipmentType equipmentType = EquipmentType.None;
    [BoxGroup(STATS_BOX, LabelText = "数值提升")]
    public List<StatChange> statChanges = new();
    [BoxGroup(STATS_BOX, LabelText = "装备路径")]
    public List<string> path;
}

public enum EquipmentType
{
    None = 0,
    Head,
    Body,
    Legs,
    Back,//披风
    Weapon,
    Follower,
}

public class StatChange
{
    public StatType statType;
    public float value;
}