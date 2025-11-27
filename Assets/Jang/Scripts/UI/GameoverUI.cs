using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverUI;

    private void Start()
    {
        gameOverUI.SetActive(false); 
    }

    public void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);
        Time.timeScale = 0f; // 게임 멈춤
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        //적 스폰과 풀을 초기화
        DiceSurvivor.Manager.EnemySpawnManager.Instance.ResetSpawn();
        DiceSurvivor.Manager.EnemyPoolManager.Instance.ResetPool();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
