using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    public class Action_UseSkill : MonoBehaviour
    {
        private RangedAI rangedAI;
        private TankAI tankAI;
        private Animator animator;
        private Transform playerTransform;

        void Start()
        {
            rangedAI = GetComponent<RangedAI>();
            tankAI = GetComponent<TankAI>();
            animator = GetComponent<Animator>();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        public NodeState ExecuteTask()
        {
            if (playerTransform == null) return NodeState.FAILURE;

            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

            if (rangedAI != null)
            {

                animator.SetFloat("moveX", directionToPlayer.x);
                animator.SetFloat("moveY", directionToPlayer.y);

                rangedAI.Shoot(directionToPlayer);
                AIStatistics.RecordDecision("Range Shoot");
                return NodeState.SUCCESS;
            }

            if (tankAI != null)
            {

                animator.SetFloat("moveX", directionToPlayer.x);
                        animator.SetFloat("moveY", directionToPlayer.y);

                tankAI.UseGroundSlam();
                return NodeState.SUCCESS;
            }

            return NodeState.FAILURE;
        }
    }
}