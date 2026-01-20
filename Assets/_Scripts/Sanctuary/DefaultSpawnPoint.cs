using UnityEngine;

public class DefaultSpawnPoint : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        GameObject activePlayer = null;

        if (existingPlayer != null)
        {

            existingPlayer.transform.position = transform.position;
            Rigidbody2D rb = existingPlayer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            activePlayer = existingPlayer;
        }
        else
        {

            if (playerPrefab != null)
            {
                activePlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity);
                Debug.Log("Player Di-Spawn di Sanctuary.");
            }
            else
            {
                Debug.LogError("DefaultSpawnPoint: Player Prefab belum di-set!");
            }
        }

        if (activePlayer != null)
        {
            Health hp = activePlayer.GetComponent<Health>();
            if (hp != null)
            {
                hp.Heal(9999);

            }

            Rigidbody2D rb = activePlayer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
    }
}