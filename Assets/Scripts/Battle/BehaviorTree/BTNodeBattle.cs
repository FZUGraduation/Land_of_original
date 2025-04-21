using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using UnityEngine;
//战斗黑板数据
/*
inSkill->是否在使用技能
inAction->是否在行动中
actionStartNode->是否执行过ActionStart节点
getHurtNode->是否受伤了
*/
/*
owner->BaseCharacter
animator->Animator
*/
[NodeLabel("Battle_ActionStart_行动开始")]
public class ActionStart : BtActionNode
{
    public override BehaviourState Tick()
    {
        if (blackboard.boolDir["ActionStart"])
        {
            return NodeState = BehaviourState.成功;
        }
        blackboard.boolDir["actionStartNode"] = true;
        //判断身上是否有一些特殊状态
        var character = blackboard.objectDir["owner"] as BaseCharacter;
        return NodeState = BehaviourState.成功;
    }
}

[NodeLabel("Battle_ActionEnd_行动结束")]
public class ActionEnd : BtActionNode
{
    public override BehaviourState Tick()
    {
        blackboard.boolDir["inAction"] = false;
        blackboard.boolDir["inSkill"] = false;
        blackboard.boolDir["actionStartNode"] = false;
        BattleData.Instance.Emit(BattleData.ActionNext);
        return NodeState = BehaviourState.成功;
    }
}
[NodeLabel("Battle_So_Skill_判断技能,用于hero")]
public class So_Skill : BtPrecondition
{
    [FoldoutGroup("@NodeName"), LabelText("技能index"), Range(0, 3)]
    public int skillIndex = 0;
    private SkillConfigData skillConfig = null;
    private bool isInit = false;
    public override BehaviourState Tick()
    {
        if (isInit && skillConfig == null) return NodeState = BehaviourState.失败;
        var character = blackboard.objectDir["owner"] as BaseCharacter;
        if (!isInit)
        {
            isInit = true;

            skillConfig = BattleData.Instance.GetCharacterData(character.BattleID).GetSkillConfig(skillIndex);
            if (skillConfig == null) return NodeState = BehaviourState.失败;
        }
        if (character is BaseHero hero)
        {
            //如果这个节点是对应技能的节点，则执行
            if (hero.GetCurrSkillName() == skillConfig.key)
            {
                NodeState = ChildNode.Tick();
                return NodeState;
            }
        }
        return NodeState = BehaviourState.失败;
    }
}

[NodeLabel("Battle_SkillExcute_执行技能")]
public class SkillExcute : BtActionNode
{
    // [FoldoutGroup("@NodeName"), LabelText("指定技能(Enemy)"), Range(0, 3), Tooltip("指定技能的index,Enemy才会用到这个参数")]
    // public int skillIndex = 0;
    // private SkillConfigData skillConfig = null;
    public override BehaviourState Tick()
    {
        var character = blackboard.objectDir["owner"];

        switch (character)
        {
            case BaseHero hero:
                BattleData.Instance.Emit(BattleData.SkillExcute, hero.BattleID, hero.GetCurrSkillName(), hero.SkillTargetBattleId);
                break;
            case BaseEnemy enemy:
                var enemyData = enemy.GetBattleData() as BattleEnemyData;
                if (!string.IsNullOrEmpty(enemyData.currSkillName))
                {
                    BattleData.Instance.Emit(BattleData.SkillExcute, enemy.BattleID, enemyData.currSkillName, -1);
                }
                // if (skillConfig == null)
                // {
                //     skillConfig = enemyData.GetSkillConfig(skillIndex);
                //     if (skillConfig == null) return NodeState = BehaviourState.失败;
                // }
                // //如果被沉默了,则不能释放技能
                // if (enemyData.GetSpecialEffectValue(SpecialEffectType.Silent) != null && skillConfig.isBasicAttack == false)
                // {
                //     return NodeState = BehaviourState.失败;
                // }
                break;
        }
        return NodeState = BehaviourState.成功;
    }
}

[NodeLabel("Battle_是否满足角色状态条件")]
public class CharacterStatu : BtPrecondition
{
    [Tooltip("是否是计算百分比, 如果按照百分比计算下面取0-1")]
    public bool isPercenage = false;
    public float MinValue = 0;
    public float MaxValue = 0;
    public StatType statType = StatType.HP;
    public override BehaviourState Tick()
    {
        var character = blackboard.objectDir["owner"] as BaseCharacter;
        if (character == null) return NodeState = BehaviourState.失败;
        var stat = character.GetStat(statType);
        float value = isPercenage ? stat.GetPercentage() : stat.BaseValue;
        if (value >= MinValue && value <= MaxValue)
        {
            NodeState = ChildNode.Tick();
            return NodeState;
        }
        return NodeState = BehaviourState.失败;
    }
}

[NodeLabel("Animation_等待动画执行")]
public class WaitAnimation : BtPrecondition
{
    [FoldoutGroup("@NodeName"), LabelText("动画名称")]
    public string animationName;
    [LabelText("额外延时"), SerializeField, FoldoutGroup("@NodeName")]
    private float elseTimer = 0.1f;
    private float clipLength;
    private float _currentTimer;
    private bool onPlaying = false;
    private AnimatorClipInfo clipInfo;
    public override BehaviourState Tick()
    {
        if (string.IsNullOrEmpty(animationName))
        {
            Debug.Log("animationName is null or empty");
            if (ChildNode != null)
            {
                NodeState = ChildNode.Tick();
            }
            else
            {
                NodeState = BehaviourState.成功;
            }
            return NodeState;
        }

        if (!onPlaying)
        {
            var animator = blackboard.objectDir["animator"] as Animator;
            if (animator == null)
            {
                Debug.LogError("animator is null");
                return NodeState = BehaviourState.失败;
            }
            animator.Play(animationName);
            onPlaying = true;
        }
        else
        {
            if (clipInfo.clip == null)
            {
                var animator = blackboard.objectDir["animator"] as Animator;
                clipInfo = animator.GetCurrentAnimatorClipInfo(0)[0];
                clipLength = clipInfo.clip.length;
                // Debug.Log(animationName + " clipLength:" + clipLength);
            }
            _currentTimer += Time.deltaTime;
            if (_currentTimer >= clipLength + elseTimer)
            {
                _currentTimer = 0f;
                onPlaying = false;
                if (ChildNode != null)
                {
                    NodeState = ChildNode.Tick();
                }
                else
                {
                    NodeState = BehaviourState.成功;
                }
                return NodeState;
            }
        }
        return NodeState = BehaviourState.执行中;
    }
}