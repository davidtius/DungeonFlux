using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip hitEnemySound;

    [Header("Bullet Settings")]
    public float speed = 20f;
    public float lifeTime = 2f;
    public int damage = 1;
    public bulletType bulletTo = bulletType.Enemies;

    [Header("Status Effects")]
    public BulletEffect effectType = BulletEffect.None;

    public int burnDamage = 1;
    public int burnTicks = 3;
    public float burnInterval = 1f;

    public float slowFactor = 0.5f;
    public float slowDuration = 3f;

    public enum bulletType {Player, Enemies};
    public enum BulletEffect { None, Fire, Ice }
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = transform.up * speed;

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Walls"))
        {
            Destroy(gameObject);
        }

        if (other.CompareTag(bulletTo.ToString()))
        {
            Health targetHealth = other.GetComponent<Health>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(damage);

                switch (effectType)
                {
                    case BulletEffect.Fire:
                        targetHealth.ApplyBurn(burnDamage, burnTicks, burnInterval);
                        break;
                    case BulletEffect.Ice:
                        targetHealth.ApplySlow(slowFactor, slowDuration);
                        break;
                }

                if (bulletTo == bulletType.Enemies && PlayerDataTracker.Instance != null)
                {
                    PlayerDataTracker.Instance.RecordDamageDealt(damage);
                }
            }
            if (hitEnemySound) AudioSource.PlayClipAtPoint(hitEnemySound, transform.position);
            Destroy(gameObject);
        }
    }
}