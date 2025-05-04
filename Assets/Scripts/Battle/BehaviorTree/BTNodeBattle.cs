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
public class ActionStart : BtPrecondition
{
    public override BehaviourState Tick()
    {
        if (blackboard.boolDir["actionStartNode"])
        {
            return NodeState = ChildNode.Tick();
        }
        blackboard.boolDir["actionStartNode"] = true;
        //判断身上是否有一些特殊状态
        var character = blackboard.objectDir["owner"] as BaseCharacter;
        if (character == null) return NodeState = BehaviourState.失败;

        var battleData = character.GetBattleData();
        if (battleData.GetSpecialEffectValue(SpecialEffectType.Sleep) != null)
        {
            Debug.Log(battleData.battleID + " 被睡眠了，不能行动");
            return NodeState = BehaviourState.失败;
        }

        return NodeState = ChildNode.Tick();
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
    public bool isSkill = false;
    [LabelText("额外延时"), SerializeField, FoldoutGroup("@NodeName")]
    private float elseTimer = 0.1f;
    private float clipLength;
    private float _currentTimer;
    private bool onPlaying = false;
    private AnimatorClipInfo clipInfo;
    public override BehaviourState Tick()
    {
        if (isSkill)
        {
            var character = blackboard.objectDir["owner"] as BaseCharacter;
            var skillConfig = character.GetBattleData().GetSkillConfig(character.GetCurrSkillName());
            if (skillConfig != null)
            {
                animationName = skillConfig.animationName;
                elseTimer = skillConfig.elseAnimTime;
            }
        }
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

[NodeLabel("Goto_直线移动")]
public class Goto : BtActionNode
{
    [LabelText("移动的秒速"), SerializeField, FoldoutGroup("@NodeName"), Range(0, 20)]
    private float speed;
    [LabelText("被移动的"), SerializeField, FoldoutGroup("@NodeName")]
    private Transform move;
    [LabelText("目的地"), SerializeField, FoldoutGroup("@NodeName")]
    private Transform destination;
    [LabelText("停止范围"), SerializeField, FoldoutGroup("@NodeName")]
    private float failover;
    public override BehaviourState Tick()
    {
        var character = blackboard.objectDir["owner"] as BaseCharacter;

        if (!move)
        {
            move = character.transform;
        }
        if (!destination)
        {
            var skillConfig = character.GetBattleData().GetSkillConfig(character.GetCurrSkillName());
            if (skillConfig != null && !skillConfig.needGotoTargetPos)
            {
                return NodeState = BehaviourState.成功;//如果不需要移动，则直接返回成功
            }
            blackboard.objectDir["startPosition"] = move.position;
            blackboard.objectDir["startRotation"] = move.rotation;
            var animator = blackboard.objectDir["animator"] as Animator;
            animator.Play("Run");
            int targetID = (character.GetBattleData() as BattleEnemyData).currTargets[0];
            var target = BattleData.Instance.GetCharacterData(targetID).characterMono.transform;
            if (target == null) return NodeState = BehaviourState.失败;
            destination = target;
            character.transform.LookAt(destination.position);
        }
        if (Vector3.Distance(move.position, destination.position) < failover)
        {
            destination = null;
            return NodeState = BehaviourState.成功;
        }
        var v = (destination.position - move.position).normalized;
        move.position += v * speed * Time.deltaTime;
        return NodeState = BehaviourState.执行中;
    }
}

[NodeLabel("GoBack_对应Goto")]
public class GotBack : BtActionNode
{
    public float jumpHeight = 2f; // 跳跃高度
    public float jumpDuration = 1f; // 跳跃时间
    private bool isInit = false;
    private bool isJumping = false;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsedTime = 0f;
    public override BehaviourState Tick()
    {
        var character = blackboard.objectDir["owner"] as BaseCharacter;
        var animator = blackboard.objectDir["animator"] as Animator;
        if (!isInit)
        {
            var skillConfig = character.GetBattleData().GetSkillConfig(character.GetCurrSkillName());
            if (skillConfig != null && !skillConfig.needGotoTargetPos)
            {
                return NodeState = BehaviourState.成功;//如果不需要移动，则直接返回成功
            }

            isInit = true;
            animator.Play("Jump");
            elapsedTime = 0f;
            startPosition = character.transform.position;
            targetPosition = (Vector3)blackboard.objectDir["startPosition"];
            isJumping = true;
        }
        if (isJumping)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpDuration;

            // 计算水平位置
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, t);

            // 计算垂直位置（抛物线）
            float verticalOffset = jumpHeight * Mathf.Sin(Mathf.PI * t);

            // 更新物体位置
            character.transform.position = horizontalPosition + Vector3.up * verticalOffset;

            // 结束跳跃
            if (t >= 1f)
            {
                isJumping = false;
            }
        }
        if (!isJumping)
        {
            isInit = false;
            animator.Play("Idle");
            character.transform.position = targetPosition;
            character.transform.rotation = (Quaternion)blackboard.objectDir["startRotation"];
            return NodeState = BehaviourState.成功;
        }
        else
        {
            return NodeState = BehaviourState.执行中;
        }
    }
}

[NodeLabel("SetActiveSelf,设置激活状态")]
public class SetActiveSelf : BtActionNode
{
    [LabelText("是否激活"), SerializeField, FoldoutGroup("@NodeName")]
    private bool isActive = true;
    public override BehaviourState Tick()
    {
        var character = blackboard.objectDir["owner"] as BaseCharacter;
        character.gameObject.SetActive(isActive);
        return NodeState = BehaviourState.成功;
    }
}
