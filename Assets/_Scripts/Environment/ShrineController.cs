using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class ShrineController : MonoBehaviour
{
    [Header("UI References (Scene)")]
    private GameObject shrinePanel;
    private Button btnHeal;
    private Button btnTrade;
    private Button btnCancel;

    [Header("Visuals")]
    public GameObject interactPrompt;
    public Sprite usedSprite;
    private SpriteRenderer spriteRenderer;

    [Header("Audio Settings")]
    public AudioSource sfxSource;
    public AudioClip clickSound;

    [Header("Settings")]
    public int healAmountPercent = 50;
    public int damageBonus = 2;
    public int maxHpPenalty = 5;

    private bool isPlayerInZone = false;
    private bool isUsed = false;
    private GameObject playerObj;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform panelTrans = FindDeepChild(canvasObj.transform, "ShrinePanel");
            if (panelTrans != null)
            {
                shrinePanel = panelTrans.gameObject;
                Transform tHeal = FindDeepChild(panelTrans, "Btn_Heal");
                Transform tTrade = FindDeepChild(panelTrans, "Btn_Trade");
                Transform tCancel = FindDeepChild(panelTrans, "Btn_Leave");

                if (tHeal) btnHeal = tHeal.GetComponent<Button>();
                if (tTrade) btnTrade = tTrade.GetComponent<Button>();
                if (tCancel) btnCancel = tCancel.GetComponent<Button>();
            }
            else
            {
                Debug.LogError("CRITICAL: Objek 'ShrinePanel' tidak ditemukan.");
            }
        }
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindDeepChild(child, childName);
            if (result != null)
                return result;
        }
        return null;
    }

    public void Interact()
    {
        Debug.Log($"Interact Function. Shrine?");
        if (shrinePanel == null) return;
        Debug.Log($"Shrine selected.");

        btnHeal.onClick.RemoveAllListeners();
        btnHeal.onClick.AddListener(ChooseHeal);

        btnTrade.onClick.RemoveAllListeners();
        btnTrade.onClick.AddListener(ChooseTrade);

        btnCancel.onClick.RemoveAllListeners();
        btnCancel.onClick.AddListener(CloseMenu);

        shrinePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    void ChooseHeal()
    {
        PlayClickSFX();
        if (playerObj != null)
        {
            Health playerHealth = playerObj.GetComponent<Health>();
            int healVal = Mathf.CeilToInt(playerHealth.maxHealth * (healAmountPercent / 100f));
            playerHealth.Heal(healVal);
        }

        if (PlayerDataTracker.Instance != null)
        {
            PlayerDataTracker.Instance.RecordShrineDecision(true);
        }

        FinishInteraction();
    }

    void ChooseTrade()
    {
        PlayClickSFX();
        if (playerObj != null)
        {

            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc != null) pc.ApplyTemporaryDamageBuff(damageBonus);

            Health playerHealth = playerObj.GetComponent<Health>();
            if (playerHealth != null) playerHealth.ModifyMaxHealth(-maxHpPenalty);
        }

        if (PlayerDataTracker.Instance != null)
        {
            PlayerDataTracker.Instance.RecordShrineDecision(false);
        }
        FinishInteraction();
    }

    void CloseMenu()
    {
        PlayClickSFX();
        shrinePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void FinishInteraction()
    {
        isUsed = true;

        if (usedSprite != null) spriteRenderer.sprite = usedSprite;
        if (interactPrompt != null) interactPrompt.SetActive(false);

        if (PlayerDataTracker.Instance != null)
        {
            PlayerDataTracker.Instance.RecordShrineUse();
        }

        CloseMenu();
        Debug.Log("Shrine Used.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Objek masuk ke Shrine Trigger: {other.name} dengan Tag: {other.tag}");

        if (other.CompareTag("Player") && !isUsed)
        {
            isPlayerInZone = true;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetCurrentShrine(this);
            }
            playerObj = other.gameObject;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetCurrentShrine(null);
            }
            playerObj = null;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    void PlayClickSFX()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}