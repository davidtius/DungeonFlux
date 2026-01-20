using UnityEngine;
using DungeonFlux.AI;

namespace DungeonFlux.Tasks
{
    public class Action_Teleport : MonoBehaviour
    {
        [Header("Settings")]
        public float minInterval = 3f;
        public float maxInterval = 6f;
        public float teleportRadius = 5f;
        public LayerMask obstacleLayer;
        public GameObject teleportEffect;

        private float timer;

        void Start()
        {
            // HAPUS DEFAULT, BIAR KITA SET MANUAL DI INSPECTOR
            // if (obstacleLayer == 0) ... 
            
            ResetTimer();
        }

        public NodeState ExecuteTask()
        {
            // DEBUG 1: Apakah Task ini dipanggil oleh Tree?
            // Uncomment baris bawah ini kalau mau cek spam log-nya
            // Debug.Log("Teleport Task Checking..."); 

            timer -= Time.deltaTime;

            if (timer > 0) return NodeState.FAILURE;

            // WAKTUNYA TELEPORT
            Debug.Log($"[{gameObject.name}] Waktunya Teleport! Mencari posisi...");

            Vector2 targetPos = GetValidPosition();
            
            if (targetPos != Vector2.zero)
            {
                if (teleportEffect) Instantiate(teleportEffect, transform.position, Quaternion.identity);
                transform.position = targetPos;
                if (teleportEffect) Instantiate(teleportEffect, transform.position, Quaternion.identity);
                
                Debug.Log($"[{gameObject.name}] SUKSES: Random Teleport!"); // <--- INI YG KAMU CARI
                
                ResetTimer();
                return NodeState.SUCCESS;
            }

            // DEBUG 2: Kalau masuk sini, berarti gagal nemu tempat
            Debug.LogWarning($"[{gameObject.name}] GAGAL: Tidak menemukan posisi kosong (Cek Obstacle Layer!)");
            
            timer = 1.0f; // Coba lagi 1 detik kemudian
            return NodeState.FAILURE;
        }

        void ResetTimer() { timer = Random.Range(minInterval, maxInterval); }

        Vector2 GetValidPosition()
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 p = (Vector2)transform.position + Random.insideUnitCircle * teleportRadius;
                
                // Cek Tembok
                Collider2D hit = Physics2D.OverlapCircle(p, 0.4f, obstacleLayer);
                
                if (hit == null) 
                {
                    return p;
                }
                else
                {
                    // DEBUG 3: Liat dia nabrak apa?
                    // Debug.Log($"Percobaan {i} gagal, nabrak: {hit.name} (Layer: {LayerMask.LayerToName(hit.gameObject.layer)})");
                }
            }
            return Vector2.zero;
        }
        
        // Visualisasi di Scene View biar kelihatan
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, teleportRadius);
        }
    }
}