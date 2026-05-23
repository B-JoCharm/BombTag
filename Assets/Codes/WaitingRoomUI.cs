using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomUI : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private BasicSpawner spawner;

    [Header("Timer Selection (Host Only)")]
    [SerializeField] private GameObject timerSettingPanel;
    [SerializeField] private Button timer10Button;
    [SerializeField] private Button timer30Button;
    [SerializeField] private Button timer60Button;

    private float selectedDuration = 30f;
    private static readonly Color highlightColor = new Color(0.3f, 0.7f, 1f);

    private void Start()
    {
        errorText.gameObject.SetActive(false);
        startGameButton.onClick.AddListener(OnStartGame);
        readyButton.onClick.AddListener(OnReady);
        leaveButton.onClick.AddListener(() =>
        {
            StopAllCoroutines();
            errorText.gameObject.SetActive(false);
            spawner.LeaveGame();
        });

        timer10Button.onClick.AddListener(() => SelectDuration(10f));
        timer30Button.onClick.AddListener(() => SelectDuration(30f));
        timer60Button.onClick.AddListener(() => SelectDuration(60f));
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.Object == null || !GameManager.Instance.Object.IsValid) return;
        if (GameManager.Instance.IsGameStarted)
            gameObject.SetActive(false);
    }

    public void Setup(bool isHost)
    {
        StopAllCoroutines();
        errorText.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(isHost);
        readyButton.gameObject.SetActive(!isHost);
        readyButton.interactable = true;
        timerSettingPanel.SetActive(isHost);

        if (isHost)
            SelectDuration(selectedDuration);

        gameObject.SetActive(true);
    }

    private void OnStartGame()
    {
        if (!GameManager.Instance.TryStartGame())
            StartCoroutine(ShowError("Not all players are ready."));
    }

    public void OnGameReset()
    {
        readyButton.interactable = true;
        errorText.gameObject.SetActive(false);

        if (GameManager.Instance != null && GameManager.Instance.Object != null && GameManager.Instance.Object.IsValid)
            SelectDuration(GameManager.Instance.GameDuration);
    }

    private void SelectDuration(float duration)
    {
        selectedDuration = duration;

        if (GameManager.Instance != null && GameManager.Instance.Object != null && GameManager.Instance.Object.IsValid)
            GameManager.Instance.SetGameDuration(duration);

        UpdateTimerButtons();
    }

    private void UpdateTimerButtons()
    {
        SetButtonHighlight(timer10Button, selectedDuration == 10f);
        SetButtonHighlight(timer30Button, selectedDuration == 30f);
        SetButtonHighlight(timer60Button, selectedDuration == 60f);
    }

    private void SetButtonHighlight(Button button, bool selected)
    {
        var colors = button.colors;
        Color target = selected ? highlightColor : Color.white;
        colors.normalColor = target;
        colors.selectedColor = target;
        button.colors = colors;
    }

    private void OnReady()
    {
        readyButton.interactable = false;
        PlayerController.LocalBombHolder?.RPC_SetReady(true);
    }

    private IEnumerator ShowError(string message)
    {
        errorText.text = message;
        errorText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        errorText.gameObject.SetActive(false);
    }
}
