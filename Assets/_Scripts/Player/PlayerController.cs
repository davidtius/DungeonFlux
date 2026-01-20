using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class LootData
    {
        public enum ItemType { Weapon, Skill }
        public ItemType itemType;

        public string itemName;
        public Sprite icon;

        public GameObject bulletPrefab;
        public float fireRate = 0.5f;
        public AudioClip shootSound;

        public SkillType skillEffect;
    }

    [Header("Base Stats")]

    public int baseDamage = 1;
    public int buffDamage = 0;

    public int defaultBaseDamage = 1;
    public float defaultMoveSpeed = 5f;
    public float baseLightRadius = 5f;

    [Header("Visual References")]
    public Transform flashlightObject;

    [Header("Inventory Settings")]
    public List<LootData> ownedWeapons = new List<LootData>();
    private int currentWeaponIndex = 0;
    [HideInInspector]
    public LootData currentSkillData;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public Transform firePoint;

    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Skill Settings")]
    public PlayerController.SkillType currentSkill = SkillType.None;
    public enum SkillType { None, Explosion, DoubleDamage, SpeedBoost }

    [Header("Audio SFX")]
    public AudioClip dashSound;
    public AudioClip switchSound;
    public AudioClip skillSound;
    public AudioClip groundSkillSound;
    public AudioClip interactSound;
    public AudioClip skillReadySound;

    [Header("Skill 1: Explosion (Tank Style)")]
    public float explosionCooldown = 180f;
    public float explosionRadius = 4f;
    public int explosionDamageMultiplier = 5;
    public GameObject explosionVfxPrefab;
    public LayerMask enemyLayer;

    [Header("Skill 2: Double Damage (Buff)")]
    public float doubleDamageCooldown = 120f;
    public float doubleDamageDuration = 5f;
    public float damageMultiplier = 2f;
    public float bulletSizeMultiplier = 2f;

    [Header("Skill 3: Speed Boost (Buff)")]
    public float speedBoostCooldown = 90f;
    public float speedBoostDuration = 10f;
    public float speedMultiplier = 3f;

    private float nextFireTime = 0f;
    private float nextDashTime = 0f;
    private float nextSkillTime = 0f;

    private bool isDashing = false;
    private bool isFiring = false;

    private bool isDoubleDamageActive = false;
    private bool isSpeedBoostActive = false;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private Vector2 mousePosition;
    private Animator animator;
    private ShrineController currentShrine;
    private TreasureChest currentChest;
    private SanctuaryShop currentShop;
    private AudioSource audioSource;
    private bool wasSkillOnCooldown = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {

        baseDamage = defaultBaseDamage;
        moveSpeed = defaultMoveSpeed;

        StartCoroutine(KeepSyncingHUD());

        UpdateStatsFromSave();
    }

    public void UpdateStatsFromSave()
    {
        if (SaveManager.HasSaveFile())
        {
            SaveData data = SaveManager.Load();

            baseDamage = defaultBaseDamage + data.damageLevel;
            Debug.Log($"Stat Updated: Damage = {baseDamage}");

            moveSpeed = defaultMoveSpeed + (data.speedLevel * 0.5f);
            Debug.Log($"Stat Updated: Speed = {moveSpeed}");

            if (flashlightObject != null)
            {
                float targetRadius = 5 + (data.lightLevel * 1.0f);
                float scaleFactor = targetRadius / 5;
                flashlightObject.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
                Debug.Log($"Stat Updated: Light Scale = {scaleFactor}");
            }

            Health myHealth = GetComponent<Health>();
            if (myHealth != null)
            {
                myHealth.UpdateStatsFromSave();

                if (SceneManager.GetActiveScene().name == "Sanctuary")
                {
                    myHealth.RestoreFullHealth();

                    // transform.position = Vector3.zero;
                }
            }

            LoadInventory(data);
        }

        UpdateWeaponHUD();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Player pindah ke scene: {scene.name}");

        if (scene.name == "Sanctuary")
        {

            transform.position = Vector3.zero;

            Health myHealth = GetComponent<Health>();
            if (myHealth != null)
            {
                myHealth.RestoreFullHealth();
                Debug.Log("Player di Sanctuary: HP Healed Penuh.");
            }

            isFiring = false;
            isDashing = false;
        }

        StartCoroutine(KeepSyncingHUD());
    }

    IEnumerator KeepSyncingHUD()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitUntil(() => GameHUDManager.Instance != null);
            ForceUpdateAllHUD();
            yield return new WaitForSeconds(0.5f);
        }
    }

    void ForceUpdateAllHUD()
    {
        if (GameHUDManager.Instance == null) return;

        UpdateWeaponHUD();

        if (currentSkillData != null)
        {
            GameHUDManager.Instance.UpdateSkillIcon(currentSkillData.icon);
        }

        int totalDmg = CalculateTotalDamage();
        GameHUDManager.Instance.UpdatePlayerStats(totalDmg);

        Health myHealth = GetComponent<Health>();
        if (myHealth != null)
        {
            GameHUDManager.Instance.UpdateHP(myHealth.GetCurrentHealth(), myHealth.maxHealth);
        }
    }

    public void OnMove(InputValue value) { moveDirection = value.Get<Vector2>(); }
    public void OnLook(InputValue value) { mousePosition = value.Get<Vector2>(); }

    public void OnDash(InputValue value)
    {
        if (value.isPressed && !isDashing && moveDirection != Vector2.zero)
        {
            if (isSpeedBoostActive || Time.time >= nextDashTime)
            {
                StartCoroutine(PerformDash());
            }
        }
    }

    public void OnSkill(InputValue value)
    {
        if (value.isPressed && Time.time >= nextSkillTime)
        {
            ActivateSkill();
        }
    }

    public void OnFire(InputValue value) { isFiring = value.Get<float>() > 0.5f; }

    void FixedUpdate()
    {
        if (isDashing) return;

        float currentSpeed = isSpeedBoostActive ? moveSpeed * speedMultiplier : moveSpeed;

        rb.linearVelocity = moveDirection * currentSpeed;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(mousePosition);

        Vector2 lookDirToMouse = (worldMousePos - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(lookDirToMouse.y, lookDirToMouse.x) * Mathf.Rad2Deg - 90f;
        firePoint.rotation = Quaternion.Euler(0f, 0f, angle);

        if (isFiring)
        {
            animator.SetFloat("moveX", lookDirToMouse.x);
            animator.SetFloat("moveY", lookDirToMouse.y);
        } else if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Vector2 moveDir = rb.linearVelocity.normalized;
            animator.SetFloat("moveX", moveDir.x);
            animator.SetFloat("moveY", moveDir.y);
        }

        animator.SetFloat("Speed", rb.linearVelocity.sqrMagnitude);

        if (isDashing) return;

        if (isFiring && Time.time >= nextFireTime)
        {
            if (ownedWeapons.Count > 0)
            {
                LootData currentWep = ownedWeapons[currentWeaponIndex];
                nextFireTime = Time.time + 1f / currentWep.fireRate;
                Shoot(currentWep);
            }
        }

        bool isSkillReady = (Time.time >= nextSkillTime);
        if (wasSkillOnCooldown && isSkillReady)
        {
            if (skillReadySound != null && audioSource != null) audioSource.PlayOneShot(skillReadySound);
            wasSkillOnCooldown = false;
        }

        float currentMaxCooldown = 1f;
        switch (currentSkill)
        {
            case SkillType.Explosion: currentMaxCooldown = explosionCooldown; break;
            case SkillType.DoubleDamage: currentMaxCooldown = doubleDamageCooldown; break;
            case SkillType.SpeedBoost: currentMaxCooldown = speedBoostCooldown; break;
        }

        float skillRatio = 0f;
        if (Time.time < nextSkillTime)
        {
            float timeRemaining = nextSkillTime - Time.time;
            skillRatio = 1f - (timeRemaining / currentMaxCooldown);
        }
        else
        {
            skillRatio = 1f;
        }

        float dashRatio = 0f;
        if (Time.time < nextDashTime)
            dashRatio = 1f - ((nextDashTime - Time.time) / dashCooldown);
        else
            dashRatio = 1f;

        if (GameHUDManager.Instance != null)
        {
            GameHUDManager.Instance.UpdateCooldowns(skillRatio, dashRatio);
        }
    }

    void Shoot(LootData weapon)
    {
        if (weapon.bulletPrefab == null) return;

        if (weapon.shootSound != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(weapon.shootSound);
        }

        GameObject bulletObj = Instantiate(weapon.bulletPrefab, firePoint.position, firePoint.rotation);
        BulletController bullet = bulletObj.GetComponent<BulletController>();

        if (bullet != null)
        {

            bullet.damage = CalculateTotalDamage();

            if (isDoubleDamageActive)
            {
                bullet.transform.localScale *= bulletSizeMultiplier;
            }
        }
    }

    private void ActivateSkill()
    {
        if (PlayerDataTracker.Instance != null) PlayerDataTracker.Instance.RecordSkillUse();
        wasSkillOnCooldown = true;

        switch (currentSkill)
        {
            case SkillType.Explosion:
                PerformExplosion();
                nextSkillTime = Time.time + explosionCooldown;
                if (skillSound != null) audioSource.PlayOneShot(groundSkillSound);
                break;

            case SkillType.DoubleDamage:
                StartCoroutine(BuffDoubleDamage());
                nextSkillTime = Time.time + doubleDamageCooldown;
                if (skillSound != null) audioSource.PlayOneShot(skillSound);
                break;

            case SkillType.SpeedBoost:
                StartCoroutine(BuffSpeedBoost());
                nextSkillTime = Time.time + speedBoostCooldown;
                if (skillSound != null) audioSource.PlayOneShot(skillSound);
                break;

            case SkillType.None: break;
            default: break;
        }
    }

    private void PerformExplosion()
    {
        if (explosionVfxPrefab != null)
        {
            GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
            GroundSlamEffect effect = vfx.GetComponent<GroundSlamEffect>();

            int baseTotal = baseDamage + buffDamage;
            int dmg = baseTotal * explosionDamageMultiplier;

            if (isDoubleDamageActive) dmg *= 2;

            if (effect != null)
            {
                effect.Initialize(dmg, explosionRadius, enemyLayer, "Enemies");
            }
        }
    }

    private IEnumerator BuffDoubleDamage()
    {
        isDoubleDamageActive = true;
        UpdateWeaponHUD();
        Debug.Log("<color=orange>SKILL: Double Damage Active!</color>");

        yield return new WaitForSeconds(doubleDamageDuration);

        isDoubleDamageActive = false;
        UpdateWeaponHUD();
        Debug.Log("SKILL: Double Damage Ended.");
    }

    private IEnumerator BuffSpeedBoost()
    {
        isSpeedBoostActive = true;
        Debug.Log("<color=cyan>SKILL: Speed Boost Active (Unlimited Dash)!</color>");

        yield return new WaitForSeconds(speedBoostDuration);

        isSpeedBoostActive = false;
        Debug.Log("SKILL: Speed Boost Ended.");
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        if (dashSound != null) audioSource.PlayOneShot(dashSound);

        if (!isSpeedBoostActive)
        {
            nextDashTime = Time.time + dashCooldown;
        }

        Vector2 dashDir = moveDirection != Vector2.zero ? moveDirection.normalized : Vector2.zero;
        rb.linearVelocity = dashDir * dashSpeed;

        if (PlayerDataTracker.Instance != null)
        {

            PlayerDataTracker.Instance.RecordDash();

            if (IsDashingTowardsEnemy())
            {
                PlayerDataTracker.Instance.RecordOffensiveDash();
            }
        }

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    private bool IsDashingTowardsEnemy()
    {
        float checkRadius = 5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemies"))
            {
                Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
                if (Vector2.Dot(moveDirection.normalized, dirToEnemy) > 0.5f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public Vector2 GetAimDirection() { return firePoint.up; }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            PauseManager pauseMgr = FindAnyObjectByType<PauseManager>();
            if (pauseMgr != null) pauseMgr.TogglePause();
        }
    }

    public void PermanentUpgradeDamage(int amount)
    {
        baseDamage += amount;
        UpdateWeaponHUD();
        Debug.Log($"Upgrade Permanen Berhasil. Base Damage skrg: {baseDamage}");
    }

    public void ApplyTemporaryDamageBuff(int amount)
    {
        buffDamage += amount;
        UpdateWeaponHUD();
        Debug.Log($"Buff Shrine Diterima. Buff Damage skrg: {buffDamage}");
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            if (currentShrine != null)
            {
                if (interactSound != null) audioSource.PlayOneShot(interactSound);
                currentShrine.Interact();
            }
            else if (currentChest != null)
            {
                if (interactSound != null) audioSource.PlayOneShot(interactSound);
                currentChest.Interact(this);
                currentChest.interactionPrompt.SetActive(false);
            }
            else if (currentShop != null)
            {
                if (interactSound != null) audioSource.PlayOneShot(interactSound);
                currentShop.Interact();
            }
        }
    }

    public void SetCurrentShop(SanctuaryShop shop) { currentShop = shop; }
    public void SetCurrentShrine(ShrineController shrine) { currentShrine = shrine; }
    public void SetCurrentChest(TreasureChest chest) { currentChest = chest; }

    public void OnSwitch(InputValue value)
    {
        if (value.isPressed && ownedWeapons.Count > 1)
        {
            if (switchSound != null) audioSource.PlayOneShot(switchSound);

            currentWeaponIndex = (currentWeaponIndex + 1) % ownedWeapons.Count;
            UpdateWeaponHUD();
            nextFireTime = Time.time + 0.2f;
        }
    }

    void UpdateWeaponHUD()
    {
        if (ownedWeapons.Count > 0 && GameHUDManager.Instance != null)
        {
            LootData current = ownedWeapons[currentWeaponIndex];
            GameHUDManager.Instance.UpdateWeaponInfo(current.icon);

            int currentTotalDamage = CalculateTotalDamage();
            GameHUDManager.Instance.UpdatePlayerStats(currentTotalDamage);
        }
    }

    public LootData PickupLoot(LootData newItem)
    {
        if (newItem.itemType == LootData.ItemType.Weapon)
        {
            if (ownedWeapons.Count < 2)
            {
                ownedWeapons.Add(newItem);
                currentWeaponIndex = ownedWeapons.Count - 1;
                UpdateWeaponHUD();
                SaveInventory();
                return null;
            }
            else
            {
                LootData oldWeapon = ownedWeapons[currentWeaponIndex];
                ownedWeapons[currentWeaponIndex] = newItem;
                UpdateWeaponHUD();
                SaveInventory();
                return oldWeapon;
            }
        }
        else if (newItem.itemType == LootData.ItemType.Skill)
        {
            LootData oldSkill = currentSkillData;
            currentSkillData = newItem;
            currentSkill = newItem.skillEffect;

            if (GameHUDManager.Instance != null)
            {
                GameHUDManager.Instance.UpdateSkillIcon(newItem.icon);
            }

            nextSkillTime = Time.time;
            SaveInventory();
            return oldSkill;
        }
        return null;
    }

    public int CalculateTotalDamage()
    {
        int weaponDmg = 0;

        if (ownedWeapons.Count > 0 && currentWeaponIndex < ownedWeapons.Count)
        {
            LootData currentWeapon = ownedWeapons[currentWeaponIndex];
            if (currentWeapon.bulletPrefab != null)
            {
                BulletController bulletScript = currentWeapon.bulletPrefab.GetComponent<BulletController>();
                if (bulletScript != null)
                {
                    weaponDmg = bulletScript.damage;
                }
            }
        }

        int total = baseDamage + buffDamage + weaponDmg;

        if (isDoubleDamageActive)
        {
            total = Mathf.RoundToInt(total * damageMultiplier);
        }

        return total;
    }

    void LoadInventory(SaveData data)
    {
        if (data.savedWeaponNames == null || data.savedWeaponNames.Count == 0)
        {
            Debug.Log("Save Data Inventory kosong. Menggunakan senjata Default.");
            UpdateWeaponHUD();
            return;
        }

        ownedWeapons.Clear();

        foreach (string wName in data.savedWeaponNames)
        {
            if (DDAManager.Instance != null)
            {
                LootData item = DDAManager.Instance.GetItemByName(wName);
                if (item != null) ownedWeapons.Add(item);
            }
        }

        currentWeaponIndex = Mathf.Clamp(data.savedWeaponIndex, 0, Mathf.Max(0, ownedWeapons.Count - 1));

        if (!string.IsNullOrEmpty(data.savedSkillName) && DDAManager.Instance != null)
        {
            LootData skillItem = DDAManager.Instance.GetItemByName(data.savedSkillName);
            if (skillItem != null)
            {
                currentSkillData = skillItem;
                currentSkill = skillItem.skillEffect;
            }
        }

        UpdateWeaponHUD();
    }

    public void SaveInventory()
    {
        SaveData data = SaveManager.HasSaveFile() ? SaveManager.Load() : new SaveData();

        data.savedWeaponNames.Clear();
        foreach (var w in ownedWeapons)
        {
            data.savedWeaponNames.Add(w.itemName);
        }

        data.savedWeaponIndex = currentWeaponIndex;
        data.savedSkillName = (currentSkillData != null) ? currentSkillData.itemName : "";

        SaveManager.Save(data);
        Debug.Log("Inventory Saved.");
    }

    public void OnPrintStats(InputValue value) { AIStatistics.PrintStats(); }

    public void OnResetStats(InputValue value) { AIStatistics.ResetStats(); }
}