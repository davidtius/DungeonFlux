using UnityEngine;
using UnityEngine.UI;

// Skrip ini menggambar kotak biru di semua objek yang Raycast Target-nya NYALA
public class UIRaycastViewer : MonoBehaviour
{
    static Vector3[] fourCorners = new Vector3[4];

    void OnDrawGizmos()
    {
        // Cari semua komponen Grafik (Image, Text, RawImage)
        foreach (var graphic in FindObjectsByType<Graphic>(FindObjectsSortMode.None))
        {
            // Jika Raycast Target NYALA
            if (graphic.raycastTarget)
            {
                RectTransform rectTransform = graphic.rectTransform;
                rectTransform.GetWorldCorners(fourCorners);
                
                // Gambar kotak biru
                Gizmos.color = Color.cyan;
                for (int i = 0; i < 4; i++)
                {
                    Gizmos.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);
                }
            }
        }
    }
}