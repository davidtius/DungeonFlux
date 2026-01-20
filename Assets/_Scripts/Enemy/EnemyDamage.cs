using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDamage : MonoBehaviour
{
    [Tooltip("Damage dasar untuk serangan ini SEBELUM di-scale")]
    public int baseDamage = 1;

    [Tooltip("Jeda waktu (detik) antar damage jika pemain menempel terus")]
    public float damageInterval = 1.0f;
    private float nextDamageTime = 0f;

    private Health selfHealth;
    private Animator animator;

    void Start()
    {
        selfHealth = GetComponent<Health>();
        animator = GetComponent<Animator>();

        if (selfHealth == null)
        {
            Debug.LogError("EnemyDamage.cs tidak menemukan Health.cs di " + gameObject.name);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            if (Time.time >= nextDamageTime)
            {
                Health playerHealth = collision.gameObject.GetComponent<Health>();

                if (playerHealth != null && selfHealth != null)
                {
                    nextDamageTime = Time.time + damageInterval;

                    float finalMultiplier = selfHealth.finalDamageMultiplier;

                    int damageToDeal = Mathf.CeilToInt(baseDamage * finalMultiplier);

                    playerHealth.TakeDamage(damageToDeal);

                    if (animator != null)
                    {

                        animator.SetTrigger("Attack");
                    }

                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }
    }
}