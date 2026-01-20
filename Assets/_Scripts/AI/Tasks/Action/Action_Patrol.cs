using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    public class Action_Patrol : MonoBehaviour
    {
        [Header("Patrol Logic")]
        public float patrolSpeed = 2f;
        public float patrolRadius = 3f;
        public float waitTime = 1f;
        public LayerMask obstacleLayer;

        [Header("Rotation")]
        public float rotationSpeed = 5f;

        private Vector2 startPosition;
        private Vector2 targetPosition;
        private float waitTimer;
        private Animator animator;
        private Health myHealth;

        void Awake()
        {
            myHealth = GetComponent<Health>();
            animator = GetComponent<Animator>();
            startPosition = transform.position;
            if (obstacleLayer == 0) obstacleLayer = LayerMask.GetMask("Default");
            SetNewPatrolTarget();
        }

        public NodeState ExecuteTask()
        { 
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null) return NodeState.FAILURE;
            }

            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
                animator.SetFloat("moveX", 0);
                animator.SetFloat("moveY", 0);
                return NodeState.RUNNING;
            }

            float currentSpeed = patrolSpeed;
            if (myHealth != null) currentSpeed *= myHealth.speedMultiplier;

            if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            {
                waitTimer = waitTime;
                SetNewPatrolTarget();
                animator.SetFloat("moveX", 0);
                animator.SetFloat("moveY", 0);
                AIStatistics.RecordDecision("Patrol");
                return NodeState.SUCCESS;
            }
            else
            {

                Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
                animator.SetFloat("moveX", direction.x);
                animator.SetFloat("moveY", direction.y);
                return NodeState.RUNNING;
            }
        }

        private void SetNewPatrolTarget()
        {

            for (int i = 0; i < 10; i++)
            {
                Vector2 randomPoint = startPosition + Random.insideUnitCircle * patrolRadius;

                RaycastHit2D hit = Physics2D.Linecast(transform.position, randomPoint, obstacleLayer);

                if (hit.collider == null)
                {
                    targetPosition = randomPoint;
                    return;
                }
            }

            targetPosition = transform.position;
        }
    }
}