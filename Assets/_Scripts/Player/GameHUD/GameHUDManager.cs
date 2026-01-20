using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameHUDManager : MonoBehaviour
{
    public static GameHUDManager Instance { get; private set; }

    [Header("Player Status")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI ectoText;

    [Header("Combat Actions")]
    public Image weaponIcon;
    public Image skillIcon;
    public Image dashIcon;

    [Header("Level Info")]
    public TextMeshProUGUI biomeNameText;
    public TextMeshProUGUI floorInfoText;
    public TextMeshProUGUI timerText;

    [Header("Stats & Kills")]
    public TextMeshProUGUI damageStatText;
    public TextMeshProUGUI killStatsText;

    [Header("Trap FX References")]
    public Image trapOverlay;
    public TextMeshProUGUI trapTimerText;
    public float flashSpeed = 2f;
    public float maxAlpha = 0.3f;

    [Header("Notifications")]
    public TextMeshProUGUI notificationText;
    public float notificationDuration = 3f;

    private Coroutine trapEffectCoroutine;
    private Coroutine notificationCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (notificationText != null)
        {
            notificationText.alpha = 0;
        }
    }

    public void ShowNotification(string message, Color color, float fontSize = 60f)
    {
        if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
        notificationCoroutine = StartCoroutine(PlayNotification(message, color, fontSize));
    }

    private IEnumerator PlayNotification(string message, Color color, float fontSize)
    {
        if (notificationText == null) yield break;

        notificationText.text = message;
        notificationText.color = color;
        notificationText.fontSize = fontSize;
        notificationText.alpha = 1;

        notificationText.transform.localScale = Vector3.one * 1.2f;
        float t = 0;
        while(t < 0.2f)
        {
            t += Time.deltaTime;
            notificationText.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t/0.2f);
            yield return null;
        }

        yield return new WaitForSeconds(notificationDuration);

        float fadeSpeed = 2f;
        while (notificationText.alpha > 0)
        {
            notificationText.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
    }

    public void UpdateHP(int current, int max)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }
        if (hpText != null) hpText.text = $"{current}/{max}";
    }

    public void UpdateCooldowns(float skillRatio, float dashRatio)
    {
        if (skillIcon != null) skillIcon.fillAmount = skillRatio;
        if (dashIcon != null) dashIcon.fillAmount = dashRatio;
    }

    public void UpdateLevelInfo(string biomeName, int biomeIndex, int floorCount)
    {
        if (biomeNameText != null) biomeNameText.text = biomeName;
        if (floorInfoText != null) floorInfoText.text = $"Floor {floorCount}";
    }

    public void UpdateTrackerStats(float time, int damage, int ecto, string killString)
    {

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60F);
            int seconds = Mathf.FloorToInt(time % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        if (ectoText != null) ectoText.text = ecto.ToString();
        if (killStatsText != null) killStatsText.text = killString;
    }

    public void StartTrapEffect(TrapRoomController.TrapType type)
    {

        StopTrapEffect();

        Color targetColor = Color.clear;

        switch (type)
        {
            case TrapRoomController.TrapType.PoisonGas:
                targetColor = Color.green;
                if (trapTimerText) trapTimerText.text = "Poison Gas Active!";
                break;
            case TrapRoomController.TrapType.TimeBomb:
                targetColor = Color.red;
                break;
        }

        trapEffectCoroutine = StartCoroutine(PulseOverlay(targetColor));
    }

    public void UpdateTrapTimer(float timeRemaining)
    {
        if (trapTimerText != null)
        {
            trapTimerText.text = $"Clear Enemies In {timeRemaining:F1}s";
            trapTimerText.color = (timeRemaining < 5f) ? Color.red : Color.white;
        }
    }

    public void StopTrapEffect()
    {
        if (trapEffectCoroutine != null) StopCoroutine(trapEffectCoroutine);

        if (trapOverlay != null) trapOverlay.color = Color.clear;
        if (trapTimerText != null) trapTimerText.text = "";
    }

    IEnumerator PulseOverlay(Color baseColor)
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime * flashSpeed;

            float alpha = Mathf.PingPong(timer, maxAlpha);

            baseColor.a = alpha;
            trapOverlay.color = baseColor;

            yield return null;
        }
    }

    public void UpdateEctoplasma(int amount)
    {
        if (ectoText != null) ectoText.text = amount.ToString();
    }

    public void UpdateWeaponInfo(Sprite icon)
    {
        if (weaponIcon != null)
        {
            weaponIcon.sprite = icon;

            weaponIcon.color = (icon == null) ? Color.clear : Color.white;
        }
    }

    public void UpdateSkillIcon(Sprite icon)
    {
        if (skillIcon != null)
        {
            skillIcon.sprite = icon;
            skillIcon.color = (icon == null) ? Color.clear : Color.white;
        }
    }

    public void UpdatePlayerStats(int damage)
    {
        if (damageStatText != null) damageStatText.text = $"DMG: {damage}";
    }
}