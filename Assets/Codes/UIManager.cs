using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject waitingRoomPanel;
    [SerializeField] private GameObject gamePanel;

    private void Awake()
    {
        lobbyPanel.SetActive(true);
        waitingRoomPanel.SetActive(false);
        gamePanel.SetActive(false);
    }
}
