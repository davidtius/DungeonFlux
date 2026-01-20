using UnityEngine;

public class RangedAI : MonoBehaviour
{
    public AudioClip shootSound;
    private AudioSource audioSource;

    [Header("Ranged Attack Setup")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireCooldown = 1.5f;

    private float lastFireTime;

    void Start() {
        lastFireTime = Time.time;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

    public void Shoot(Vector2 shootDirection)
    {

        if (Time.time >= lastFireTime + fireCooldown)
        {
            if (shootSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(shootSound);
            }

            lastFireTime = Time.time;

            if (bulletPrefab != null && firePoint != null)
            {
                float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg - 90f;
                Quaternion bulletRotation = Quaternion.Euler(0, 0, angle);

                Instantiate(bulletPrefab, firePoint.position, bulletRotation);
            }
        }
    }
}