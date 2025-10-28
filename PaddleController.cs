/*
 * Fruit Smash (Justin Nam) — PaddleController
 * Purpose: deterministic, readable paddle movement that supports mouse or keyboard.
 */
using UnityEngine;

namespace FruitSmash
{
    public class PaddleController : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 10f;
        public float mouseFollow = 12f;

        [Header("Bounds")]
        public float minX = -7.5f;
        public float maxX =  7.5f;

        void Update()
        {
            float dx = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;

            if (Mathf.Approximately(dx, 0f) && Input.mousePresent)
            {
                float targetX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
                float delta = Mathf.Clamp(targetX - transform.position.x,
                                          -mouseFollow * Time.deltaTime,
                                           mouseFollow * Time.deltaTime);
                dx = delta;
            }

            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x + dx, minX, maxX);
            transform.position = p;
        }
    }
}
