using UnityEngine;

public class BrickHitSound : MonoBehaviour
{
    public AudioClip smashClip;
    public float volume = 1f;
    void OnCollisionEnter2D(Collision2D c)
    {
        if (smashClip) AudioSource.PlayClipAtPoint(smashClip, transform.position, volume);
    }
}
