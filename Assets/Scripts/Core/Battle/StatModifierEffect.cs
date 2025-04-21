
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;



/// <summary>
/// 属性修改器,用于修改属性数值,buff,debuff等
/// </summary>
public class StatModifierEffect : BaseEffect
{
    // public ModifierTarget target = ModifierTarget.Self;
    [Tooltip("数值计算类型，flat表示固定数值，percent表示乘百分比")]
    public BasicModifierType statCalculatetype = BasicModifierType.Flat;
    [Tooltip("触发时机")]
    public ModifierApplication executeTime = ModifierApplication.InstantAttackOrHeal;


    [ShowIf("statCalculatetype", BasicModifierType.Percent)]
    [Tooltip("来源对象:基于谁的数值计算百分比")]
    public SouceStatType sourceObject = SouceStatType.Self;

    [ShowIf("statCalculatetype", BasicModifierType.Percent)]
    [Tooltip("基于属性:根据来源对象的哪个属性的值作为基础数值,乘以下面的value, eg:比如造成自身攻击的的100%伤害,baseStat要选ATK,sourceObject要选self,excuteStat要选HP,value填-1")]
    public StatType baseStat = StatType.ATK;

    [Tooltip("目标属性:对目标哪个属性生效")]
    public StatType excuteStat = StatType.HP;
    public float value = 1;
    [Tooltip("是否可以暴击")]
    public bool canCrit = true;
    [Tooltip("是否可以计算防御,只有计算伤害时才会需要计算防御")]
    public bool canDef = true;
    [Tooltip("是否可以叠加")]
    public bool canMulti = true;


    [HideInInspector, JsonIgnore]
    public BattleCharacterData sourceData = null;
    // [HideInInspector, JsonIgnore]
    // public BattleCharacterData selfData = null;

    public StatModifierEffect() { }

    //深拷贝构造函数
    public StatModifierEffect(StatModifierEffect other, BattleCharacterData source = null)
    {
        this.StayCount = other.StayCount;
        this.statCalculatetype = other.statCalculatetype;
        this.executeTime = other.executeTime;
        this.excuteStat = other.excuteStat;
        this.value = other.value;
        this.sourceData = source;
        this.canCrit = other.canCrit;
        this.canDef = other.canDef;
        this.icon = other.icon;
        this.effectName = other.effectName;
        this.description = other.description;
        this.canMulti = other.canMulti;
        this.sourceObject = other.sourceObject;
        this.baseStat = other.baseStat;
        this.canDef = other.canDef;
        this.canCrit = other.canCrit;
    }
}
public enum StatType
{
    HP,
    MP,
    ATK,
    DEF,
    SPEED,
    CRITRATE,
    CRITDAMAGE,
    HITRATE,
}
public enum BasicModifierType
{
    Flat = 0,//直接加到属性上,eg：加基础攻击
    Percent = 1,//加到属性的百分比上,eg：加攻击的10%
    // PercentMult = 2,//乘到属性的百分比上,eg：伤害乘以10%
}
public enum SouceStatType
{
    Self,
    Target
}
public enum ModifierApplication
{
    InstantAttackOrHeal,   //直接造成伤害或治疗时，直接触发
    ActionStart,   //行动前触发一次，一般只有伤害和治疗才会有这个
    ActionEnd,    //行动后触发一次，一般只有伤害和治疗才会有这个
    RoundStart,    //回合前触发一次，一般只有伤害和治疗才会有这个
    RoundEnd,     //回合后触发一次，一般只有伤害和治疗才会有这个
    UntilRemoved,   //直到它被移除前一直存在，比如减速的debuff,不会被重复触发
}