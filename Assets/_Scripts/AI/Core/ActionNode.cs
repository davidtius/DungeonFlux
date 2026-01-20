using System;

namespace DungeonFlux.AI
{
    public class ActionNode : Node
    {
        private Func<NodeState> action;

        public ActionNode(Func<NodeState> action)
        {
            this.action = action;
        }

        public override NodeState Execute()
        {
            return action.Invoke();
        }
    }
}