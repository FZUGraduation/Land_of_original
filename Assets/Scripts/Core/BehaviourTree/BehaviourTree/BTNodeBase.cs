using System;
using System.Collections.Generic;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JLBehaviourTree.BehaviourTree
{

    /// <summary>
    /// 顺序节点，按顺序执行子节点，全部成功返回成功，任一失败立即终止。
    /// 示例：NPC开门行为 → 走到门前（成功）→ 转动门把手（成功）→ 推门（成功）。 </summary>
    [NodeLabel("Sequence_顺序节点")]
    public class Sequence : BtComposite
    {
        [LabelText("运行字节点"), FoldoutGroup("@NodeName"), ReadOnly]
        public int _index;
        [FoldoutGroup("@NodeName"), ReadOnly, TextArea]
        public string desc = "顺序节点，按顺序执行子节点，全部成功返回成功，任一失败立即终止。";

        public override BehaviourState Tick()
        {
            if (ChildNodes.Count == 0)
            {
                NodeState = BehaviourState.失败;
                return BehaviourState.失败;
            }


            var state = ChildNodes[_index].Tick();
            NodeState = state;
            switch (state)
            {
                case BehaviourState.成功:
                    _index++;
                    if (_index >= ChildNodes.Count)
                    {
                        _index = 0;
                    }
                    return BehaviourState.成功;
                case BehaviourState.失败:
                    _index = 0;//重置,也可以不重置，都行
                    return BehaviourState.失败;
                case BehaviourState.执行中:
                    return state;
            }
            return BehaviourState.未执行;
        }
    }
    /// <summary>
    /// 选择节点，按顺序执行子节点，直到找到一个成功的子节点后停止。
    /// 示例：敌人AI决策 → 攻击玩家（失败）→ 寻找掩体（成功）→ 停止后续行为。 </summary>
    [NodeLabel("Selector_选择节点")]
    public class Selector : BtComposite
    {
        [LabelText("运行字节点"), FoldoutGroup("@NodeName"), ReadOnly]
        public int _index;
        [FoldoutGroup("@NodeName"), ReadOnly, TextArea]
        public string desc = "选择节点，按顺序执行子节点，直到找到一个成功的子节点后停止。";
        public override BehaviourState Tick()
        {
            if (ChildNodes.Count == 0)
            {
                // ChangeFailState();
                return NodeState;
            }

            var selectState = ChildNodes[_index].Tick();
            NodeState = selectState;
            switch (selectState)
            {
                case BehaviourState.成功:
                    _index = 0;
                    return selectState;
                case BehaviourState.失败:
                    // ChangeFailState();
                    _index++;
                    if (_index >= ChildNodes.Count)
                    {
                        _index = 0;
                        return BehaviourState.失败;
                    }
                    break;
                default:
                    return selectState;
            }

            // for (int i = 0; i < ChildNodes.Count; i++)
            // {
            //     var state = ChildNodes[i].Tick();
            //     if (state == BehaviourState.失败 || _index == i) continue;
            //     _index = i;
            //     NodeState = state;
            //     return state;
            // }
            // ChangeFailState();
            return BehaviourState.失败;
        }
    }

    /// <summary>
    /// 并行节点，同时执行所有子节点，可配置成功/失败条件（如全部成功、多数成功等）。
    /// 示例：边移动边射击，同时监测玩家是否进入视野。 </summary>
    [NodeLabel("Parallel_并行节点")]
    public class Parallel : BtComposite
    {
        [FoldoutGroup("@NodeName"), ReadOnly, TextArea]
        public string desc = "并行节点，同时执行所有子节点，可配置成功/失败条件（如全部成功、多数成功等）。";
        public override BehaviourState Tick()
        {
            List<BehaviourState> states = new();
            for (int i = 0; i < ChildNodes.Count; i++)
            {
                var state = ChildNodes[i].Tick();
                switch (state)
                {
                    case BehaviourState.失败:
                        // ChangeFailState();
                        NodeState = BehaviourState.失败;
                        return NodeState;
                    default:
                        states.Add(state);
                        break;
                }
            }

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] == BehaviourState.执行中)
                {
                    NodeState = BehaviourState.执行中;
                    return BehaviourState.执行中;
                }
            }

            NodeState = BehaviourState.成功;
            return BehaviourState.成功;
        }

    }

    [NodeLabel("Repeat_循环执行")]
    public class Repeat : BtPrecondition
    {
        [LabelText("循环次数"), FoldoutGroup("@NodeName")]
        public int LoopNumber;
        [LabelText("循环停止数"), FoldoutGroup("@NodeName")]
        public int LoopStop;
        public override BehaviourState Tick()
        {
            var state = ChildNode.Tick();
            if (LoopStop <= LoopNumber)
            {
                LoopNumber = 0;
                NodeState = BehaviourState.成功;
                return BehaviourState.成功;
            }
            LoopNumber++;

            if (state == BehaviourState.失败)
            {
                // ChangeFailState();
                NodeState = BehaviourState.失败;
            }
            else
            {
                NodeState = BehaviourState.执行中;
            }
            return NodeState;
        }
    }

    /// <summary> 满足条件执行 </summary>
    [NodeLabel("So_满足bool条件执行")]
    public class So : BtPrecondition
    {
        [LabelText("执行条件"), FoldoutGroup("@NodeName")]
        public Func<bool> Condition;
        [LabelText("执行条件"), FoldoutGroup("@NodeName"), SerializeField]
        public bool active;
        public override BehaviourState Tick()
        {
            if (Condition == null)
            {
                // ChangeFailState();
                if (active)
                {
                    NodeState = ChildNode.Tick();
                    return NodeState;
                }
            }
            else if (Condition.Invoke())
            {
                NodeState = ChildNode.Tick();
                return NodeState;
            }
            // ChangeFailState();
            NodeState = BehaviourState.失败;
            return BehaviourState.失败;
        }
    }

    /// <summary> 满足对应字符串条件执行 </summary>
    [NodeLabel("So_Str满足string条件执行")]
    public class So_Str : BtPrecondition
    {
        [LabelText("执行条件"), FoldoutGroup("@NodeName")]
        public Func<string> Condition;
        [LabelText("所需字符串"), FoldoutGroup("@NodeName")]
        public string str;
        public override BehaviourState Tick()
        {
            if (Condition != null)
            {
                if (Condition.Invoke() == str)
                {
                    NodeState = ChildNode.Tick();
                    return NodeState;
                }
            }
            else
            {
                Debug.Log("条件为NULL");
            }
            // ChangeFailState();
            NodeState = BehaviourState.失败;
            return BehaviourState.失败;
        }
    }

    [NodeLabel("So_Blackboard_满足黑板Bool条件执行")]
    public class So_Blackboard : BtPrecondition
    {
        [LabelText("执行条件"), FoldoutGroup("@NodeName")]
        public string Key;
        public override BehaviourState Tick()
        {
            if (blackboard.boolDir.ContainsKey(Key) && blackboard.boolDir[Key])
            {
                NodeState = ChildNode.Tick();
                return NodeState;
            }
            return BehaviourState.失败;
        }
    }

    [NodeLabel("Not_不满足条件执行")]
    public class Not : BtPrecondition
    {
        [LabelText("执行条件"), FoldoutGroup("@NodeName")]
        public Func<bool> Condition;
        public override BehaviourState Tick()
        {
            if (Condition == null)
            {
                // ChangeFailState();
                return BehaviourState.失败;
            }

            if (!Condition.Invoke())
            {
                NodeState = ChildNode.Tick();
                return NodeState;
            }
            // ChangeFailState();
            return BehaviourState.失败;
        }
    }

    [NodeLabel("Delay_延时节点")]
    public class Delay : BtPrecondition
    {
        [LabelText("延时"), SerializeField, FoldoutGroup("@NodeName")]
        private float timer;

        private float _currentTimer;
        public override BehaviourState Tick()
        {
            _currentTimer += Time.deltaTime;
            if (_currentTimer >= timer)
            {
                _currentTimer = 0f;
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
            return NodeState = BehaviourState.执行中;
        }
    }

    [NodeLabel("Base_SetActive_启用禁用")]
    public class SetObjectActive : BtActionNode
    {
        [LabelText("是否启用"), SerializeField, FoldoutGroup("@NodeName")]
        private bool _isActive;
        [LabelText("启用对象"), SerializeField, FoldoutGroup("@NodeName")]
        private GameObject _particle;

        public override BehaviourState Tick()
        {
            if(!_particle) return NodeState = BehaviourState.失败;
            _particle.SetActive(_isActive);
            Debug.Log(NodeName + (_isActive ? "启用" : "禁用") + "了" + _particle.name);
            return NodeState = BehaviourState.成功;
        }
    }

    [NodeLabel("Set_Blackboard_设置黑板bool值")]
    public class Set_Blackboard : BtActionNode
    {
        [LabelText("黑板Key"), SerializeField, FoldoutGroup("@NodeName")]
        private string _key;
        [LabelText("黑板值"), SerializeField, FoldoutGroup("@NodeName")]
        private bool _value;

        public override BehaviourState Tick()
        {
            blackboard.boolDir[_key] = _value;
            Debug.Log(NodeName + "设置了" + _key + "为" + _value);
            return NodeState = BehaviourState.成功;
        }
    }

    // public class Run : BtActionNode
    // {
    //     public override BehaviourState Tick()
    //     {
    //         Running();
    //         NodeState = BehaviourState.成功;
    //         return BehaviourState.成功;
    //     }

    //     public void Running()
    //     {
    //         Debug.Log(NodeName + " 节点执行了！");
    //     }
    // }

    // public class Running : BtActionNode
    // {
    //     [LabelText("执行进度"), FoldoutGroup("@NodeName")]
    //     public float Schedule;
    //     public override BehaviourState Tick()
    //     {
    //         if (Schedule >= 0.6f)
    //         {
    //             Schedule = 0;
    //             Debug.Log(NodeName + " 节点任务完成");
    //             NodeState = BehaviourState.成功;
    //             return BehaviourState.成功;
    //         }
    //         Schedule += Time.deltaTime;
    //         //Debug.Log(NodeName+ " 节点进度： "+Schedule*20+"%");
    //         NodeState = BehaviourState.执行中;
    //         return BehaviourState.执行中;
    //     }
    // }

    // [NodeLabel("Goto_直线移动")]
    // public class Goto : BtActionNode
    // {
    //     [LabelText("移动的秒速"), SerializeField, FoldoutGroup("@NodeName"), Range(0, 20)]
    //     private float speed;
    //     [LabelText("被移动的"), SerializeField, FoldoutGroup("@NodeName")]
    //     private Transform move;
    //     [LabelText("目的地"), SerializeField, FoldoutGroup("@NodeName")]
    //     private Transform destination;
    //     [LabelText("停止范围"), SerializeField, FoldoutGroup("@NodeName")]
    //     private float failover;
    //     public override BehaviourState Tick()
    //     {
    //         if (!move || !destination)
    //         {
    //             // ChangeFailState();
    //             return NodeState;
    //         }
    //         if (Vector3.Distance(move.position, destination.position) < failover)
    //         {
    //             return NodeState = BehaviourState.成功;
    //         }
    //         var v = (destination.position - move.position).normalized;
    //         move.position += v * speed * Time.deltaTime;
    //         return NodeState = BehaviourState.执行中;
    //     }
    // }

    // [NodeLabel("DiscoveryTarget_发现目标")]
    // public class DiscoveryTarget : BtPrecondition
    // {
    //     [LabelText("自己"), SerializeField, FoldoutGroup("@NodeName")]
    //     private Transform _self;
    //     [LabelText("目标"), SerializeField, FoldoutGroup("@NodeName")]
    //     private Transform _destination;
    //     [LabelText("发现距离"), SerializeField, FoldoutGroup("@NodeName")]
    //     private float _warnRange;

    //     public override BehaviourState Tick()
    //     {
    //         if (!_destination.gameObject.activeSelf)
    //         {
    //             // ChangeFailState();
    //             return NodeState;
    //         }
    //         var distance = Vector3.Distance(_self.position, _destination.position);
    //         if (distance <= _warnRange)
    //         {
    //             ChildNode.Tick();
    //             return NodeState = BehaviourState.执行中;
    //         }
    //         else
    //         {
    //             // ChangeFailState();
    //             return NodeState;
    //         }
    //     }
    // }

    // [NodeLabel("AnimatorPlay_播放动画")]
    // public class AnimatorPlay : BtActionNode
    // {
    //     [LabelText("名称"), SerializeField, FoldoutGroup("@NodeName")]
    //     private string animationName;
    //     [LabelText("动画"), SerializeField, FoldoutGroup("@NodeName")]
    //     private Animator animator;

    //     public override BehaviourState Tick()
    //     {
    //         if (!animator)
    //         {
    //             // ChangeFailState();
    //             Debug.LogError("没有找到动画组件");
    //             NodeState = BehaviourState.失败;
    //             return NodeState;
    //         }
    //         animator.Play(animationName);
    //         return NodeState = BehaviourState.成功;
    //     }
    // }
}