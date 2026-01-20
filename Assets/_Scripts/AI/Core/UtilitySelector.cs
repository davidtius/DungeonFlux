using System.Collections.Generic;
using UnityEngine; 

namespace DungeonFlux.AI
{
    public class UtilitySelector : CompositeNode
    {
        private List<float> baseUtilities;
        private Dictionary<string, float> ddaWeights;

        private List<string> tacticNames; 
        
        public UtilitySelector(List<Node> children, List<string> tacticNames, List<float> baseUtilities) : base(children)
        {
            this.tacticNames = tacticNames;
            this.baseUtilities = baseUtilities;
            
            this.ddaWeights = new Dictionary<string, float>();
            foreach (string name in tacticNames)
            {
                this.ddaWeights.Add(name, 1.0f);
            }
        }

        public void SetWeights(Dictionary<string, float> newWeights)
        {
            this.ddaWeights = newWeights;
        }

        public override NodeState Execute()
        {
            float bestScore = -Mathf.Infinity;
            Node bestNode = null;
            
            for (int i = 0; i < children.Count; i++)
            {
                string name = tacticNames[i];
                float baseUtil = baseUtilities[i];
                
                float ddaWeight = ddaWeights.ContainsKey(name) ? ddaWeights[name] : 1.0f;
                
                float score = baseUtil * ddaWeight;
                
                Debug.Log($"Taktik: {name}, Skor: {score} (Base: {baseUtil} * Weight: {ddaWeight})");

                if (score > bestScore)
                {
                    bestScore = score;
                    bestNode = children[i];
                }
            }

            if (bestNode != null)
            {
                Debug.Log($"EKSEKUSI TAKTIK: {tacticNames[children.IndexOf(bestNode)]} dengan skor {bestScore}");
                return bestNode.Execute();
            }

            return NodeState.FAILURE;
        }
    }
}