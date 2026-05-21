using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TMP_InputField gameCodeInput;
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button closeButton;
    [SerializeField] private BasicSpawner spawner;

    private void Start()
    {
        errorPanel.SetActive(false);
        createGameButton.onClick.AddListener(OnCreateGame);
        joinGameButton.onClick.AddListener(OnJoinGame);
        closeButton.onClick.AddListener(() => errorPanel.SetActive(false));
    }

    private void OnCreateGame()
    {
        if (!ValidateInputs()) return;
        SetButtonsInteractable(false);
        spawner.CreateGame(nicknameInput.text.Trim(), gameCodeInput.text.Trim());
    }

    private void OnJoinGame()
    {
        if (!ValidateInputs()) return;
        SetButtonsInteractable(false);
        spawner.JoinGame(nicknameInput.text.Trim(), gameCodeInput.text.Trim());
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(nicknameInput.text))
        {
            ShowError("Please enter a nickname.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(gameCodeInput.text))
        {
            ShowError("Please enter a room code.");
            return false;
        }
        return true;
    }

    public void ShowError(string message)
    {
        errorPanel.SetActive(true);
        errorText.text = message;
        SetButtonsInteractable(true);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        createGameButton.interactable = interactable;
        joinGameButton.interactable = interactable;
    }

    public void HideLobby()
    {
        gameObject.SetActive(false);
    }
}
