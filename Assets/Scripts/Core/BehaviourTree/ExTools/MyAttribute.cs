using System;

namespace JLBehaviourTree.ExTools
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OpenViewAttribute : Attribute
    {
        public string ButtonName = "打开行为树";
    }
}