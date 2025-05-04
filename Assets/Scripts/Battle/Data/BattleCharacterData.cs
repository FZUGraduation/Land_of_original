
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum BattleActionType
{
    WaitAction = 0,//等待动作
    ActionEnd,//动作结束
    InAction,//正在执行动作
    Dead,//死亡
}
public class BattleCharacterData
{
    public bool IsHero => this is BattleHeroData;
    /// <summary> 每局战斗的character会有一个唯一的ID，用来索引 </summary>
    public readonly int battleID;
    public BaseCharacter characterMono;
    private BattleActionType actionType = BattleActionType.WaitAction;
    public BattleActionType ActionType
    {
        get => actionType;
        set
        {
            actionType = value;
            if (actionType == BattleActionType.Dead)
            {
                //死亡
                // BattleData.Instance.RemoveCharacter(this);
            }
        }
    }
    //用来存储玩家局内数值的类
    protected StatValueRuntimeData hp;
    protected StatValueRuntimeData mp;
    protected StatValueRuntimeData atk;
    protected StatValueRuntimeData def;
    protected StatValueRuntimeData speed;
    protected StatValueRuntimeData critRate;
    protected StatValueRuntimeData critDamage;
    protected StatValueRuntimeData hitRate;
    protected Dictionary<SpecialEffectType, SpecialEffect> specialEffects = new();
    protected Dictionary<StatType, StatValueRuntimeData> statDic = new();
    private Dictionary<string, int> skillCoolDown = new(); //技能冷却时间
    //构造函数
    public BattleCharacterData(int battleID)
    {
        this.battleID = battleID;
        hp = new StatValueRuntimeData(this.battleID);
        mp = new StatValueRuntimeData(this.battleID);
        atk = new StatValueRuntimeData(this.battleID);
        def = new StatValueRuntimeData(this.battleID);
        speed = new StatValueRuntimeData(this.battleID);
        critRate = new StatValueRuntimeData(this.battleID);
        critDamage = new StatValueRuntimeData(this.battleID);
        hitRate = new StatValueRuntimeData(this.battleID);

        statDic[StatType.HP] = hp;
        statDic[StatType.MP] = mp;
        statDic[StatType.ATK] = atk;
        statDic[StatType.DEF] = def;
        statDic[StatType.SPEED] = speed;
        statDic[StatType.CRITRATE] = critRate;
        statDic[StatType.CRITDAMAGE] = critDamage;
        statDic[StatType.HITRATE] = hitRate;
    }

    public StatValueRuntimeData GetStat(StatType statType)
    {
        if (statDic.ContainsKey(statType))
        {
            return statDic[statType];
        }
        return null;
    }

    public void AddSpecialEffect(SpecialEffect specialEffect)
    {
        specialEffects.TryGetValue(specialEffect.type, out SpecialEffect effect);
        if (effect == null)
        {
            specialEffects[specialEffect.type] = specialEffect;
            BattleData.Instance.Emit(BattleData.AddEffect, battleID, specialEffect);
        }
        else
        {
            effect.StayCount = specialEffect.StayCount;//刷新持续时间
        }
    }

    /// <summary> 每回合开始前初始化 </summary>
    public virtual void BeforeAction()
    {
        //!!在 C# 中，不能在遍历集合时直接修改集合的内容，这会导致 InvalidOperationException 异常。
        // foreach (var item in skillCoolDown)
        // {
        //     if (item.Value > 0)
        //     {
        //         skillCoolDown[item.Key]--;
        //     }
        // }
        // 创建一个临时列表存储字典的键
        var keys = new List<string>(skillCoolDown.Keys);

        // 遍历临时列表，安全地修改字典的值
        foreach (var key in keys)
        {
            if (skillCoolDown[key] > 0)
            {
                skillCoolDown[key]--;
            }
        }
    }

    public virtual SkillConfigData GetSkillConfig(string skillName) { return null; }

    public void OnUseSkill(string skillName)
    {
        var skillConfig = GetSkillConfig(skillName);
        if (skillConfig.coolDown > 0)
        {
            skillCoolDown[skillName] = skillConfig.coolDown + 1; //技能冷却时间,加1是因为回合开始前会-1
        }
    }
    public int GetSkillCoolDown(string skillName)
    {
        if (skillCoolDown.ContainsKey(skillName))
        {
            return skillCoolDown[skillName];
        }
        return 0;
    }
    /// <summary> 行动结束时清除持续时间为0的modifier和effect </summary>
    public void OnActionEnd()
    {
        foreach (var stat in statDic.Values)
        {
            stat.ActionEnd();
        }
        // 使用临时列表存储要移除的元素
        List<SpecialEffectType> effectsToRemove = new List<SpecialEffectType>();

        foreach (var kvp in specialEffects)
        {
            var effect = kvp.Value;
            var type = kvp.Key;
            if (effect.StayCount > 0)
            {
                effect.StayCount--;
            }
            if (effect.StayCount == 0)
            {
                effectsToRemove.Add(type);
            }
        }

        // 遍历临时列表并移除元素
        foreach (var type in effectsToRemove)
        {
            BattleData.Instance.Emit(BattleData.RemoveEffect, battleID, specialEffects[type]);
            specialEffects.Remove(type);
        }
    }

    public SpecialEffect GetSpecialEffectValue(SpecialEffectType type)
    {
        specialEffects.TryGetValue(type, out SpecialEffect effect);
        return effect;
    }
}
