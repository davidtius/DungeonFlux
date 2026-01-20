using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public string mainMenuSceneName = "MainMenu";

    [Header("Stats Text References")]
    public TextMeshProUGUI biomeFloorText;
    public TextMeshProUGUI enemiesKilledText;
    public TextMeshProUGUI bossKilledText;
    public TextMeshProUGUI damageDealtText;
    public TextMeshProUGUI playTimeText;
    public TextMeshProUGUI ectoText;
    public TextMeshProUGUI weaponsText;
    public TextMeshProUGUI upgradeStatsText;
    public TextMeshProUGUI playerTypeText;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip deathSound;
    public AudioClip clickSound;
    public AudioClip hoverSound;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

    public void TriggerGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {

        Debug.Log("GAME OVER!");

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic();
        }

        if (sfxSource != null && deathSound != null)
        {
            sfxSource.PlayOneShot(deathSound);
        }

        yield return new WaitForSeconds(1.5f);

        ShowStats();

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void ShowStats()
    {

        PlayerDataTracker tracker = PlayerDataTracker.Instance;
        LevelGenerator levelGen = LevelGenerator.Instance;

        SaveData saveData = SaveManager.HasSaveFile() ? SaveManager.Load() : new SaveData();

        if (biomeFloorText != null && levelGen != null)
        {
            string biomeName = levelGen.GetCurrentBiomeTheme().biomeName;
            biomeFloorText.text = $"{biomeName} - Floor {levelGen.GetCurrentFloor()}";
        }

        if (enemiesKilledText != null && tracker != null)
        {
            enemiesKilledText.text = $"Swarmer Killed: {tracker.totalSwarmerKilled}\nRanged Killed: {tracker.totalRangedKilled}\nTank Killed: {tracker.totalTankKilled}";
        }

        if (bossKilledText != null && tracker != null)
        {
            bossKilledText.text = $"Boss Killed: {tracker.totalBossKilled}";
        }

        if (damageDealtText != null && tracker != null)
        {
            damageDealtText.text = $"Total Damage Dealt: {tracker.totalRunDamageDealt}";
        }

        if (playTimeText != null && tracker != null)
        {
            float t = tracker.totalRunTime;
            int min = Mathf.FloorToInt(t / 60F);
            int sec = Mathf.FloorToInt(t % 60F);
            playTimeText.text = $"PlayTime: {min:00}:{sec:00}";
        }

        if (ectoText != null && tracker != null)
        {
            ectoText.text = $"Remaining\nEctoplasma: {tracker.totalEctoplasma}";
        }

        string weaponListString = "None";
        PlayerController player = FindAnyObjectByType<PlayerController>(FindObjectsInactive.Include);

        if (player != null)
        {
            List<string> names = new List<string>();
            foreach (var w in player.ownedWeapons)
            {
                names.Add(w.itemName);
            }
            if (names.Count > 0) weaponListString = string.Join(", ", names);
        }
        else
        {

            SaveData saved = SaveManager.HasSaveFile() ? SaveManager.Load() : new SaveData();
            if (saveData.savedWeaponNames != null && saveData.savedWeaponNames.Count > 0)
                weaponListString = string.Join(", ", saveData.savedWeaponNames);
        }

        if (weaponsText != null)
        {
            weaponsText.text = $"Weapons: {weaponListString}";
        }

        if (upgradeStatsText != null)
        {
            upgradeStatsText.text = $"Upgrades:HP ({saveData.healthLevel}), Dmg ({saveData.damageLevel}), Spd ({saveData.speedLevel}), Light ({saveData.lightLevel})";
        }

        if (playerTypeText != null && DDAManager.Instance != null)
        {
            playerTypeText.text = $"Playstyle: {DDAManager.Instance.GetCurrentProfileName()}";
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;

       SaveManager.DeleteSave();

        if (PlayerDataTracker.Instance != null)
        {
            PlayerDataTracker.Instance.ResetFloorStats();
            PlayerDataTracker.Instance.totalEctoplasma = 0;
            Destroy(PlayerDataTracker.Instance.gameObject);
        }

        Debug.Log("GAME OVER: Semua progress dihapus. Pemain kembali ke titik nol.");

        LevelLoader.LoadLevel(mainMenuSceneName);
    }

    public void PlayClickSFX()
    {
        if (sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
    }

    public void PlayHoverSFX()
    {
        if (sfxSource != null && hoverSound != null) sfxSource.PlayOneShot(hoverSound);
    }
}