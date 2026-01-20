using UnityEngine;
using System.Collections;

public class TankAI : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip chargeSound;
    private AudioSource audioSource;

    [Header("Tank Skill Setup")]
    public GameObject groundSlamPrefab;
    public Transform groundSlamPoint;
    public float skillCooldown = 5f;

    public float skillRadius = 3f;
    public int skillDamage = 10;
    public string playerTag = "Player";

    [Header("Telegraph Settings")]
    public float warningDuration = 1.0f;
    public Color warningColor = Color.red;
    public float flashSpeed = 10f;

    private float lastSkillTime;
    private LayerMask playerLayer;

    private bool isUsingSkill = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        try
        {
            playerLayer = LayerMask.GetMask(LayerMask.LayerToName(GameObject.FindGameObjectWithTag(playerTag).layer));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TANK AI " + gameObject.name +  " GAGAL: Cek Player ada di scene dan memiliki Tag '{playerTag}'. Error: {e.Message}");
        }

        lastSkillTime = Time.time;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void UseGroundSlam()
    {

        if (Time.time >= lastSkillTime + skillCooldown)
        {

            lastSkillTime = Time.time;

            Debug.Log("TANK " + gameObject.name +  " : Menggunakan Ground Slam!");

            StartCoroutine(PerformGroundSlamSequence());
        }
    }

    private IEnumerator PerformGroundSlamSequence()
    {
        isUsingSkill = true;

        if (chargeSound != null) audioSource.PlayOneShot(chargeSound);

        float timer = 0f;
        while (timer < warningDuration)
        {
            timer += Time.deltaTime;

            if (spriteRenderer != null)
            {
                float t = Mathf.PingPong(Time.time * flashSpeed, 1f);
                spriteRenderer.color = Color.Lerp(originalColor, warningColor, t);
            }
            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        Debug.Log("TANK  " + gameObject.name +  " : Ground Slam Dieksekusi!");

        if (groundSlamPrefab != null && groundSlamPoint != null)
        {
            GameObject slamEffect = Instantiate(groundSlamPrefab, groundSlamPoint.position, Quaternion.identity);

            GroundSlamEffect effectScript = slamEffect.GetComponent<GroundSlamEffect>();
            if (effectScript != null)
            {
                effectScript.Initialize(skillDamage, skillRadius, playerLayer, playerTag);
            }
        }
        else
        {
            PerformAoEDamage();
        }

        isUsingSkill = false;
    }

    private void PerformAoEDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundSlamPoint.position, skillRadius, playerLayer);

        foreach(Collider2D hit in hits)
        {
            Health playerHealth = hit.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(skillDamage);
                Debug.Log("TANK  " + gameObject.name +  " : Ground Slam (fallback) mengenai Player!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundSlamPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundSlamPoint.position, skillRadius);
        }
    }
}