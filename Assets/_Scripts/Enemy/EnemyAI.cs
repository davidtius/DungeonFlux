using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float speed = 3f;
    public float sightRange = 10f;

    private Transform player;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= sightRange)
            {

                Vector2 direction = (player.position - transform.position).normalized;

                rb.linearVelocity = direction * speed;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                rb.rotation = angle;
            }
            else
            {

                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}