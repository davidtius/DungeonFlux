using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("Settings")]

    public float visibleDistance = 5.0f;
    public Health owner;

    public Image fillImage;
    private Transform mainCameraTransform;
    private Transform playerTransform;
    private Canvas canvas;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;

        canvas = GetComponent<Canvas>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (owner != null && owner.characterType == Health.CharacterType.Boss) visibleDistance = 15f;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {

        float fillAmount = currentHealth / maxHealth;

        if (fillImage != null)
        {
            fillImage.fillAmount = fillAmount;
        } else
        {
            Debug.Log("fill Image tidak ditemukan");
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {

            transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                             mainCameraTransform.rotation * Vector3.up);
        }

        if (playerTransform != null && canvas != null)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= visibleDistance)
            {
                canvas.enabled = true;
            }
            else
            {
                canvas.enabled = false;
            }
        }
    }
}