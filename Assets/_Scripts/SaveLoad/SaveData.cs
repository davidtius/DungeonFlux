using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{

    public int totalEctoplasma = 1000;

    public List<int> biomeOrder;
    public int currentFloor = 1;
    public string lastSceneName = "GamePlay";
    public List<string> savedWeaponNames = new List<string>();
    public int savedWeaponIndex = 0;

    public string savedSkillName = "";

    public float ema_Aggressive = 0f;
    public float ema_Passive = 0f;
    public float ema_Explorer = 0f;
    public float ema_Speedrunner = 0f;

    public int healthLevel = 0;
    public int damageLevel = 0;
    public int speedLevel = 0;
    public int lightLevel = 0;

    public SaveData()
    {
        totalEctoplasma = 0;
        currentFloor = 1;
        ema_Aggressive = 0f;
        ema_Passive = 0f;
        ema_Explorer = 0f;
        ema_Speedrunner = 0f;
        biomeOrder = new List<int>();
        savedWeaponNames = new List<string>();
    }
}