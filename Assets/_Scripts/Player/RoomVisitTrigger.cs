using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomVisitTrigger : MonoBehaviour
{
    public bool isExitRoom = false;
    private bool hasBeenVisited = false;
    private float entryTime;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            entryTime = Time.time;

            if (!hasBeenVisited)
            {
                hasBeenVisited = true;

                Debug.Log("Room Visited: " + gameObject.name);

                if (PlayerDataTracker.Instance != null)
                {

                    PlayerDataTracker.Instance.RecordRoomVisited();

                    if (isExitRoom)
                    {
                        PlayerDataTracker.Instance.RecordExitRoomFound();
                    }
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            float duration = Time.time - entryTime;
            
            if (PlayerDataTracker.Instance != null)
            {
                PlayerDataTracker.Instance.RecordRoomDwellTime(duration);
            }
        }
    }
}