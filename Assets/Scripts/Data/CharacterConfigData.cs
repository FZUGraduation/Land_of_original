
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterConfigData : ConfigData
{
    [VerticalGroup("TDSplit/LRSplit/Left/General Settings/Split/Right")]
    [LabelText("模型")]
    public GameObject modelPrefab;
    [VerticalGroup("TDSplit/LRSplit/Left/General Settings/Split/Right")]
    public GameObject worldPrefab;
    [VerticalGroup("TDSplit/LRSplit/Left/General Settings/Split/Right")]
    public Sprite bodyicon;
    [BoxGroup(STATS_BOX, LabelText = "数值属性"), MinValue(1f)]
    public float hp = 1;
    [BoxGroup(STATS_BOX), MinValue(0f)]
    public float mp = 0;
    [BoxGroup(STATS_BOX), Tooltip("攻击"), MinValue(0f)]
    public float atk = 0;
    [BoxGroup(STATS_BOX), Tooltip("防御力"), MinValue(0f)]
    public float def = 0;
    [BoxGroup(STATS_BOX), Tooltip("速度 范围100-200"), Range(100f, 200f)]
    public float speed = 100;//速度
    [BoxGroup(STATS_BOX), Tooltip("暴击率 范围0-1"), Range(0, 1)]
    public float critRate = 0;//暴击率
    [BoxGroup(STATS_BOX), Tooltip("暴击伤害 范围1.5倍-2倍"), Range(1.5f, 2f)]
    public float critDamage = 1.5f;//暴击伤害
    [BoxGroup(STATS_BOX), Tooltip("命中率 范围0-1"), Range(0, 1)]
    public float hitRate = 0.2f;//命中率

    public List<SkillConfigData> skills = new();
    [Tooltip("默认技能,所有技能条件不满足时候使用的技能")]
    public int defaultSkillIndex = 0; // 默认技能索引
}
