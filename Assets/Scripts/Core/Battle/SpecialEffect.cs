
using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

public class SpecialEffect : BaseEffect
{
    public SpecialEffectTriggerType executeTime = SpecialEffectTriggerType.InstantTrigger;
    public SpecialEffectType type = SpecialEffectType.None;
    [Tooltip("有些效果可能需要一个数值")]
    public int value = 0;
    [HideInInspector, JsonIgnore]
    public BattleCharacterData sourceData = null;
    public SpecialEffect() { }

    //深拷贝构造函数
    public SpecialEffect(SpecialEffect other, BattleCharacterData source = null)
    {
        this.StayCount = other.StayCount;
        this.executeTime = other.executeTime;
        this.type = other.type;
        this.sourceData = source;
        this.icon = other.icon;
        this.effectName = other.effectName;
        this.description = other.description;
        this.StayCount = other.StayCount;
    }
}
public enum SpecialEffectType
{
    None, //无效果
    Sleep, //睡眠
    Silent, //沉默
}

public enum SpecialEffectTriggerType
{
    InstantTrigger,   //比如净化debuff，立即触发,不需要显示在玩家的状态条上
    UntilRemoved,   //直到它被移除前一直存在，比如冰冻，眩晕这种可以存在几个回合的debuff
}