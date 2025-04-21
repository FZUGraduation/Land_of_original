using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JLBehaviourTree.BehaviourTree
{
    public enum BehaviourState
    {
        未执行, 成功, 失败, 执行中
    }

    public enum NodeType
    {
        无, 根节点, 组合节点, 条件节点, 行为节点
    }

    public class BlackBoard
    {
        public Dictionary<string, bool> boolDir = new();
        public Dictionary<string, object> objectDir = new();
    }

    #region 根数据
    [BoxGroup]
    [HideLabel]
    [HideReferenceObjectPicker]
    public abstract class BtNodeBase
    {
        [TextArea, FoldoutGroup("@NodeName"), LabelText("备注")]
        public string nodeDes;
        [HideInInspector]
        public string Guid = "";
        [HideInInspector]
        public Vector2 Position;
        [ReadOnly, FoldoutGroup("@NodeName"), LabelText("类型")]
        public NodeType NodeType;
        [ReadOnly, FoldoutGroup("@NodeName"), LabelText("状态")]
        public BehaviourState NodeState;
        [ReadOnly, FoldoutGroup("@NodeName"), LabelText("类名")]
        public string className;
        [FoldoutGroup("@NodeName"), LabelText("节点名称")]
        public string NodeName;

        /// <summary> 黑板模式,存储信息，整个行为树的节点公用 </summary>
        protected BlackBoard blackboard = null;
        protected BtNodeBase()
        {
            Guid = System.Guid.NewGuid().ToString();
            className = GetType().Name;
        }

        public abstract BehaviourState Tick();


        protected void ChangeFailState()
        {
            NodeState = BehaviourState.失败;
            switch (this)
            {
                case BtComposite composite:
                    composite.ChildNodes.ForEach(node => node.ChangeFailState());
                    break;
                case BtPrecondition precondition:
                    precondition.ChildNode?.ChangeFailState();
                    break;
            }
        }

        public void InitBlackboard(BlackBoard blackboard)
        {
            this.blackboard = blackboard;
            switch (this)
            {
                case BtComposite composite:
                    composite.ChildNodes.ForEach(node => node.InitBlackboard(blackboard));
                    break;
                case BtPrecondition precondition:
                    precondition.ChildNode?.InitBlackboard(blackboard);
                    break;
            }
        }
    }

    /// <summary> 组合节点 </summary>
    public abstract class BtComposite : BtNodeBase
    {
        [FoldoutGroup("@NodeName"), LabelText("子节点")]
        public List<BtNodeBase> ChildNodes = new List<BtNodeBase>();

        protected BtComposite() : base()
        {
            NodeType = NodeType.组合节点;
        }
    }

    /// <summary> 条件节点,只有一个子节点,检查某个条件是否满足（如“血量<30%”），返回成功/失败。 </summary>
    public abstract class BtPrecondition : BtNodeBase
    {
        [FoldoutGroup("@NodeName"), LabelText("子节点")]
        public BtNodeBase ChildNode;

        protected BtPrecondition() : base()
        {
            NodeType = NodeType.条件节点;
        }
    }

    /// <summary> 行为节点，执行具体行为（如移动、攻击），完成后返回成功/失败。 </summary>
    public abstract class BtActionNode : BtNodeBase
    {
        protected BtActionNode() : base()
        {
            NodeType = NodeType.行为节点;
        }
    }
    #endregion

}
