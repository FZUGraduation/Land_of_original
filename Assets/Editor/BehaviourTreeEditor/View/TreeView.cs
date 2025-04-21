using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.BTAutoLayout;
using JLBehaviourTree.Editor.EditorExTools;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace JLBehaviourTree.Editor.View
{
    public class TreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<TreeView, UxmlTraits> { }

        public NodeView RootNode;
        public Dictionary<string, NodeView> NodeViews;

        public TreeView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Resources/Core/BehaviourTree/BehaviourTreeView.uss"));
            //增加右键菜单
            GraphViewMenu();
            //选择对象
            RegisterCallback<KeyDownEvent>(KeyDownControl);
            RegisterCallback<MouseEnterEvent>(MouseEnterControl);
            //改变视图时触发的事件
            graphViewChanged = OnGraphViewChanged;
            NodeViews = new Dictionary<string, NodeView>();
        }

        public void OnGUI()
        {
            //OnDrawMove();
            nodes.ForEach(node => (node as NodeView).UpdateNodeView());
        }

        public void OnStartMove()
        {
            if (RootNode == null)
            {
                Debug.LogError("根节点为空");
                return;
            }
            NodeAutoLayouter.Layout(new RNG_LayoutNodeConvertor().Init(RootNode));
        }

        /// <summary> 添加节点时放入字典 </summary>
        public new void AddElement(GraphElement graphElement)
        {
            base.AddElement(graphElement);
            if (graphElement is NodeView nodeView)
            {
                //Debug.Log($" 名称: {nodeView.NodeViewData.NodeName} -- Guid: {nodeView.NodeViewData.Guid} ");
                NodeViews.Add(nodeView.NodeViewData.Guid, nodeView);
            }
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            switch (selectable)
            {
                case Edge edge:
                    edge.RemoveLink();
                    break;
                case NodeView view:
                    NodeViews.Remove(view.NodeViewData.Guid);
                    BehaviourTreeView.TreeData.NodeData.Remove(view.NodeViewData);
                    break;
            }
        }

        public new void RemoveElement(GraphElement graphElement)
        {
            base.RemoveElement(graphElement);
            if (graphElement is NodeView view)
            {
                NodeViews.Remove(view.NodeViewData.Guid);
                BehaviourTreeView.TreeData.NodeData.Remove(view.NodeViewData);
            }

        }


        #region 按键回调

        private GraphViewChange OnGraphViewChanged(GraphViewChange viewChange)
        {
            //不为空说明有新的线条被创建
            viewChange.edgesToCreate?.ForEach(edge =>
            {
                //把数据添加到节点中，这是一个扩展方法
                edge.AddLink();
            });
            //不为空说明有线条或者节点被删除
            viewChange.elementsToRemove?.ForEach(element =>
            {
                switch (element)
                {
                    case Edge edge:
                        edge.RemoveLink();
                        break;
                    case NodeView view:
                        NodeViews.Remove(view.NodeViewData.Guid);
                        BehaviourTreeView.TreeData.NodeData.Remove(view.NodeViewData);
                        break;
                }
            });
            return viewChange;
        }
        private void MouseEnterControl(MouseEnterEvent evt)
        {
            BehaviourTreeView.Instance.WindowRoot.InspectorView.UpdateInspector();
        }

        private void KeyDownControl(KeyDownEvent evt)
        {
            BehaviourTreeView.Instance.WindowRoot.InspectorView.UpdateInspector();
            if (evt.keyCode == KeyCode.Tab)
            {
                evt.StopPropagation();
            }
            //evt.StopPropagation();可以阻止事件继续传递，不会触发其他事件，只影响到当前窗口
            if (!evt.ctrlKey) return;
            switch (evt.keyCode)
            {
                case KeyCode.S:
                    BehaviourTreeView.Instance.Save();
                    evt.StopPropagation();
                    break;
                case KeyCode.E:
                    OnStartMove();
                    evt.StopPropagation();
                    break;
                case KeyCode.X:
                    Cut(null);
                    evt.StopPropagation();
                    break;
                case KeyCode.C:
                    Copy(null);
                    evt.StopPropagation();
                    break;
                case KeyCode.V:
                    Paste(null);
                    evt.StopPropagation();
                    break;
            }
        }


        public void Cut(DropdownMenuAction da)
        {
            Copy(null);
            CutSelectionCallback();
        }

        private void Copy(DropdownMenuAction da)
        {
            var ns = selection.OfType<NodeView>()
                // .Select(n => n)
                .Select(n => n.NodeViewData).ToList();
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();

            setting.CopyNode = ns.CloneData();
        }

        private void Paste(DropdownMenuAction da)
        {
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            if (setting.CopyNode == null) return;
            if (setting.CopyNode.Count == 0) return;
            ClearSelection();
            List<NodeView> pasteNode = new List<NodeView>();
            //生成节点并选择，重新序列化克隆的节点
            for (int i = 0; i < setting.CopyNode.Count; i++)
            {
                NodeView node = new NodeView(setting.CopyNode[i]);
                this.AddElement(node);
                node.SetPosition(new Rect(setting.CopyNode[i].Position, Vector2.one));
                AddToSelection(node);
                pasteNode.Add(node);
                BehaviourTreeView.TreeData.NodeData.Add(setting.CopyNode[i]);
            }
            pasteNode.ForEach(n => n.LinkChildNode());
            setting.CopyNode = setting.CopyNode.CloneData();
        }





        #endregion

        #region 右键菜单

        private RightClickMenu menuWindowProvider;
        public void GraphViewMenu()
        {
            menuWindowProvider = ScriptableObject.CreateInstance<RightClickMenu>();
            menuWindowProvider.OnSelectEntryHandler = OnMenuSelectEntry;

            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);
            };
        }

        //点击右键菜单时触发
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // base.BuildContextualMenu(evt);
            evt.menu.AppendAction("创建节点", _ =>
            {
                var windowRoot = BehaviourTreeView.Instance.rootVisualElement;
                var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent,
                    _.eventInfo.mousePosition + BehaviourTreeView.Instance.position.position);
                SearchWindow.Open(new SearchWindowContext(windowMousePosition), menuWindowProvider);
            });
            evt.menu.AppendAction("整理节点", _ => OnStartMove());
            evt.menu.AppendAction("Cut", Cut);
            evt.menu.AppendAction("Copy", Copy);
            evt.menu.AppendAction("Paste", Paste);


            //evt.menu.MenuItems().Remove(evt.menu.MenuItems().Find(match => match.ToString() == ""));

        }

        //点击菜单时菜单创建Node
        private bool OnMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowRoot = BehaviourTreeView.Instance.rootVisualElement;

            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent,
                context.screenMousePosition - BehaviourTreeView.Instance.position.position);

            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
            var nodeBase = System.Activator.CreateInstance((System.Type)searchTreeEntry.userData) as BtNodeBase;
            var nodeLabel = nodeBase.GetType().GetCustomAttribute(typeof(NodeLabelAttribute)) as NodeLabelAttribute;
            nodeBase.NodeName = nodeBase.GetType().Name;
            if (nodeLabel != null)
            {
                if (nodeLabel.Label != "" && nodeLabel.Label != null)
                {
                    nodeBase.NodeName = nodeLabel.Label;
                }
            }
            nodeBase.NodeType = nodeBase.GetType().GetNodeType();
            nodeBase.Position = graphMousePosition;
            if (string.IsNullOrEmpty(nodeBase.Guid))
            {
                nodeBase.Guid = System.Guid.NewGuid().ToString();
            }
            CreateNode(searchTreeEntry.userData as Type, graphMousePosition, nodeBase);
            return true;
        }

        private void CreateNode(Type type, Vector2 position, BtNodeBase nodeBase)
        {
            NodeView nodeView = new(nodeBase);
            nodeView.SetPosition(new Rect(position, Vector2.one));
            this.AddElement(nodeView);
            BehaviourTreeView.TreeData.NodeData.Add(nodeBase);
            AddToSelection(nodeView);
        }

        //覆写GetCompatiblePorts 定义链接规则,返回所有能够连接的端口
        public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            return ports.Where(endPorts =>
                endPorts.direction != startAnchor.direction && endPorts.node != startAnchor.node)
                .ToList();//不能连接相同类型的端口（输入不能连接输入，输出不能连接输出）+ 不能连接自己
        }

        #endregion

    }
}