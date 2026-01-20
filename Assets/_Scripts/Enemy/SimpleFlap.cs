using UnityEngine;

public class SimpleFlap : MonoBehaviour
{
    public float speed = 10f;
    public float strength = 0.1f;
    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // Bikin scale X naik turun dikit (efek mengepak)
        float flap = Mathf.Sin(Time.time * speed) * strength;
        transform.localScale = new Vector3(initialScale.x + flap, initialScale.y, initialScale.z);
    }
}