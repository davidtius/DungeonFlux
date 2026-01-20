using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{

    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Action_AttackMelee))]
    public class Action_BossSwarmer_ChaseAndRest : MonoBehaviour
    {
        [Header("Boss Stats")]
        public float chaseSpeed = 7f;
        public float restSpeed = 1f;
        public float chaseDuration = 5f;
        public float restDuration = 3f;
        public float attackRange = 1.5f;

        [Header("Rotation")]
        public float rotationSpeed = 5f;

        private Transform playerTransform;
        private float timer;
        private bool isResting;
        private Health health;
        private Action_AttackMelee task_AttackMelee;

        void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
            health = GetComponent<Health>();
            task_AttackMelee = GetComponent<Action_AttackMelee>();
            timer = chaseDuration;
            isResting = false;
        }

        public NodeState ExecuteTask()
        {
            if (playerTransform == null || task_AttackMelee == null) return NodeState.FAILURE;

            timer -= Time.deltaTime;
            float currentSpeed = isResting ? restSpeed : chaseSpeed;
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= attackRange)
            {

                task_AttackMelee.ExecuteTask();
            }
            else
            {

                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, currentSpeed * Time.deltaTime);
            }

            if (isResting)
            {
                if (timer <= 0)
                {
                    isResting = false;
                    timer = chaseDuration;
                }
            }
            else
            {
                if (timer <= 0)
                {
                    isResting = true;
                    timer = restDuration;
                }
            }

            return NodeState.RUNNING;
        }

    }
}