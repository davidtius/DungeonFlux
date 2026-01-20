using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    public class Action_Flee : MonoBehaviour
    {
        [Header("Flee Logic")]
        public string playerTag = "Player";
        public float fleeSpeed = 5f;

        [Header("Rotation")]
        public float rotationSpeed = 5f;

        private Transform playerTransform;
        private Animator animator;
        private Health myHealth;

        void Start()
        {
            myHealth = GetComponent<Health>();
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            animator = GetComponent<Animator>();
        }

        public NodeState ExecuteTask()
        {
            if (playerTransform == null)
            {
                animator.SetFloat("moveX", 0);
                animator.SetFloat("moveY", 0);
                return NodeState.FAILURE;
            }

            AIStatistics.RecordDecision("Flee");

            float currentSpeed = fleeSpeed;
            if (myHealth != null) currentSpeed *= myHealth.speedMultiplier;

            Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, transform.position + (Vector3)fleeDirection, currentSpeed * Time.deltaTime);

            animator.SetFloat("moveX", fleeDirection.x);
            animator.SetFloat("moveY", fleeDirection.y);

            return NodeState.RUNNING;
        }
    }
}