namespace DungeonFlux.AI
{
    public class InverterDecorator : DecoratorNode
    {
        public InverterDecorator(Node child) : base(child) { }

        public override NodeState Execute()
        {
            switch (child.Execute())
            {
                case NodeState.RUNNING:
                    return NodeState.RUNNING;
                case NodeState.SUCCESS:
                    return NodeState.FAILURE;
                case NodeState.FAILURE:
                    return NodeState.SUCCESS;
            }

            return NodeState.FAILURE;
        }
    }
}