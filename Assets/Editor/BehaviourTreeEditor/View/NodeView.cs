using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.Editor.EditorExTools;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace JLBehaviourTree.Editor.View
{
    public class NodeView : Node
    {
        [LabelText("节点视图数据"), OdinSerialize, HideReferenceObjectPicker]
        public BtNodeBase NodeViewData;

        #region 连接数据
        [LabelText("输入端口"), HideIf("@true")] public Port InputPort;
        [LabelText("输出端口"), HideIf("@true")] public Port OutputPort;
        #endregion

        #region 节点样式
        //背景
        private VisualElement _nodeBorderBar;
        //标题背景
        private VisualElement _nodeTitleBar;
        //标题
        private Label _titleLabel;

        #endregion

        public NodeView(BtNodeBase NodeViewData) : base(
            "Assets/Resources/Core/BehaviourTree/NodeView.uxml")
        {
            this.NodeViewData = NodeViewData;
            InitNodeView();
            InitNodeStyleData();
        }
        // public NodeView(BtNodeBase NodeViewData)
        // {
        //     this.NodeViewData = NodeViewData;
        //     InitNodeView();
        //     InitNodeStyleData();
        // }

        /// <summary>初始化节点视图</summary>
        private void InitNodeView()
        {
            title = NodeViewData.NodeName;
            //创建节点端口
            switch (NodeViewData.NodeType)
            {
                case NodeType.组合节点:
                    LoadCompositeNode();
                    break;
                case NodeType.条件节点:
                    LoadPreconditionNode();
                    break;
                case NodeType.行为节点:
                    LoadActionNode();
                    break;
            }

        }
        private void LoadCompositeNode()
        {
            InputPort = PortView.Create<Edge>();
            OutputPort = PortView.Create<Edge>(dir: Direction.Output, cap: Port.Capacity.Multi);
            inputContainer.Add(InputPort);//添加输入端口
            outputContainer.Add(OutputPort);//添加输出端口
        }

        private void LoadActionNode()
        {
            InputPort = PortView.Create<Edge>();
            inputContainer.Add(InputPort);
        }

        private void LoadPreconditionNode()
        {
            InputPort = PortView.Create<Edge>();
            OutputPort = PortView.Create<Edge>(dir: Direction.Output, cap: Port.Capacity.Single);
            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);
        }

        private async void InitNodeStyleData()
        {
            await UniTask.Delay(50);
            _nodeBorderBar = this.Q<VisualElement>("node-border");
            _nodeTitleBar = this.Q<VisualElement>("title");
            _titleLabel = this.Q<Label>("title-label");
            ChangeBgColor(BehaviourTreeSetting.GetSetting().GetNodeBgColor(NodeViewData));
            ChangeTitleColor(BehaviourTreeSetting.GetSetting().GetNodeTitleColor(NodeViewData));
        }

        /// <summary> 查询子节点并与子节点连接线条 </summary>
        public void LinkChildNode()
        {
            // await UniTask.DelayFrame(1);
            TreeView view = BehaviourTreeView.Instance.WindowRoot.TreeView;
            switch (NodeViewData)
            {
                case BtComposite composite:
                    //从自己的输出端口连接到所有子节点的输入端口
                    foreach (var t in composite.ChildNodes)
                    {
                        LinkNodes(OutputPort, view.NodeViews[t.Guid].InputPort);
                    }
                    break;
                case BtPrecondition precondition:
                    if (precondition.ChildNode == null) break;
                    LinkNodes(OutputPort, view.NodeViews[precondition.ChildNode.Guid].InputPort);
                    break;
            }
        }
        void LinkNodes(Port outputSocket, Port inputSocket)
        {
            var tempEdge = new EdgeView()
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            BehaviourTreeView.Instance.WindowRoot.TreeView.Add(tempEdge);
        }
        /// <summary>
        /// 添加子对象
        /// </summary>
        /// <param name="node"></param>
        public void AddChild(NodeView node)
        {
            switch (NodeViewData)
            {
                case BtComposite composite:
                    composite.ChildNodes.Add(node.NodeViewData);
                    break;
                case BtPrecondition precondition:
                    precondition.ChildNode = node.NodeViewData;
                    break;
            }
        }

        /// <summary> 移除子对象 </summary>
        public void RemoveChild(NodeView node)
        {
            switch (NodeViewData)
            {
                case BtComposite composite:
                    composite.ChildNodes.Remove(node.NodeViewData);
                    break;
                case BtPrecondition precondition:
                    if (precondition.ChildNode == node.NodeViewData)
                    {
                        precondition.ChildNode = null;
                    }
                    break;
            }
        }

        public override void OnSelected()
        {
            base.OnSelected();
            BehaviourTreeView.Instance.WindowRoot.InspectorView.UpdateInspector();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            BehaviourTreeView.Instance.WindowRoot.InspectorView.UpdateInspector();
        }

        /// <summary> 每帧更新 </summary>
        public void UpdateNodeView()
        {
            //更新名称
            title = NodeViewData.NodeName;
            ChangeTitleColor(BehaviourTreeSetting.GetSetting().GetNodeTitleColor(NodeViewData));
            if (Application.isPlaying && NodeViewData.NodeState == BehaviourState.执行中)
            {
                ChangeBgColor(Color.cyan);
                //更新线运行
            }
            else
            {
                //更新颜色
                ChangeBgColor(BehaviourTreeSetting.GetSetting().GetNodeBgColor(NodeViewData));
            }
            if (Application.isPlaying && InputPort.connected)
            {
                UpdateEdgeState();
            }
            //更新子节点顺序
            if (NodeViewData is BtComposite)
            {
                (NodeViewData as BtComposite)?.ChildNodes
                    .Sort((x, y) => x.Position.x.CompareTo(y.Position.x));
            }

        }

        void ChangeBgColor(Color color)
        {
            if (_nodeBorderBar == null || _nodeTitleBar == null) return;
            _nodeBorderBar.style.unityBackgroundImageTintColor = color;
            _nodeTitleBar.style.unityBackgroundImageTintColor = color;
        }
        void ChangeTitleColor(Color color)
        {
            if (_titleLabel == null) return;
            _titleLabel.style.color = color;
        }

        /// <summary>
        /// 根据对象运行状态改变线条
        /// </summary>
        void UpdateEdgeState()
        {
            if (NodeViewData.NodeState == BehaviourState.执行中)
            {
                (InputPort.connections.First() as EdgeView).OnStartMovePoints();
            }
            else
            {
                (InputPort.connections.First() as EdgeView).OnStopMovePoints();
                //把子节点状态改为未执行
                switch (NodeViewData)
                {
                    case BtComposite composite:
                        for (int i = 0; i < composite.ChildNodes.Count; i++)
                        {
                            if (composite.ChildNodes[i].NodeState != BehaviourState.未执行)
                            {
                                composite.ChildNodes[i].NodeState = BehaviourState.未执行;
                            }
                        }
                        break;
                    case BtPrecondition precondition:
                        if (precondition.ChildNode != null)
                        {
                            precondition.ChildNode.NodeState = BehaviourState.未执行;
                        }
                        break;
                }
            }

        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            NodeViewData.Position = new Vector2(newPos.xMin, newPos.yMin);
        }


        /// <summary> 添加右键菜单 </summary>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // base.BuildContextualMenu(evt);
            //base.BuildContextualMenu(evt);
            //evt.menu.MenuItems().Remove(evt.menu.MenuItems().Find(match => match.ToString() == ""));
            //evt.menu.AppendAction("Create Group",CreateGroup);
            evt.menu.AppendAction("设为根节点", SetRoot);
        }

        private void SetRoot(DropdownMenuAction obj)
        {
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            BehaviourTreeView.Instance.WindowRoot.TreeView.RootNode = this;
            BehaviourTreeView.Instance.WindowRoot.TreeView.OnStartMove();
            BehaviourTreeView.TreeData.Root = NodeViewData;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool operator +(NodeView p1, NodeView p2)
        {
            p1.LinkNodes(p1.OutputPort, p2.InputPort);
            return false;
        }
    }

    public class PortView
    {
        public static Port Create<TEdge>(Orientation ori = Orientation.Vertical, Direction dir = Direction.Input,
            Port.Capacity cap = Port.Capacity.Single, Type type = null) where TEdge : Edge, new()
        {
            Port port = Port.Create<TEdge>(ori, dir, cap, type);
            port.portName = "";

            port.style.flexDirection = dir == Direction.Input ? FlexDirection.Column : FlexDirection.ColumnReverse;
            return port;
        }
    }
}