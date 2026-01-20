using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip enemy_damageSound;
    public AudioClip player_damageSound;
    public AudioClip enemy_deathSound;
    public AudioClip player_deathSound;
    private AudioSource audioSource;

    public enum CharacterType
    {
        Player,
        Enemy,
        Boss
    }
    public CharacterType characterType;
    public enum EnemyVariant { None, Swarmer, Ranged, Tank }
    [Header("Enemy Specific")]
    public EnemyVariant enemyVariant = EnemyVariant.None;
    public int maxHealth = 3;

    [Header("Scaling Settings")]
    [Tooltip("HP awal. HP di Inspector (maxHealth) akan dicopy ke sini saat Start.")]
    private int baseMaxHealth;
    public int maxHealthModifier = 0;

    [Tooltip("Multiplier damage awal prefab. 1.0 untuk musuh biasa, 2.5 untuk Boss Swarmer, dll.")]
    public float baseDamageMultiplier = 1.0f;

    [Tooltip("Peningkatan stat per level biome (1.5x = 0.5f)")]
    public float perBiomeStatIncrease = 0.5f;

    [Header("Loot Settings")]
    public GameObject ectoplasmaPrefab;
    public int minDrop = 2;
    public int maxDrop = 4;
    public int bossMinDrop = 8;
    public int bossMaxDrop = 12;

    [Tooltip("Damage multiplier FINAL setelah dikalkulasi. Dibaca oleh skrip serangan BT.")]
    [HideInInspector] public float finalDamageMultiplier = 1.0f;
    [HideInInspector] public float speedMultiplier = 1.0f;
    private Coroutine burnCoroutine;
    private Coroutine slowCoroutine;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private int currentHealth;
    private WorldSpaceHealthBar healthBar;

    void Awake()
    {

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        healthBar = GetComponentInChildren<WorldSpaceHealthBar>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        baseMaxHealth = maxHealth;

        if (characterType == CharacterType.Player && SaveManager.HasSaveFile())
        {

            UpdateStatsFromSave();
        }

        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void UpdateStatsFromSave()
    {
        if (characterType != CharacterType.Player) return;

        int targetBaseMax = baseMaxHealth;

        if (SaveManager.HasSaveFile())
        {
            SaveData data = SaveManager.Load();

            int bonusHP = data.healthLevel * 2;

            targetBaseMax = baseMaxHealth + bonusHP;
        }

        int newMaxHealth = targetBaseMax + maxHealthModifier;

        if (newMaxHealth < 1) newMaxHealth = 1;

        if (maxHealth != newMaxHealth)
        {
            maxHealth = newMaxHealth;

            if (currentHealth > maxHealth) currentHealth = maxHealth;

            UpdateHealthBar();
            Debug.Log($"HP Stat Updated. Total Max: {maxHealth} (Base: {targetBaseMax} + Mod: {maxHealthModifier})");
        }
    }

    void Start()
    {

        finalDamageMultiplier = baseDamageMultiplier;
    }

    public void ApplyBiomeScaling(int biomeLevel)
    {
        if (characterType == CharacterType.Player) return;

        if (baseMaxHealth == 0) baseMaxHealth = maxHealth;

        float biomeMultiplier = 1.0f + (perBiomeStatIncrease * (biomeLevel - 1));

        maxHealth = Mathf.CeilToInt(baseMaxHealth * biomeMultiplier);
        currentHealth = maxHealth;

        finalDamageMultiplier = baseDamageMultiplier * biomeMultiplier;

        UpdateHealthBar();
        Debug.Log($"<color=yellow>Scaling Diterapkan:</color> {gameObject.name}. Biome Lvl {biomeLevel}. HP: {maxHealth}, DmgMulti Final: {finalDamageMultiplier}");
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHealthBar();

        if (characterType == CharacterType.Player)
        {
            if (PlayerDataTracker.Instance != null) PlayerDataTracker.Instance.RecordDamageTaken(damage);
            if (player_damageSound != null && audioSource != null) audioSource.PlayOneShot(player_damageSound);
        }
        else if (characterType == CharacterType.Enemy)
        {
            if (enemy_damageSound != null && audioSource != null) audioSource.PlayOneShot(enemy_damageSound);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (characterType == CharacterType.Enemy || characterType == CharacterType.Boss)
        {
            if (PlayerDataTracker.Instance != null)
                PlayerDataTracker.Instance.RecordSpecificKill(enemyVariant, characterType);
            if (enemy_deathSound) AudioSource.PlayClipAtPoint(enemy_deathSound, transform.position);
            SpawnLoot();
        }

        if (characterType == CharacterType.Boss && LevelGenerator.Instance != null)
        {
            LevelGenerator.Instance.SpawnPortalAfterBoss();
        }

        if (characterType == CharacterType.Player && GameOverManager.Instance != null)
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.SaveInventory();

            if (player_deathSound) AudioSource.PlayClipAtPoint(player_deathSound, transform.position);
            GameOverManager.Instance.TriggerGameOver();
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetCurrentHealthPercentage()
    {
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void UpdateHealthBar()
    {
        if (characterType == CharacterType.Player)
        {
            if (GameHUDManager.Instance != null) GameHUDManager.Instance.UpdateHP(currentHealth, maxHealth);
            return;
        }

        if (healthBar == null) healthBar = GetComponentInChildren<WorldSpaceHealthBar>();
        if (healthBar != null) healthBar.UpdateHealth(currentHealth, maxHealth);
    }

    public void ModifyMaxHealth(int amount)
    {

        maxHealthModifier += amount;

        UpdateStatsFromSave();

        Debug.Log($"Max HP Modifier Changed: {amount}. Total Mod: {maxHealthModifier}");
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void ApplyBurn(int damagePerTick, int ticks, float interval)
    {
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(BurnProcess(damagePerTick, ticks, interval));
    }

    public void ApplySlow(float slowFactor, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowProcess(slowFactor, duration));
    }

    private System.Collections.IEnumerator BurnProcess(int dmg, int ticks, float interval)
    {
        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.4f, 0.4f);

        for (int i = 0; i < ticks; i++)
        {
            yield return new WaitForSeconds(interval);

            currentHealth -= dmg;
            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                Die();
                yield break;
            }
        }

        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    private System.Collections.IEnumerator SlowProcess(float factor, float duration)
    {
        speedMultiplier = factor;

        if (spriteRenderer != null) spriteRenderer.color = Color.cyan;

        yield return new WaitForSeconds(duration);

        speedMultiplier = 1.0f;

        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    private void SpawnLoot()
    {
        if (ectoplasmaPrefab == null) return;
        int dropCount = (characterType == CharacterType.Boss) ? Random.Range(bossMinDrop, bossMaxDrop + 1) : Random.Range(minDrop, maxDrop + 1);

        int currentBiomeLvl = 1;
        if (LevelGenerator.Instance != null) currentBiomeLvl = LevelGenerator.Instance.GetCurrentBiomeLevel();
        dropCount += (currentBiomeLvl - 1);

        for (int i = 0; i < dropCount; i++)
        {
            GameObject ecto = Instantiate(ectoplasmaPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = ecto.GetComponent<Rigidbody2D>();
            if (rb != null) rb.AddForce(Random.insideUnitCircle.normalized * Random.Range(2f, 5f), ForceMode2D.Impulse);
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
    }
}