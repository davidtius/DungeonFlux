using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    public class Action_Chase : MonoBehaviour
    {
        [Header("Chase Logic")]
        public string playerTag = "Player";
        public float chaseSpeed = 4f;

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
            Debug.Log(gameObject.name + " Chasing...");
            if (playerTransform == null)
            {
                Debug.Log("Player Hilang dari pandangan " + gameObject.name);
                animator.SetFloat("moveX", 0);
                animator.SetFloat("moveY", 0);
                return NodeState.FAILURE;
            }

            float currentSpeed = chaseSpeed;
            if (myHealth != null) currentSpeed *= myHealth.speedMultiplier;

            Vector2 direction = (playerTransform.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);

            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);

            AIStatistics.RecordDecision("Chase");

            return NodeState.RUNNING;
        }
    }
}