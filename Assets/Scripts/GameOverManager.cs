using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverScreen;
    public Text scoreText;
    public Text highScoreText;

    void Start()
    {
        ScoreManager.Instance.LoadHighScore();
    }

    public void ShowGameOver()
    {
        ScoreManager.Instance.CheckHighScore();
        gameOverScreen.SetActive(true); // Show the Game Over screen
        scoreText.text = "Score: " + Mathf.FloorToInt(ScoreManager.Instance.timeScore + ScoreManager.Instance.killScore).ToString();
        highScoreText.text = "High Score: " + ScoreManager.Instance.highScore.ToString();

        // Ensure the game is paused
        Time.timeScale = 0f; // Pause the game
        Debug.Log("Game Over! Time scale set to 0.");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume the game
        ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Resume the game
        ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene("MainMenu"); // Load the Main Menu scene
    }
}
