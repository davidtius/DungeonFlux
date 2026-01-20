using UnityEngine;

public class EnemyRoomKeeper : MonoBehaviour
{
    private Rect allowedBounds;
    private bool isInitialized = false;

    public void Initialize(RectInt roomGridCoords)
    {
        float padding = 1.5f;

        allowedBounds = new Rect(
            roomGridCoords.x + padding,
            roomGridCoords.y + padding,
            roomGridCoords.width - (padding * 2),
            roomGridCoords.height - (padding * 2)
        );

        isInitialized = true;
    }

    void LateUpdate()
    {
        if (!isInitialized) return;

        Vector3 currentPos = transform.position;

        float clampedX = Mathf.Clamp(currentPos.x, allowedBounds.xMin, allowedBounds.xMax);
        float clampedY = Mathf.Clamp(currentPos.y, allowedBounds.yMin, allowedBounds.yMax);

        if (currentPos.x != clampedX || currentPos.y != clampedY)
        {
            transform.position = new Vector3(clampedX, clampedY, currentPos.z);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (isInitialized)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(allowedBounds.center, allowedBounds.size);
        }
    }
}