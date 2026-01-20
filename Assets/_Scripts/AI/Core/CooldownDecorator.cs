using UnityEngine;

namespace DungeonFlux.AI
{
    public class CooldownDecorator : DecoratorNode
    {
        private float cooldownDuration;
        private float lastExecutionTime = -Mathf.Infinity;

        public CooldownDecorator(Node child, float cooldown) : base(child)
        {
            this.cooldownDuration = cooldown;
        }

        public override NodeState Execute()
        {

            if (Time.time >= lastExecutionTime + cooldownDuration)
            {

                NodeState childState = child.Execute();

                if (childState == NodeState.SUCCESS || childState == NodeState.RUNNING)
                {
                    lastExecutionTime = Time.time;
                }
                return childState;
            }
            else
            {

                return NodeState.FAILURE;
            }
        }
    }
}