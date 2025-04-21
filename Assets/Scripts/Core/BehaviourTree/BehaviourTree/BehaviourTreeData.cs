using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JLBehaviourTree.BehaviourTree
{
    [BoxGroup]
    [HideReferenceObjectPicker]
    [LabelText("行为树数据")]
    public class BehaviourTreeData
    {
        [LabelText("是否显示数据")] public bool IsShow = false;

        [LabelText("根数据"), OdinSerialize, ShowIf("IsShow")]
        public BtNodeBase Root;

        [LabelText("根数据"), OdinSerialize, ShowIf("IsShow")]
        public List<BtNodeBase> NodeData = new();

        [OdinSerialize, ShowIf("IsShow"), HideReferenceObjectPicker]
        public SaveTransform viewTransform;//保存视图位置，下次打开的时候还是这个视图位置和缩放
        private BlackBoard blackboard = null;
        private bool _isActive;

        [Button("开始"), ButtonGroup("控制")]
        public void OnStart()
        {
            _isActive = true;
            OnUpdate();
        }

        private async void OnUpdate()
        {
            while (_isActive)
            {
                Root?.Tick();
                await UniTask.Yield();
            }
        }

        [Button("结束"), ButtonGroup("控制")]
        public void OnStop() => _isActive = false;

        public void InitBlackboard(BlackBoard blackboard)
        {
            this.blackboard = blackboard;
            Root?.InitBlackboard(this.blackboard);
        }

    }
    public class SaveTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Matrix4x4 matrix;
    }
}

