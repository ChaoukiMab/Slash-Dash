using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public float timeScore;
    public int killScore;
    public int highScore;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!PlayerController.Instance.isDead)
        {
            timeScore += Time.deltaTime;
        }
    }

    public void AddKill(string enemyType)
    {
        int points = 0;

        if (enemyType == "Turtle")
        {
            points = 120;
        }
        else if (enemyType == "Mage")
        {
            points = 250;
        }

        killScore += points;
        Debug.Log($"Kill added! Enemy Type: {enemyType}, Points: {points}, Total Kill Score: {killScore}");
    }

    public void CheckHighScore()
    {
        int currentScore = Mathf.FloorToInt(timeScore) + killScore;
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
        Debug.Log($"High Score Checked: {highScore}");
    }

    public void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    public void ResetScore()
    {
        timeScore = 0;
        killScore = 0;
        Debug.Log("Scores reset.");
    }
}
