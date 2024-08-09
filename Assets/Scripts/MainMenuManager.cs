using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject optionsPanel;
    public GameObject scorePanel; // Reference to the Score panel with the High Score text
    public Text highScoreText;

    void Start()
    {
        // Load and display the high score
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = "Highest Score: " + highScore.ToString();
        }

        // Ensure score panel is inactive at start
        scorePanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Main_Level");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ShowScores()
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(true);
            highScoreText.text = "Highest Score: " + PlayerPrefs.GetInt("HighScore", 0).ToString();
        }
    }

    public void CloseScorePanel()
    {
        if (scorePanel != null)
        {
            scorePanel.SetActive(false);
        }
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    public void OpenStore()
    {
        // Implement logic to open store
    }
}
