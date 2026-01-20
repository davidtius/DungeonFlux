using System.Collections.Generic;

namespace DungeonFlux.AI
{
    public class Sequence : CompositeNode
    {
        public Sequence(List<Node> children) : base(children) { }

        public override NodeState Execute()
        {
            foreach (Node node in children)
            {
                switch (node.Execute())
                {
                    case NodeState.RUNNING:
                        return NodeState.RUNNING; 
                    case NodeState.FAILURE:
                        return NodeState.FAILURE; 
                    case NodeState.SUCCESS:
                        continue; 
                }
            }
            return NodeState.SUCCESS;
        }
    }
}