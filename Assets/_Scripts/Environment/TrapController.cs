using UnityEngine;
using TMPro;

public class TrapRoomController : MonoBehaviour
{
    public enum TrapType
    {
        None,
        PoisonGas,
        TimeBomb
    }

    [Header("Trap Settings")]
    public TrapType trapType = TrapType.PoisonGas;

    [Header("Poison Gas Settings")]
    public int poisonDamage = 1;
    public float damageInterval = 1.5f;

    [Header("Time Bomb Settings")]
    public float timeLimit = 30f;

    private LockingRoomTrigger lockingScript;
    private GameObject playerObj;
    private float timer = 0f;
    private float poisonTimer = 0f;
    private bool trapActive = false;

    public TextMeshPro timerText;

    void Start()
    {
        lockingScript = GetComponent<LockingRoomTrigger>();
        if (lockingScript == null)
        {
            Debug.LogError("TrapRoomController butuh LockingRoomTrigger di objek yang sama!");
            Destroy(this);
        }
    }

    void Update()
    {

        if (lockingScript.IsLocked && !lockingScript.IsCleared)
        {
            if (!trapActive)
            {

                trapActive = true;
                StartTrap();
            }

            if (trapType == TrapType.PoisonGas)
            {
                RunPoisonLogic();
            }
            else if (trapType == TrapType.TimeBomb)
            {
                RunTimeBombLogic();
            }
        }
        else
        {

            if (trapActive)
            {
                trapActive = false;
                StopTrap();
            }
        }
    }

    void StartTrap()
    {

        playerObj = GameObject.FindGameObjectWithTag("Player");
        if (GameHUDManager.Instance != null)
        {
            GameHUDManager.Instance.StartTrapEffect(trapType);
        }
        if (trapType == TrapType.PoisonGas)
        {
            Debug.Log("<color=green>TRAP: Poison Gas!</color>");

        }
        else if (trapType == TrapType.TimeBomb)
        {
            Debug.Log("<color=red>TRAP: Time Bomb!</color>");
            timer = timeLimit;
        }
    }

    void StopTrap()
    {
        Debug.Log("Trap Nonaktif. Ruangan aman.");
        if (timerText != null) timerText.text = "";
        if (GameHUDManager.Instance != null)
        {
            GameHUDManager.Instance.StopTrapEffect();
        }

    }

    void RunPoisonLogic()
    {
        if (playerObj == null) return;

        poisonTimer += Time.deltaTime;

        if (poisonTimer >= damageInterval)
        {
            poisonTimer = 0f;
            Health playerHealth = playerObj.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(poisonDamage);
                Debug.Log("Kena damage racun.");
            }
        }
    }

    void RunTimeBombLogic()
    {
        if (playerObj == null) return;

        timer -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timer).ToString();
            timerText.color = Color.red;
        }

        if (GameHUDManager.Instance != null)
        {
            GameHUDManager.Instance.UpdateTrapTimer(timer);
        }

        if (timer <= 0)
        {

            Health playerHealth = playerObj.GetComponent<Health>();
            if (playerHealth != null)
            {
                Debug.Log("BOOM! Waktu time bomb habis.");

                playerHealth.TakeDamage(9999);
            }
        }
    }
}