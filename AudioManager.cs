using UnityEngine;

/// 
/// Simple SFX/Music hub for the whole game.
/// I kept the API the same on purpose so the other scripts don't need edits.
/// 
public class AudioManager : MonoBehaviour
{
    // === Singleton pattern (lightweight) ===
    // I use "I" because the rest of the project already expects AudioManager.I
    public static AudioManager I { get; private set; }

    [Header("Sources")]
    // SFX go through this one-shot source. If I forget to assign it in the Inspector,
    // Awake() will add one so the game doesn't crash.
    [SerializeField] private AudioSource sfxSource;

    // Background music source. I left it optional; safe to leave unassigned.
    [SerializeField] private AudioSource musicSource;

    [Header("Clips")]
    // I kept names the same so existing references in the Inspector keep working.
    public AudioClip brickHit;
    // Bounce uses its own source so it can overlap with other SFX cleanly.
    public AudioSource bounceSource;
    public AudioClip paddleHit;
    public AudioClip wallHit;
    public AudioClip powerupPickup;
    public AudioClip loseLife;
    public AudioClip gameOver;

    [Header("Levels")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.85f;   // global SFX multiplier (I clamp per call too)


    void Awake()
    {
        // Singleton init: keep the first instance, kill extras (in case of scene reloads).
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        // Keep audio alive across scene loads (restart button won't kill music/SFX hub).
        DontDestroyOnLoad(gameObject);

        // Make sure we always have an SFX source even if I forget to hook it up.
        if (!sfxSource)
            sfxSource = gameObject.AddComponent<AudioSource>();

        // Bounce can be its own dedicated AudioSource so bounce spam doesn't cut off other SFX.
        if (!bounceSource)
            bounceSource = gameObject.AddComponent<AudioSource>();

        // Consistent defaults so nothing fires on scene load by accident.
        sfxSource.playOnAwake = false;
        bounceSource.playOnAwake = false;

        // If we ever assign a musicSource in the scene, set some safe defaults.
        if (musicSource)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    // === Tiny helper so I don't repeat boilerplate ===
    private void OneShot(AudioClip clip, float vol = 1f)
    {
        if (!clip || !sfxSource) return;
        // Clamp to be extra safe; this way callers can pass 0..1 and master sfxVolume scales it.
        float finalVol = Mathf.Clamp01(sfxVolume * Mathf.Clamp01(vol));
        sfxSource.PlayOneShot(clip, finalVol);
    }

    // === Public API (kept same names so other scripts work unchanged) ===

    public void PlayGameOver() => OneShot(gameOver);
    public void PlayLoseLife() => OneShot(loseLife);
    public void PlayBrick() => OneShot(brickHit);
    public void PlayPaddle() => OneShot(paddleHit);
    public void PlayWall() => OneShot(wallHit);
    public void PlayPickup() => OneShot(powerupPickup);

    // Bounce uses its own dedicated source so rapid bounces don't interrupt other SFX.
    public void PlayBounce()
    {
        // If there is no clip on bounceSource, I just do nothing (no error spam).
        if (bounceSource && bounceSource.clip)
            bounceSource.Play();
    }

    public void PlaySFX(AudioClip clip, float vol = 1f) => OneShot(clip, vol);
}
