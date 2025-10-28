/*
 * Fruit Smash (Justin Nam) — PowerupBase
 */
using UnityEngine;

namespace FruitSmash
{
    public abstract class PowerupBase : MonoBehaviour
    {
        public float fallSpeed = 2.5f;
        void Update() => transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Paddle")) { Apply(col.gameObject); Destroy(gameObject); }
            else if (col.CompareTag("LoseZone")) Destroy(gameObject);
        }
        protected abstract void Apply(GameObject paddle);
    }
}
