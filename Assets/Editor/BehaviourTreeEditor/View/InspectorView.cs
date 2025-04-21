using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JLBehaviourTree.Editor.View
{
    public class InspectorView : VisualElement
    {
        public IMGUIContainer inspectorBar;

        public InspectorDataView InspectorDataView;
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits> { }

        public InspectorView()
        {
            Init();
        }

        void Init()
        {
            inspectorBar = new IMGUIContainer() { name = "inspectorBar" };//创建一个gui的框
            inspectorBar.style.flexGrow = 1;//设置gui框的大小,拉伸到最大
            CreateInspectorView();
            Add(inspectorBar);
        }

        /// <summary>
        /// 更新选择节点面板显示
        /// </summary>
        public void UpdateInspector()
        {
            InspectorDataView.selectDatas.Clear();
            BehaviourTreeView.Instance.WindowRoot.TreeView.selection
                .Select(node => node as NodeView)
                .ForEach(node =>
                {
                    if (node != null)
                    {
                        InspectorDataView.selectDatas.Add(node.NodeViewData);
                    }
                });
        }

        private async void CreateInspectorView()
        {
            InspectorDataView = Resources.Load<InspectorDataView>("Core/BehaviourTree/InspectorDataView");
            if (!InspectorDataView)
            {
                InspectorDataView = ScriptableObject.CreateInstance<InspectorDataView>();
                AssetDatabase.CreateAsset(InspectorDataView, "Assets/Resources/Core/BehaviourTree/InspectorDataView.asset");
            }
            await UniTask.Delay(100);//异步延迟 100 毫秒,确保资源加载和创建过程完成
            var odinEditor = UnityEditor.Editor.CreateEditor(InspectorDataView);

            inspectorBar.onGUIHandler += () =>
            {
                odinEditor.OnInspectorGUI();
            };
        }
    }




}