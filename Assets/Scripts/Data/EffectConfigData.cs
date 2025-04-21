using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
[System.Serializable]
public class EffectConfigData : ConfigData
{
    [Tooltip("效果类型-数值效果或特殊效果"), OnValueChanged("OnEffectTypeChange"), BoxGroup(EXT_BOX_LEFT, LabelText = EXT1_CONFIG_TITLE)]
    public EffectType effectType = EffectType.None; // 
    [Tooltip("数值相关效果"), ShowIf("effectType", EffectType.StatModifuer)]
    public StatModifierEffect statModifier; // 数值效果
    [Tooltip("特殊效果"), ShowIf("effectType", EffectType.SpecialEffect)]
    public SpecialEffect specialEffect; // 特殊效果

    public void OnEffectTypeChange()
    {
        if (effectType == EffectType.StatModifuer)
        {
            specialEffect = null;
        }
        else if (effectType == EffectType.SpecialEffect)
        {
            statModifier = null;
        }
        else
        {
            statModifier = null;
            specialEffect = null;
        }
    }
}
public enum EffectType
{
    None,
    StatModifuer,
    SpecialEffect,
}