using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game")]
    public float currentTime = 10f;
    public BombHolder currentBombOwner;
    public bool isGameOver = false;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        string loserName = currentBombOwner.gameObject.name;

        gameOverPanel.SetActive(true);
        resultText.text = $"GAME OVER\n{loserName} Lose";

        Time.timeScale = 0f;
    }
}