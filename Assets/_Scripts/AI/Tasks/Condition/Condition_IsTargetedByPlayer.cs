using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    public class Condition_IsTargetedByPlayer : MonoBehaviour
    {
        [Header("Targeting Check")]
        public string playerTag = "Player";
        public float targetingAngleThreshold = 15f;

        private Transform playerTransform;
        private PlayerController playerController;

        void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerController = playerObj.GetComponent<PlayerController>();
            }
        }

        public NodeState Check()
        {
            if (playerTransform == null || playerController == null)
            {
                return NodeState.FAILURE;
            }

            Vector2 playerAimDirection = playerController.GetAimDirection();

            Vector2 directionFromPlayerToAI = (transform.position - playerTransform.position).normalized;

            float angle = Vector2.Angle(playerAimDirection, directionFromPlayerToAI);

            if (angle <= targetingAngleThreshold)
            {
                Debug.Log($"[{gameObject.name}] Sedang dibidik Player");
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }
}