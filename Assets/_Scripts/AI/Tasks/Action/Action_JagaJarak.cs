using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    public class Action_JagaJarak : MonoBehaviour
    {
        [Header("Kite Logic")]
        public string playerTag = "Player";
        public float optimalRange = 5f;
        public float tooCloseRange = 2f;
        public float speed = 3f;

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

            float currentSpeed = speed;
            if (myHealth != null) currentSpeed *= myHealth.speedMultiplier;

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

            animator.SetFloat("moveX", directionToPlayer.x);
            animator.SetFloat("moveY", directionToPlayer.y);

            if (distance < tooCloseRange)
            {

                Vector2 fleeDirection = (transform.position - playerTransform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, transform.position + (Vector3)fleeDirection, currentSpeed * Time.deltaTime);
                return NodeState.RUNNING;
            }
            else if (distance > optimalRange)
            {

                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);
                return NodeState.RUNNING;
            }
            else
            {
                animator.SetFloat("moveX", directionToPlayer.x);
                animator.SetFloat("moveY", directionToPlayer.y); 

                AIStatistics.RecordDecision("Keep Distance");
                
                return NodeState.SUCCESS;
            }
        }
    }
}