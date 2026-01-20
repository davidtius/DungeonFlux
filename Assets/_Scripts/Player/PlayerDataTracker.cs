using UnityEngine;

public class PlayerDataTracker : MonoBehaviour
{
    public static PlayerDataTracker Instance { get; private set; }

    [Header("Raw Metric Per Floor")]
    public float timeSpentInFloor = 0f;
    public int totalRoomsInFloor = 0;
    public int roomsVisited = 0;
    public int totalLootInFloor = 0;
    public int lootFound = 0;
    public int damageDealt = 0;
    public int offensiveDashes = 0;
    public int totalDashes = 0;
    public int skillsUsed = 0;
    public int damageTaken = 0;
    public float timeSpentIdle = 0f;
    public float timeSpentMoving = 0f;
    public int enemiesKilled = 0;
    public int secretRoomsFound = 0;
    public int shrinesUsed = 0;
    public int totalEnemiesInFloor = 0;

    [Header("Meta Progression")]
    public int totalEctoplasma = 0;
    public int swarmerKilled = 0;
    public int rangedKilled = 0;
    public int tankKilled = 0;
    public int bossKilled = 0;
    [HideInInspector] public float totalRunTime = 0f;

    [Header("Total Run Stats (Accumulated)")]
    public int totalRunDamageDealt = 0;
    public int totalRunEnemiesKilled = 0;
    public int totalSwarmerKilled = 0;
    public int totalRangedKilled = 0;
    public int totalTankKilled = 0;
    public int totalBossKilled = 0;

    [Header("Exploration Nuances")]
    public bool isObjectiveComplete = false;
    public bool hasVisitedExitRoom = false;
    public float timeSpentAfterObjective = 0f;
    public float totalRoomDwellTime = 0f;

    [Header("Combat Analytics")]
    public float lastKillTimestamp = -999f; // Waktu kill terakhir
    public float totalCombatKillInterval = 0f; // Total waktu antar-kill (hanya yg cepat)
    public int validCombatKillCount = 0;

    public int shrineHealsTaken = 0;
    public int shrineDamageTaken = 0;

    private bool isMoving = false;
    private Rigidbody2D rb;
    private bool isTimerPaused = false;

    public void SetTimerPaused(bool paused)
    {
        isTimerPaused = paused;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            rb = GetComponent<Rigidbody2D>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

        if (SaveManager.HasSaveFile())
        {
            SaveData data = SaveManager.Load();
            totalEctoplasma = data.totalEctoplasma;

            if (GameHUDManager.Instance != null)
            {
                GameHUDManager.Instance.UpdateEctoplasma(totalEctoplasma);
            }
        }
    }

    void Update()
    {
        if (!isTimerPaused)
        {
            float dt = Time.deltaTime;
            timeSpentInFloor += dt;
            totalRunTime += dt;
        }

        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if (isMoving)
        {
            timeSpentMoving += Time.deltaTime;
        }
        else
        {
            timeSpentIdle += Time.deltaTime;
        }

        if (GameHUDManager.Instance != null)
        {

            string kills = $"S:{totalSwarmerKilled} R:{totalRangedKilled} T:{totalTankKilled} B:{totalBossKilled}";

            GameHUDManager.Instance.UpdateTrackerStats(
                totalRunTime,
                damageDealt,
                totalEctoplasma,
                kills
            );
        }

        if (isObjectiveComplete && !isTimerPaused && hasVisitedExitRoom)
        {
            timeSpentAfterObjective += Time.deltaTime;
        }
    }

    public void RecordShrineDecision(bool isHeal)
    {
        shrinesUsed++;
        if (isHeal)
            shrineHealsTaken++;
        else
            shrineDamageTaken++;
    }

    public void SetObjectiveComplete()
    {
        isObjectiveComplete = true;
        Debug.Log("Tracker: Objective Complete! Mulai menghitung waktu eksplorasi tambahan...");
    }

    public void RecordSecretRoomFound()
    {
        secretRoomsFound++;
        Debug.Log("DDA Tracker: Secret Room Found!");
    }

    public void RecordDamageTaken(int amount)
    {
        damageTaken += amount;
    }

    public void RecordDamageDealt(int amount)
    {
        damageDealt += amount;
        totalRunDamageDealt += amount;
    }

    public void RecordOffensiveDash()
    {

        offensiveDashes++;
    }

    public void RecordSkillUse()
    {
        skillsUsed++;
    }

    public void RecordLootFound()
    {
        lootFound++;
    }

    public void RecordRoomVisited()
    {
        roomsVisited++;
    }

    public void SetFloorTotals(int totalRooms, int totalLoot)
    {
        this.totalRoomsInFloor = totalRooms;
        this.totalLootInFloor = totalLoot;
    }

