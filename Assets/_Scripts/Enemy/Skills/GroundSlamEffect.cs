using UnityEngine;
using System.Collections;

public class GroundSlamEffect : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip explosionSound;
    private AudioSource audioSource;

    [Header("Timing")]
    public float impactDelay = 0.1f;

    private int damageAmount;
    private float damageRadius;
    private LayerMask targetLayer;
    private string targetTag;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Initialize(int damage, float radius, LayerMask layer, string tag)
    {
        this.damageAmount = damage;
        this.damageRadius = radius;
        this.targetLayer = layer;
        this.targetTag = tag;

        StartCoroutine(ProcessExplosion());
    }

    private IEnumerator ProcessExplosion()
    {
        yield return new WaitForSeconds(impactDelay);

        if (explosionSound != null) audioSource.PlayOneShot(explosionSound);

        ApplyDamage();

        float destroyDelay = 0.5f;
        if (animator != null)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            destroyDelay = info.length - impactDelay;
        }

        Destroy(gameObject, destroyDelay > 0 ? destroyDelay : 0);
    }

    private void ApplyDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius, targetLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(targetTag))
            {
                Health targetHealth = hit.GetComponent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damageAmount);

                    if (targetTag == "Enemies")
                    {
                        if (PlayerDataTracker.Instance != null)
                        {
                            PlayerDataTracker.Instance.RecordDamageDealt(damageAmount);
                        }
                    }
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}