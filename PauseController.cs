using UnityEngine;

public class PauseController : MonoBehaviour
{
    [Header("Optional UI")]
    [Tooltip("Panel that shows when the game is paused (can be left empty).")]
    public GameObject pausePanel;

    bool paused = false;

    void Start()
    {
        if (pausePanel) pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        paused = !paused;
        if (pausePanel) pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    // For a UI Resume button
    public void Resume()
    {
        paused = false;
        if (pausePanel) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // Safety: if the object/script gets disabled, unpause the game
    void OnDisable()
    {
        if (paused)
        {
            paused = false;
            Time.timeScale = 1f;
            if (pausePanel) pausePanel.SetActive(false);
        }
    }
}