    public void ResetFloorStats()
    {
        timeSpentInFloor = 0f;
        roomsVisited = 0;
        lootFound = 0;
        damageDealt = 0;
        offensiveDashes = 0;
        skillsUsed = 0;
        damageTaken = 0;
        timeSpentIdle = 0f;
        timeSpentMoving = 0f;
        enemiesKilled = 0;
        swarmerKilled = 0;
        rangedKilled = 0;
        tankKilled = 0;
        bossKilled = 0;
        shrinesUsed = 0;
        totalDashes = 0;

        isObjectiveComplete = false;
        hasVisitedExitRoom = false;
        timeSpentAfterObjective = 0f;
        totalRoomDwellTime = 0f;
        shrineHealsTaken = 0;
        shrineDamageTaken = 0;

        lastKillTimestamp = -999f;
        totalCombatKillInterval = 0f;
        validCombatKillCount = 0;

        totalRoomsInFloor = 0;
        totalLootInFloor = 0;
        Debug.Log("PlayerDataTracker: Statistik lantai di-reset.");
    }

    public void StartNewFloor(int totalRooms)
    {
        ResetFloorStats();
        totalRoomsInFloor = totalRooms;
        Debug.Log($"DDA Tracker: Lantai Baru Dimulai. Total Ruangan yang bisa dikunjungi: {totalRooms}");
    }

    public float GetExplorerScore()
    {

        float roomVisitRatio = 0f;
        if (totalRoomsInFloor > 0)
            roomVisitRatio = (float)roomsVisited / totalRoomsInFloor;

        float lootFoundRatio = 0f;
        if (totalLootInFloor > 0)
            lootFoundRatio = (float)lootFound / totalLootInFloor;

        float secretRatio = (secretRoomsFound > 0) ? 1.0f : 0f;

        float score = (0.5f * roomVisitRatio) + (0.3f * lootFoundRatio) + (0.2f * secretRatio);

        return score;
    }

    public void RecordSpecificKill(Health.EnemyVariant variant, Health.CharacterType mainType)
    {
        enemiesKilled++;
        totalRunEnemiesKilled++;

        if (lastKillTimestamp > 0) 
        {
            float timeDiff = timeSpentInFloor - lastKillTimestamp;

            // FILTER: Hanya hitung jika jedanya KURANG DARI 8 Detik
            // Artinya pemain sedang 'Killing Spree'. 
            // Kalau > 8 detik, dianggap jeda jalan-jalan (tidak dihitung).
            if (timeDiff < 8.0f)
            {
                totalCombatKillInterval += timeDiff;
                validCombatKillCount++;
                // Debug.Log($"[Combat Log] Combo Kill! Jeda: {timeDiff:F2}s");
            }
        }

        // Update waktu kill terakhir ke sekarang
        lastKillTimestamp = timeSpentInFloor;

        if (mainType == Health.CharacterType.Boss)
        {
            bossKilled++;
            totalBossKilled++;
        }
        else
        {
            switch (variant)
            {
                case Health.EnemyVariant.Swarmer: swarmerKilled++; totalSwarmerKilled++; break;
                case Health.EnemyVariant.Ranged: rangedKilled++; totalRangedKilled++; break;
                case Health.EnemyVariant.Tank: tankKilled++; totalTankKilled++; break;
            }
        }
    }

    public float GetAverageCombatKillTime()
    {
        if (validCombatKillCount == 0) return 999f; // Lambat banget / Gak ada combo
        return totalCombatKillInterval / validCombatKillCount;
    }

    public void AddEctoplasma(int amount)
    {
        totalEctoplasma += amount;

        if (GameHUDManager.Instance != null)
        {
            string kills = $"S:{totalSwarmerKilled} R:{totalRangedKilled} T:{totalTankKilled} B:{totalBossKilled}";
            GameHUDManager.Instance.UpdateTrackerStats(timeSpentInFloor, damageDealt, totalEctoplasma, kills);
        }
    }

    public void StartNewFloor(int totalRooms, int totalLoot, int totalEnemies)
    {
        ResetFloorStats();

        this.totalRoomsInFloor = totalRooms;
        this.totalLootInFloor = totalLoot;
        this.totalEnemiesInFloor = totalEnemies;

        Debug.Log($"DDA Tracker: Lantai Baru. Target: {totalRooms} Ruangan, {totalLoot} Loot Item.");
    }

    public void RecordShrineUse()
    {
        shrinesUsed++;

    }

    public void RecordExitRoomFound()
    {
        if (!hasVisitedExitRoom)
        {
            hasVisitedExitRoom = true;
            Debug.Log("Tracker: Pemain menemukan Ruang Exit!");
        }
    }

    public void RecordDash()
    {
        totalDashes++;
    }

    public void RecordRoomDwellTime(float duration)
    {
        totalRoomDwellTime += duration;
        Debug.Log($"Tracker: Menghabiskan {duration:F1} detik di ruangan " + gameObject.name);
    }
}