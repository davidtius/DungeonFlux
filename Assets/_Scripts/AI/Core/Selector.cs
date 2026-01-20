using System.Collections.Generic;

namespace DungeonFlux.AI
{
    public class Selector : CompositeNode
    {
        public Selector(List<Node> children) : base(children) { }

        public override NodeState Execute()
        {
            foreach (Node node in children)
            {
                switch (node.Execute())
                {
                    case NodeState.RUNNING:
                        return NodeState.RUNNING; 
                    case NodeState.SUCCESS:
                        return NodeState.SUCCESS; 
                    case NodeState.FAILURE:
                        continue; 
                }
            }
            return NodeState.FAILURE;
        }
    }
}