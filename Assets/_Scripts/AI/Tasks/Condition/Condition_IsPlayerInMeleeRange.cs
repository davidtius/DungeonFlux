using UnityEngine;
using DungeonFlux.AI;
using System.Diagnostics;

namespace DungeonFlux.Tasks
{
    public class Condition_IsPlayerInMeleeRange : MonoBehaviour
    {
        [Header("Melee Range Check")]
        public float meleeRange = 1.5f;
        public string playerTag = "Player";
        [HideInInspector] public Transform playerTransform;

        void Start()
        {

            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        public NodeState Check()
        {
            if (playerTransform == null)
            {
                UnityEngine.Debug.Log(gameObject.name + " Cek Melee Range Gagal.");
                return NodeState.FAILURE;
            }

            UnityEngine.Debug.Log(gameObject.name + " Cek Melee Range.");

            if (Vector2.Distance(transform.position, playerTransform.position) <= meleeRange)
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }
}