/*
 *  * Purpose:
 *  - Handles what happens when a brick gets hit by the ball.
 *  - Adds score, plays effects/sounds, and maybe drops a power-up.
 */

using UnityEngine;

namespace FruitSmash
{
    public class Brick : MonoBehaviour
    {
        [Header("Points / FX")]
        public int points = 50;                 // how many points this brick gives
        public GameObject juiceSplatVFX;        // visual effect prefab when destroyed

        [System.Serializable]
        public class Drop                      // struct-like class for random drops
        {
            public GameObject prefab;           // what spawns (like banana, powerup, etc.)
            [Range(0f, 1f)] public float chance = 0.1f;  // 0.1 = 10% chance to drop
        }

        [Header("Possible drops")]
        public Drop[] drops;                    // can hold multiple possible powerups

        // When the ball hits this brick, Unity calls OnCollisionEnter2D automatically.
        void OnCollisionEnter2D(Collision2D col)
        {
            DestroySelf();                      // I keep all the logic in a helper to stay tidy.
        }

        // This runs the full "brick breaks" sequence.
        void DestroySelf()
        {
            //had to disable was causing computer crashing when streaming with OBS
            if (juiceSplatVFX)
                Instantiate(juiceSplatVFX, transform.position, Quaternion.identity);

            // Check the drop list; each item rolls its own random chance.
            if (drops != null)
            {
                foreach (var d in drops)
                {
                    if (d.prefab && Random.value < d.chance)
                        Instantiate(d.prefab, transform.position, Quaternion.identity);
                }
            }

            // Add score through the GameManager (null-safe call just in case).
            var gm = FindObjectOfType<GameManager>();
            if (gm) gm.AddScore(points);

            // Play the breaking sound through AudioManager (null-safe).
            AudioManager.I?.PlayBrick();

            // Finally remove this brick object from the scene.
            Destroy(gameObject);
        }
    }
}
