using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button playAgainButton;

    private void Start()
    {
        playAgainButton.onClick.AddListener(() => GameManager.Instance.ResetGame());
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null || GameManager.Instance.Runner == null) return;
        playAgainButton.gameObject.SetActive(GameManager.Instance.Runner.IsServer);
    }
}
