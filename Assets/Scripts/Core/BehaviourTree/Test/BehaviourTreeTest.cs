using System;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace JLBehaviourTree.Test
{
    public class BehaviourTreeTest : SerializedMonoBehaviour
    {
        [OdinSerialize, HideReferenceObjectPicker, OpenView(ButtonName = "打开视图Button")]
        public BehaviourTreeData TreeData;


        /*public void FixedUpdate()
        {
            TreeData?.Root?.Tick();
        }*/
        public BehaviourTreeData GetBtData() => TreeData;

    }

    [NodeLabel("行为树事件系统")]
    public class BtEventSystem : BtPrecondition
    {
        public Func<bool> a1;

        public override BehaviourState Tick()
        {
            return ChildNode.Tick();
        }


    }

}