


using System.Collections.Generic;
using UnityEngine;

public class OutSideGrowthConfigData : ConfigData
{
    // public ModifierTarget target = ModifierTarget.Self;
    [Tooltip("数值计算类型，flat表示固定数值，percent表示乘百分比")]
    public BasicModifierType statCalculatetype = BasicModifierType.Flat;

    [Tooltip("目标属性:对目标哪个属性生效")]
    public StatType excuteStat = StatType.HP;
    public float value = 1;
    [Tooltip("哪些天赋获得这个成长，如果有多个天赋，会触发多次")]
    public List<TalentConfigData> talentConfigData;
}
