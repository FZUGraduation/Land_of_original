
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public class SkillConfigData : ConfigData
{
    [Tooltip("是否是普攻，普攻不会被沉默"), BoxGroup(EXT_BOX_LEFT)]
    public bool isBasicAttack = false; // 是否是普通攻击
    [BoxGroup(EXT_BOX_LEFT)]
    public SkillTarget target = SkillTarget.SingleEnemy; // 技能目标
    [BoxGroup(EXT_BOX_LEFT)]
    public int mpCost = 0; // 技能消耗
    [Tooltip("技能目标自动选择逻辑"), BoxGroup(EXT_BOX_LEFT)]
    public SkillAutoSelectType autoTargetSelectType = SkillAutoSelectType.None; // 技能目标选择类型
    [Tooltip("是否需要通过天赋解锁"), BoxGroup(EXT_BOX_LEFT)]
    public bool needUnlock = false;
    [ShowIf("needUnlock"), BoxGroup(EXT_BOX_LEFT)]
    public TalentConfigData unlockTalent;
    [Tooltip("技能效果")]
    public List<EffectConfigData> effects = new(); // 技能buff、debuff
    [Tooltip("技能冷却时间"), BoxGroup(EXT_BOX_LEFT)]
    public int coolDown = 0;
    [Tooltip("技能是否有释放条件，主要判断自身的属性是否在某个范围内"), BoxGroup(EXT_BOX_LEFT)]
    public bool hasCondition = false; // 是否有释放条件
    [ShowIf("hasCondition"), BoxGroup(EXT_BOX_LEFT)]
    public SkillCondition condition; // 技能释放条件
    public SkillEffectViewData effectView = null; // 技能特效
    public string animationName = ""; // 技能动画名称

    [Tooltip("判断技能释放条件"), BoxGroup(EXT_BOX_LEFT)]
    public bool CheckCondition(BattleCharacterData character, bool checkCondition = true)
    {
        if (character.GetSkillCoolDown(key) > 0) return false;//技能冷却中
        bool isSilent = character.GetSpecialEffectValue(SpecialEffectType.Silent) != null;
        if (isSilent && isBasicAttack == false) return false;//被沉默了

        //如果没有释放条件或者没有条件，或者不需要检查条件，则直接返回true
        if (!hasCondition || condition == null || !checkCondition) return true;
        //开始检查条件
        var stat = character.GetStat(condition.statType);
        float value = condition.isPercenage ? stat.GetPercentage() : stat.BaseValue;
        if (value >= condition.MinValue && value <= condition.MaxValue)
        {
            return true;
        }
        return false;
    }

}

[System.Serializable]
public class SkillCondition
{
    [Tooltip("是否按照百分比计算")]
    public bool isPercenage = true;
    public float MinValue = 0;
    public float MaxValue = 0;
    public StatType statType = StatType.HP;
}

[System.Serializable]
public class SkillEffectViewData
{
    public GameObject effectViewPrefab; // 技能特效预制体
    public EffectViewType effectViewType; // 技能特效类型
    public bool destoryOnEnd = true; // 是否在技能结束后销毁特效
    public bool isLoop = false; // 是否循环播放
}

public enum EffectViewType
{
    None,
    Particle,
    Renderer,
}

public enum SkillTarget
{
    Self,//自己
    SingleHero,//己方单体
    SingleEnemy,//敌方单体
    AllHero,//己方全体
    AllEnemy,//敌方全体
}
public enum SkillAutoSelectType
{
    None,
    MinHp,
    MaxHp,
    Random,
}