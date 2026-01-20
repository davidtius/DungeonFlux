using UnityEngine;

public class ExitPortal : MonoBehaviour
{
    public string playerTag = "Player";

    [Header("Visual Settings")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color unlockedColor = Color.white;

    private bool triggered = false;
    private bool isLocked = true;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {

        LockPortal();
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (isLocked)
        {
            if (other.CompareTag(playerTag))
            {

                if (GameHUDManager.Instance != null)
                    GameHUDManager.Instance.ShowNotification("LOCKED\nFind Treasure First!", Color.red, 40f);
            }
            return;
        }

        if (!triggered && other.CompareTag(playerTag))
        {

            if (LevelGenerator.Instance != null)
            {

                 triggered = true;
                Debug.Log("Pemain mencapai Pintu Keluar.");
                 DDAManager.Instance.AnalyzeAndPrepareNextFloor();
                 LevelGenerator.Instance.GoToNextFloor();
            }
            else
            {
                Debug.LogError("LevelGenerator Instance tidak ditemukan oleh ExitPortal.");
            }
        }
    }

    public void LockPortal()
    {
        isLocked = true;
        if (spriteRenderer != null) spriteRenderer.color = lockedColor;
    }

    public void UnlockPortal()
    {
        isLocked = false;
        if (spriteRenderer != null) spriteRenderer.color = unlockedColor;

        Debug.Log("PORTAL TERBUKA!");
    }
}