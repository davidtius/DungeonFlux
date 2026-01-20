
using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    public class Condition_IsPlayerInSight : MonoBehaviour
    {
        [Header("Player Detection")]
        public string playerTag = "Player";
        public float detectionRadius = 8f;
        public LayerMask obstacleLayer;

        private Transform playerTransform;

        public NodeState Check()
        {

            if (playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
                if (playerObj != null)
                {
                    playerTransform = playerObj.transform;
                }
                else
                {

                    return NodeState.FAILURE;
                }
            }

            if (Vector2.Distance(transform.position, playerTransform.position) > detectionRadius)
            {
                return NodeState.FAILURE;
            }

            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, detectionRadius, obstacleLayer);

            if (hit.collider != null)
            {

                return NodeState.FAILURE;
            }

            return NodeState.SUCCESS;
        }

        public Transform getPlayerTransform() {return playerTransform;}
    }
}