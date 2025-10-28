/*
 * Handles main game flow: waves, score, lives, ball spawning, and UI updates.
 * I tried to keep things readable for later debugging since everything ties together here.
 */

using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Player State")]
    public int lives = 3;   // how many tries the player starts with
    public int wave = 1;    // which level layout we’re on
    int score = 0;          // I keep score private so only AddScore() changes it

    [Header("UI References")]
    // Hook these up in the Inspector (Score, Lives, Wave text objects)
    public TMP_Text scoreText;
    public TMP_Text livesText;
    public TMP_Text waveText;

    [Header("Brick Prefabs / Grid")]
    // Each fruit brick prefab has different point values or behaviors
    public GameObject brickApple;
    public GameObject brickBanana;
    public GameObject brickStrawberry;
    public Transform gridParent; // all bricks get spawned under this parent

    [Header("Ball Management")]
    // Ball is spawned from prefab, not placed manually in the scene
    public GameObject ballPrefab;
    public GameObject currentBall;
    public float respawnDelay = 0.25f; // small delay before next ball appears

    void Start()
    {
        // Start up the first wave and show the HUD
        SpawnBall();
        BuildWave();
        UpdateUI();
    }

    // spawns a ball prefab just above the paddle (if found)
    public void SpawnBall()
    {
        // safety check in case something left over in scene
        if (currentBall != null) Destroy(currentBall);

        if (ballPrefab == null)
        {
            Debug.LogError("Ball prefab missing! Assign GameManager.ballPrefab in Inspector.");
            return;
        }

        Vector3 spawnPos = GetPaddleSpawnPos();
        currentBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        currentBall.name = "Ball";
    }

    // finds where to put the ball; usually just above the paddle
    Vector3 GetPaddleSpawnPos()
    {
        var paddle = GameObject.FindGameObjectWithTag("Paddle");
        if (paddle)
            return paddle.transform.position + Vector3.up * 0.5f;
        // fallback spot if paddle somehow not found
        return new Vector3(0f, -3.5f, 0f);
    }

    // adds points to total and refreshes HUD
    public void AddScore(int value)
    {
        score += value;
        UpdateUI();
    }

    // called by BallController when the ball hits the LoseZone
    public void OnBallLost(BallController ball)
    {
        Debug.Log($"OnBallLost called. Lives BEFORE: {lives}");

        if (ball != null)
            Destroy(ball.gameObject);

        // reduce lives safely (no negative numbers)
        lives = Mathf.Max(0, lives - 1);

        // sound cue for losing a life
        AudioManager.I?.PlayLoseLife();

        UpdateUI();

        if (lives <= 0)
        {
            // final life lost — show Game Over UI + stop time
            ShowGameOver();
            AudioManager.I?.PlayGameOver();
            return;
        }

        // delay helps prevent immediate respawn collisions
        StartCoroutine(RespawnAfter(respawnDelay));
    }

    IEnumerator RespawnAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnBall();
    }

    // handles end-of-game logic and showing the UI
    void ShowGameOver()
    {
        Time.timeScale = 0f;
        Debug.Log("Game Over");

        // prefer using a proper UI script if present
        var ui = Object.FindFirstObjectByType<GameOverUI>();
        if (ui != null)
            ui.Show(score);
        else
            GameObject.Find("GameOverPanel")?.SetActive(true);
    }

    // UI refresh function — called anytime score/lives/wave change
    void UpdateUI()
    {
        if (scoreText)
            scoreText.text = $"Score: {score}";
        if (livesText)
            livesText.text = $"Lives: {lives}";
        if (waveText)
            waveText.text = $"Wave: {wave}";
    }

    // builds a grid of fruit bricks for the current wave
    void BuildWave()
    {
        if (!gridParent)
        {
            Debug.LogError("Assign gridParent.");
            return;
        }

        // clear any leftover bricks first
        for (int i = gridParent.childCount - 1; i >= 0; --i)
            Destroy(gridParent.GetChild(i).gameObject);

        // layout settings for columns/rows
        int cols = 12;
        int rows = Mathf.Min(3 + wave / 2, 6);
        float startX = -7f, startY = 3.5f, dx = 1.2f, dy = 0.6f;

        // harder waves add more fruit types
        var pool = (brickStrawberry && wave >= 3)
            ? new GameObject[] { brickApple, brickBanana, brickStrawberry }
            : new GameObject[] { brickApple, brickBanana };

        // spawn bricks in a simple grid pattern
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var prefab = pool[Random.Range(0, pool.Length)];
                if (!prefab) continue;

                Vector3 pos = new Vector3(startX + c * dx, startY - r * dy, 0);
                Instantiate(prefab, pos, Quaternion.identity, gridParent);
            }
        }

        // check for wave completion later
        StartCoroutine(CheckCleared());
    }

    // keeps watching the grid until no bricks left, then builds next wave
    IEnumerator CheckCleared()
    {
        while (true)
        {
            if (gridParent.childCount == 0)
            {
                wave++;
                // small bonus life every 3rd wave
                if (wave % 3 == 0) lives++;

                BuildWave();
                UpdateUI();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
