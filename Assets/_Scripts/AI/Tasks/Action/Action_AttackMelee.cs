using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    [RequireComponent(typeof(Condition_IsPlayerInMeleeRange))]
    public class Action_AttackMelee : MonoBehaviour
    {
        public AudioClip attackSound;
        private AudioSource audioSource;

        [Header("Melee Attack Settings")]
        public float attackCooldown = 1.0f;
        public int meleeDamage = 5;

        private float lastAttackTime = -Mathf.Infinity;
        private Condition_IsPlayerInMeleeRange meleeRangeCheck;

        void Start()
        {
            meleeRangeCheck = GetComponent<Condition_IsPlayerInMeleeRange>();
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        public NodeState ExecuteTask()
        { 
            if (meleeRangeCheck.Check() != NodeState.SUCCESS)
            {
                return NodeState.FAILURE;
            }

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                if (attackSound) audioSource.PlayOneShot(attackSound);

                lastAttackTime = Time.time;

                Debug.Log($"[{gameObject.name}] Serang Melee!");

                PerformMeleeDamage();

                AIStatistics.RecordDecision("Attack");

                return NodeState.SUCCESS;
            }
            else
            {

                return NodeState.RUNNING;
            }
        }

        private void PerformMeleeDamage()
        {
            if (meleeRangeCheck.playerTransform != null)
            {
                Health playerHealth = meleeRangeCheck.playerTransform.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(meleeDamage);
                }
            }
        }
    }
}