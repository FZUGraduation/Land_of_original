
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

public class BattleEnemyData : BattleCharacterData
{
    public EnemyConfigData enemyConfig;
    public int position;
    public string currSkillName = ""; // 当前使用的技能名称
    public BattleEnemyData(EnemyConfigData enemyConfig, int position, int battleID) : base(battleID)
    {
        this.enemyConfig = enemyConfig;
        this.position = position;
        //将玩家局外数值赋值给玩家局内数值
        hp.InitValue(enemyConfig.hp, 0, enemyConfig.hp);
        mp.InitValue(enemyConfig.mp);
        atk.InitValue(enemyConfig.atk);
        def.InitValue(enemyConfig.def);
        speed.InitValue(enemyConfig.speed);
        critRate.InitValue(enemyConfig.critRate);
        critDamage.InitValue(enemyConfig.critDamage);
        hitRate.InitValue(enemyConfig.hitRate);
    }
    public override SkillConfigData GetSkillConfig(string skillName)
    {
        return enemyConfig.skills.Find(s => s.key == skillName);
    }
    public override SkillConfigData GetSkillConfig(int skillIndex)
    {
        if (skillIndex < 0 || skillIndex >= enemyConfig.skills.Count)
        {
            return null;
        }
        return enemyConfig.skills[skillIndex];
    }

    public override void BeforeAction()
    {
        base.BeforeAction();
        //回合开始前获得当前回合使用的技能名称
        currSkillName = GetUseSkillName();
    }

    /// <summary> 获取当前回合使用的技能名称 </summary>
    private string GetUseSkillName()
    {
        List<SkillConfigData> skillList = new();
        foreach (var item in enemyConfig.skills)
        {
            if (item.CheckCondition(this)) skillList.Add(item);
        }
        if (skillList.Count == 0)
        {
            //如果没有技能满足条件，则判断默认技能能不能使用
            if (enemyConfig.defaultSkillIndex >= 0 && enemyConfig.defaultSkillIndex < enemyConfig.skills.Count)
            {
                if (enemyConfig.skills[enemyConfig.defaultSkillIndex].CheckCondition(this, false))
                {
                    return enemyConfig.skills[enemyConfig.defaultSkillIndex].key;
                }
            }
        }
        else
        {
            //如果有技能满足条件，则随机选择一个技能
            int randomIndex = Random.Range(0, skillList.Count);
            return skillList[randomIndex].key;
        }
        return null;
    }
}
