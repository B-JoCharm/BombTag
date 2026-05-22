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

    private void Start()
    {
        errorText.gameObject.SetActive(false);
        startGameButton.onClick.AddListener(OnStartGame);
        readyButton.onClick.AddListener(OnReady);
        leaveButton.onClick.AddListener(() => spawner.LeaveGame());
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.Object == null || !GameManager.Instance.Object.IsValid) return;
        if (GameManager.Instance.IsGameStarted)
            gameObject.SetActive(false);
    }

    public void Setup(bool isHost)
    {
        startGameButton.gameObject.SetActive(isHost);
        readyButton.gameObject.SetActive(!isHost);
        readyButton.interactable = true;
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
