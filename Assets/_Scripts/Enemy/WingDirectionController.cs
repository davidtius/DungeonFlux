using UnityEngine;

public class WingDirectionController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;           // Animator musuh (untuk baca moveX/moveY)
    public SpriteRenderer bodyRenderer; // Sprite badan musuh (untuk acuan layer)
    
    [Header("Wings")]
    public SpriteRenderer leftWing;     // Masukkan objek Sayap Kiri
    public SpriteRenderer rightWing;    // Masukkan objek Sayap Kanan

    [Header("Settings")]
    public int offsetBehind = -1; // Order layer saat sayap di BELAKANG badan
    public int offsetFront = 1;   // Order layer saat sayap di DEPAN badan

    void Update()
    {
        if (animator == null) return;

        // 1. Ambil data arah dari Animator
        float moveX = animator.GetFloat("moveX");
        float moveY = animator.GetFloat("moveY");

        // 2. Tentukan mana yang lebih dominan (Gerak Horizontal atau Vertikal?)
        // Agar tidak bingung saat jalan miring (diagonal)
        bool isHorizontal = Mathf.Abs(moveX) > Mathf.Abs(moveY);

        if (isHorizontal)
        {
            // --- GERAK KIRI / KANAN ---
            
            // Kembalikan layer ke belakang badan (default)
            SetWingOrder(offsetBehind);

            if (moveX > 0) // Hadap KANAN
            {
                // User Request: Sayap Kanan Mati (biar seolah tertutup badan)
                leftWing.enabled = true;
                rightWing.enabled = false; 
            }
            else // Hadap KIRI
            {
                // User Request: Sayap Kiri Mati
                leftWing.enabled = false;
                rightWing.enabled = true;
            }
        }
        else
        {
            // --- GERAK ATAS / BAWAH ---
            
            // Nyalakan kedua sayap
            leftWing.enabled = true;
            rightWing.enabled = true;

            if (moveY > 0) // Hadap ATAS (Membelakangi Kamera)
            {
                // User Request: Sayap di ATAS objek (Menutupi punggung musuh)
                SetWingOrder(offsetFront);
            }
            else // Hadap BAWAH (Menghadap Kamera)
            {
                // User Request: Normal (Sayap di belakang punggung)
                SetWingOrder(offsetBehind);
            }
        }
    }

    // Fungsi helper untuk ubah urutan layer
    void SetWingOrder(int offset)
    {
        // Pastikan kita mengikuti sorting order badan + offset
        int bodyOrder = bodyRenderer.sortingOrder;
        leftWing.sortingOrder = bodyOrder + offset;
        rightWing.sortingOrder = bodyOrder + offset;
    }
}