using UnityEngine;
using System.Collections;

namespace FruitSmash
{
    public class SpeedUpPower : PowerupBase
    {
        public float factor = 1.2f;
        public float duration = 10f;
        protected override void Apply(GameObject paddle)
        {
            foreach (var b in GameObject.FindObjectsOfType<BallController>())
                StartCoroutine(Boost(b));
        }
        IEnumerator Boost(BallController b)
        {
            float oldMax = b.maxSpeed, oldStart = b.startSpeed;
            b.maxSpeed *= factor; b.startSpeed *= factor;
            yield return new WaitForSeconds(duration);
            b.maxSpeed = oldMax; b.startSpeed = oldStart;
        }
    }
}
