/*
 * Fruit Smash — BallController
 */

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BallController : MonoBehaviour
{
    [Header("Speed (game feel)")]
    [Tooltip("Where the ball starts speed-wise after launch.")]
    public float startSpeed = 7f;

    [Tooltip("Never allow the ball to exceed this (prevents tunneling even with Continuous).")]
    public float maxSpeed = 15f;

    [Tooltip("Never allow the ball to slow below this (prevents crawl after weird bounces).")]
    public float minSpeed = 6f;

    [Header("Bounce shaping")]
    [Tooltip("0..1 of vertical component required; higher = less horizontal skimming.")]
    [Range(0f, 0.95f)] public float minYAngle = 0.20f;   // ~11.5°

    [Tooltip("Tiny kick after paddle hit so rallies feel snappier.")]
    public float speedGainOnHit = 0.10f;

    [Tooltip("Keep paddle exits at least this far from straight up (degrees).")]
    [Range(0f, 45f)] public float minAngleFromUp = 18f;

    [Header("Stuck prevention")]
    [Tooltip("Small impulse applied if the ball barely moves for a moment.")]
    public float nudgeImpulse = 0.25f;

    [Tooltip("Seconds of low movement before applying a nudge.")]
    public float stuckSeconds = 0.5f;

    [Header("Look")]
    [Tooltip("Assign coconut so every respawn forces this sprite.")]
    public Sprite coconutSprite;

    // --- cached refs ---
    Rigidbody2D rb;
    SpriteRenderer sr;

    // --- runtime state ---
    Vector2 lastPos;
    float stillTime;
    bool launched;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // Make sure we always look like the coconut whenever the ball is enabled (spawn/respawn).
        if (coconutSprite && sr) sr.sprite = coconutSprite;
    }

    void Start()
    {
        // Scene starts with the ball "stuck" to the paddle until Space.
        StickToPaddle();
    }

    void Update()
    {
        // Simple launch input. I keep input here (not FixedUpdate) on purpose.
        if (!launched && Input.GetKeyDown(KeyCode.Space))
            Launch();
    }

    void FixedUpdate()
    {
        // 1) Clamp speed every physics step so we neither crawl nor blast out of control.
        if (launched)
        {
            Vector2 v = rb.linearVelocity;
            float spd = Mathf.Clamp(v.magnitude, minSpeed, maxSpeed);
            // If velocity is almost zero, give it a neutral diagonal so it’s not stuck.
            rb.linearVelocity = (v.sqrMagnitude > 0.0001f ? v.normalized : new Vector2(0.707f, 0.707f)) * spd;
        }

        // 2) If we basically didn't move, build up a timer and give a tiny nudge.
        bool barelyMoved = Vector2.Distance(transform.position, lastPos) < 0.01f
                           || rb.linearVelocity.magnitude < (minSpeed * 0.5f);

        if (barelyMoved)
        {
            stillTime += Time.fixedDeltaTime;
            if (stillTime > stuckSeconds)
            {
                rb.AddForce(Random.insideUnitCircle.normalized * nudgeImpulse, ForceMode2D.Impulse);
                stillTime = 0f;
            }
        }
        else
        {
            stillTime = 0f;
        }

        lastPos = transform.position;
    }

    void StickToPaddle()
    {
        // Puts ball just above paddle, reset velocity.
        var paddle = GameObject.FindGameObjectWithTag("Paddle");
        if (paddle) transform.position = paddle.transform.position + Vector3.up * 0.5f;

        rb.linearVelocity = Vector2.zero;
        launched = false;
    }

    void Launch()
    {
        // I aim generally upward with random left/right so the start isn't deterministic.
        launched = true;
        float dirX = Random.value < 0.5f ? -1f : 1f;
        rb.linearVelocity = new Vector2(dirX, 1f).normalized * startSpeed;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        // Paddle: steer based on contact offset, add a tiny speed gain, and ensure angle correctness.
        if (col.collider.CompareTag("Paddle"))
        {
            AudioManager.I?.PlayPaddle();

            float x = (transform.position.x - col.transform.position.x) / (col.collider.bounds.size.x * 0.5f);
            Vector2 dir = new Vector2(x, 1f).normalized;

            rb.linearVelocity = dir * Mathf.Min(rb.linearVelocity.magnitude + speedGainOnHit, maxSpeed);

            // Keep exits from being too vertical (feels bad if it yo-yos straight up).
            float angle = Vector2.Angle(Vector2.up, rb.linearVelocity);
            if (angle < minAngleFromUp)
                rb.linearVelocity = Quaternion.AngleAxis(minAngleFromUp - angle, Vector3.forward) * rb.linearVelocity;
        }
        else
        {
            // Non-paddle collisions (walls/bricks) can share the wall sound.
            AudioManager.I?.PlayWall();
        }

        // After ANY collision, make sure Y component isn't too tiny (prevents long wall skims).
        EnforceMinimumYAngle();
    }

    void EnforceMinimumYAngle()
    {
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude < 0.0001f) return;

        Vector2 n = v.normalized;
        float ay = Mathf.Abs(n.y);
        if (ay < minYAngle)
        {
            // Keep horizontal sign, enforce a minimum vertical component.
            float sx = Mathf.Sign(n.x == 0 ? 1 : n.x);
            float sy = Mathf.Sign(n.y == 0 ? 1 : n.y);

            // Build a direction with at least minYAngle vertical component.
            Vector2 dir = new Vector2(
                sx * Mathf.Sqrt(Mathf.Max(0f, 1f - (minYAngle * minYAngle))),   // horizontal part
                sy * minYAngle                                                   // vertical part
            );

            rb.linearVelocity = dir.normalized * Mathf.Max(minSpeed, v.magnitude);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // LoseZone MUST be a Trigger. I notify GameManager and let it handle lives/respawn.
        if (other.CompareTag("LoseZone"))
        {
            Debug.Log("Ball hit LoseZone – calling GameManager.OnBallLost");
            var gm = FindObjectOfType<GameManager>();
            if (!gm) Debug.LogError("No GameManager found in scene!");
            else gm.OnBallLost(this);
        }
    }

    // Editor nicety: keep tunables in sane ranges even if I drag sliders wildly.
    void OnValidate()
    {
        maxSpeed = Mathf.Max(maxSpeed, 0.01f);
        minSpeed = Mathf.Clamp(minSpeed, 0.01f, maxSpeed);
        stuckSeconds = Mathf.Max(0f, stuckSeconds);
        nudgeImpulse = Mathf.Max(0f, nudgeImpulse);
    }
}
