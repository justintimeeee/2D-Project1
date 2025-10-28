using UnityEngine;

public class HitSoundRouter : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D col)
    {
        if (!AudioManager.I) return;

        if (col.collider.CompareTag("Paddle"))
            AudioManager.I.PlayPaddle();
        else if (col.collider.CompareTag("Wall"))
            AudioManager.I.PlayWall();
        else if (col.collider.CompareTag("Brick"))
            AudioManager.I.PlayBrick();
    }
}
