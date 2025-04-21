using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace JLBehaviourTree.Editor.View
{

    public class OpenViewAttributeDrawer : OdinAttributeDrawer<OpenViewAttribute, BehaviourTreeData>
    {
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 让此label传递下去，便于其他的特性进行绘制
            this.CallNextDrawer(label);
            if (GUILayout.Button(Attribute.ButtonName))
            {
                BehaviourTreeView.TreeData = ValueEntry.SmartValue;
                BehaviourTreeView.OpenView();
            }

        }
    }


    [InitializeOnLoad]
    public static class PlayModeStateChangedExample
    {
        static PlayModeStateChangedExample()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (!BehaviourTreeView.Instance) return;
            if (state != PlayModeStateChange.ExitingEditMode && state != PlayModeStateChange.EnteredPlayMode) return;
            BehaviourTreeView.Instance.RefreshWindow();
        }
    }


    public class BehaviourTreeView : EditorWindow
    {
        public static BehaviourTreeView Instance;
        public static BehaviourTreeData TreeData;
        public SplitView WindowRoot;

        [MenuItem("Tools/JLBehaviourTree/BehaviourTreeView _#&i")]//#&i表示快捷键shift+alt+i
        public static void OpenView()
        {
            BehaviourTreeView wnd = GetWindow<BehaviourTreeView>();
            string title = TreeData?.Root?.NodeName ?? "行为树";
            wnd.titleContent = new GUIContent(title);
            Instance = wnd;
        }

        /// <summary> 加载GUI </summary>
        public void CreateGUI()
        {
            BehaviourTreeDataInit(TreeData);
        }

        /// <summary> 每帧更新,说是一秒更新10次？ </summary>
        private void OnInspectorUpdate()
        {
            WindowRoot?.TreeView?.OnGUI();
        }

        // private void OnEnable()
        // {
        //     Undo.undoRedoPerformed += RefreshWindow;
        // }

        // private void OnDisable()
        // {
        //     Undo.undoRedoPerformed -= RefreshWindow;
        // }

        private void OnDestroy()
        {
            Save();
            TreeData = null;
        }

        private void OnSelectionChange()
        {

        }

        public void RefreshWindow()
        {
            rootVisualElement.Clear();
            BehaviourTreeDataInit(TreeData);
        }

        public void BehaviourTreeDataInit(BehaviourTreeData treeData)
        {
            Instance = this;
            VisualElement root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("Core/BehaviourTree/BehaviourTreeView");
            visualTree.CloneTree(root);
            WindowRoot = root.Q<SplitView>("SplitView");
            WindowRoot.TreeView = WindowRoot.Q<TreeView>();
            WindowRoot.InspectorView = WindowRoot.Q<InspectorView>();
            WindowRoot.InspectorTitle = WindowRoot.Q<Label>("InspectorTitle");

            //获取并设置窗口大小与位置
            if (treeData == null) return;
            var tr = treeData.viewTransform;
            if (tr != null)
            {
                WindowRoot.TreeView.viewTransform.position = tr.position;
                WindowRoot.TreeView.viewTransform.scale = tr.scale;
            }

            if (treeData.NodeData == null) return;

            treeData.NodeData.ForEach(n => CreateNodeViews(n, treeData.Root));
            //创建节点后连线
            WindowRoot.TreeView.nodes.OfType<NodeView>().ForEach(n => n.LinkChildNode());
        }


        public void CreateNodeViews(BtNodeBase nodeData, BtNodeBase rootNode = null)
        {
            TreeView view = Instance.WindowRoot.TreeView;
            NodeView nodeView = new NodeView(nodeData);
            if (nodeData == rootNode)
            {
                view.RootNode = nodeView;
            }
            nodeView.SetPosition(new Rect(nodeData.Position, Vector2.one));
            view.AddElement(nodeView);
        }

        public void Save()
        {
            if (Application.isPlaying) return;
            if (TreeData == null) return;
            TreeData.viewTransform = new SaveTransform();
            TreeData.viewTransform.position = WindowRoot.TreeView.viewTransform.position;
            TreeData.viewTransform.scale = WindowRoot.TreeView.viewTransform.scale;
            var scene = SceneLoader.Instance.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    public class RightClickMenu : ScriptableObject, ISearchWindowProvider
    {
        public delegate bool SelectEntryDelegate(SearchTreeEntry searchTreeEntry
            , SearchWindowContext context);

        public SelectEntryDelegate OnSelectEntryHandler;

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("创建节点"))
            };
            entries = AddNodeType<BtComposite>(entries, "组合节点");
            entries = AddNodeType<BtPrecondition>(entries, "条件节点");
            entries = AddNodeType<BtActionNode>(entries, "行为节点");
            return entries;
        }

        /// <summary>
        /// 通过反射获取对应的菜单数据
        /// </summary>
        public List<SearchTreeEntry> AddNodeType<T>(List<SearchTreeEntry> entries, string pathName)
        {
            entries.Add(new SearchTreeGroupEntry(new GUIContent(pathName)) { level = 1 });
            List<System.Type> rootNodeTypes = typeof(T).GetDerivedClasses();
            foreach (var rootType in rootNodeTypes)
            {
                string menuName = rootType.Name;
                if (rootType.GetCustomAttribute(typeof(NodeLabelAttribute)) is NodeLabelAttribute nodeLabel)
                {
                    menuName = nodeLabel.MenuName;
                    if (nodeLabel.MenuName == "")
                    {
                        menuName = rootType.Name;
                    }
                }
                entries.Add(new SearchTreeEntry(new GUIContent(menuName)) { level = 2, userData = rootType });
            }
            return entries;
        }


        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (OnSelectEntryHandler == null)
            {
                return false;
            }
            return OnSelectEntryHandler(SearchTreeEntry, context);
        }
    }
}
