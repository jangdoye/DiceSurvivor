using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static bool isPaused = false;
    public GameObject pausePanel; // Pause UI Panel

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pausePanel.SetActive(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        // 씬 전환 또는 종료
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // 예시
    }
}
