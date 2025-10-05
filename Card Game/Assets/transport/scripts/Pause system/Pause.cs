using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public Canvas pauseCanvas;
    private bool isPaused = false;

    void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            TogglePause();
            Event.current.Use();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        pauseCanvas.gameObject.SetActive(isPaused );
        Time.timeScale = isPaused ? 0 : 1;
    }

    public void OnResumeButtonClicked()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    public void OnPauseButtonClicked()
    {
        if (!isPaused)
        {
            TogglePause();
        }
    }

    public void OnQuitButtonClicked()
    {
        Application.Quit(); 
    }
    public void Backtomenu()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        pauseCanvas.gameObject.SetActive(isPaused);
        SceneManager.LoadScene(0);
    }
}