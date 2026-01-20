using UnityEngine;
using DungeonFlux.AI;
using System.Collections;

namespace DungeonFlux.Tasks
{
    public class Action_Evade : MonoBehaviour
    {
        [Header("Evade Logic")]
        public float evadeDuration = 0.3f;
        public float evadeSpeed = 7f;

        private bool isEvading = false;
        private Health myHealth;

        void Start()
        {
            myHealth = GetComponent<Health>();
        }

        public NodeState ExecuteTask()
        {
            AIStatistics.RecordDecision("Evade");

            if (isEvading)
            {
                return NodeState.RUNNING;
            }
            else
            {
                StartCoroutine(EvadeCoroutine());
                return NodeState.RUNNING;
            }
        }

        private IEnumerator EvadeCoroutine()
        {
            isEvading = true;

            float currentSpeed = evadeSpeed;
            if (myHealth != null) currentSpeed *= myHealth.speedMultiplier;
            Vector2 evadeDirection = (Random.value > 0.5f) ? transform.right : -transform.right;

            float timer = 0f;
            while (timer < evadeDuration)
            {
                transform.position += (Vector3)evadeDirection * currentSpeed * Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }

            isEvading = false;

        }
    }
}