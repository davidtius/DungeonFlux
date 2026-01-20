using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransferPortal : MonoBehaviour
{
    [Header("Destination")]
    public string targetSceneName;

    [Header("Mode")]
    [Tooltip("Portal di BOSS ROOM (Menuju Sanctuary).")]
    public bool advanceFloor = false;

    [Tooltip("mengulang dari Lantai 1 (Game Over).")]
    public bool resetRun = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.SaveInventory();
            }

            Debug.Log($"Portal Masuk! Mode: AdvanceFloor={advanceFloor}, ResetRun={resetRun}, Target={targetSceneName}");

            SaveData data = SaveManager.HasSaveFile() ? SaveManager.Load() : new SaveData();

            if (PlayerDataTracker.Instance != null)
            {

                data.totalEctoplasma = PlayerDataTracker.Instance.totalEctoplasma;
            }

            if (advanceFloor)
            {
                if (data.currentFloor!=1) data.currentFloor++;
                Debug.Log($"Portal: Stage Clear! Lanjut ke Lantai {data.currentFloor}");
            }

            else if (resetRun)
            {
                data.currentFloor = 1;
            }

            data.lastSceneName = targetSceneName;
            SaveManager.Save(data);

                Debug.Log("Loading . . .");
                LevelLoader.LoadLevel(targetSceneName);
        }
    }
}
