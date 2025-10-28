using UnityEngine;

namespace FruitSmash
{
    public class SplitPower : PowerupBase
    {
        public float angle = 15f;
        protected override void Apply(GameObject paddle)
        {
            var ball = Object.FindFirstObjectByType<BallController>();
            if (!ball) return;
            var clone = Instantiate(ball.gameObject, ball.transform.position, Quaternion.identity);
            var rb = clone.GetComponent<Rigidbody2D>();
            var rb0 = ball.GetComponent<Rigidbody2D>();
            if (rb && rb0) rb.linearVelocity = Quaternion.Euler(0,0,angle) * rb0.linearVelocity;
        }
    }
}
