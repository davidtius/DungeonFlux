using UnityEngine;
using UnityEngine.Audio;

public class FootstepSystem : MonoBehaviour
{
    [Header("Settings")]
    public AudioClip[] stepSounds;
    public float stepInterval = 0.35f;
    [Range(0f, 1f)] public float volume = 0.3f;

    [Header("Spatial Settings (Khusus Musuh)")]
    public bool is3D = false;

    [Header("Mixer Settings")]
    public AudioMixerGroup sfxGroup;

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private float stepTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        audioSource = gameObject.AddComponent<AudioSource>();

        if (sfxGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxGroup;
        }

        audioSource.playOnAwake = false;
        audioSource.volume = volume;

        if (is3D)
        {
            audioSource.spatialBlend = 1f;
            audioSource.maxDistance = 10f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        else
        {
            audioSource.spatialBlend = 0f;
        }
    }

    void Update()
    {

        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                PlayStep();
                stepTimer = stepInterval;
            }
        }
        else
        {

            stepTimer = 0;
        }
    }

    void PlayStep()
    {
        if (stepSounds.Length == 0) return;

        AudioClip clip = stepSounds[Random.Range(0, stepSounds.Length)];

        audioSource.pitch = Random.Range(0.9f, 1.1f);

        audioSource.volume = volume * Random.Range(0.9f, 1.0f);

        audioSource.PlayOneShot(clip);
    }
}