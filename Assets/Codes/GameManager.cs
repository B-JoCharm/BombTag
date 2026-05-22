using Fusion;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Game")]
    [Networked] public float CurrentTime { get; set; }
    [Networked, OnChangedRender(nameof(OnGameStartedChanged))]
    public NetworkBool IsGameStarted { get; set; }
    [Networked, OnChangedRender(nameof(OnGameOverChanged))]
    public NetworkBool IsGameOver { get; set; }
    [Networked] public BombHolder CurrentBombOwner { get; set; }

    private List<BombHolder> players = new List<BombHolder>();
    public IReadOnlyList<BombHolder> Players => players;

    [Header("Bomb")]
    [SerializeField] private BombObject bombObject;

    [Header("UI")]
    [SerializeField] private WaitingRoomUI waitingRoomUI;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    private void Awake()
    {
        Instance = this;
    }




    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (!IsGameStarted) return;
        if (IsGameOver) return;
        if (CurrentBombOwner == null) return;

        CurrentTime -= Runner.DeltaTime;

        if (CurrentTime <= 0)
        {
            CurrentTime = 0;
            GameOver();
        }
    }

    public void RegisterPlayer(BombHolder player)
    {
        if (players.Contains(player)) return;

        players.Add(player);
    }

    private void GiveRandomPlayerBomb()
    {
        if (players.Count == 0) return;

        int randomIndex = Random.Range(0, players.Count);
        BombHolder selectedPlayer = players[randomIndex];

        bombObject.SetOwner(selectedPlayer);
    }

    public void PassBomb(BombHolder newOwner)
    {
        if (!Object.HasStateAuthority) return;
        if (IsGameOver) return;
        if (newOwner == null) return;
        if (newOwner == CurrentBombOwner) return;

        bombObject.SetOwner(newOwner);
    }

    public void UnregisterPlayer(BombHolder player)
    {
        if (!players.Contains(player)) return;
        if (Object == null || !Object.IsValid) return;

        bool wasBombOwner = CurrentBombOwner == player;
        players.Remove(player);

        if (!wasBombOwner) return;

        bombObject.ClearOwner();
        CurrentBombOwner = null;

        if (players.Count >= 1 && IsGameStarted && !IsGameOver)
            GiveRandomPlayerBomb();
    }

    public bool TryStartGame()
    {
        if (!Object.HasStateAuthority) return false;
        if (players.Count < 2) return false;

        foreach (var player in players)
        {
            if (!player.IsReady) return false;
        }

        IsGameStarted = true;
        CurrentTime = 10f;
        GiveRandomPlayerBomb();
        return true;
    }

    private void GameOver()
    {
        IsGameOver = true;
    }

    private void OnGameStartedChanged()
    {
        if (IsGameStarted)
        {
            waitingRoomUI.gameObject.SetActive(false);
            gamePanel.SetActive(true);
            gameOverPanel.SetActive(false);
        }
        else
        {
            gamePanel.SetActive(false);
            waitingRoomUI.gameObject.SetActive(true);
            waitingRoomUI.OnGameReset();
        }
    }

    private void OnGameOverChanged()
    {
        if (!IsGameOver)
        {
            gameOverPanel.SetActive(false);
            return;
        }

        string loserName = CurrentBombOwner != null
            ? CurrentBombOwner.Nickname.ToString()
            : "Unknown Player";

        gameOverPanel.SetActive(true);
        resultText.text = $"GAME OVER\n{loserName} Lose";
    }

    public void ResetGame()
    {
        if (!Object.HasStateAuthority) return;

        bombObject.ClearOwner();
        CurrentBombOwner = null;
        CurrentTime = 0;

        foreach (var player in players)
        {
            if (player.Object.InputAuthority == Runner.LocalPlayer)
                player.IsReady = true;
            else
                player.IsReady = false;
        }

        IsGameOver = false;
        IsGameStarted = false;
    }

    public void ShowHostDisconnectedMessage()
    {
        gameOverPanel.SetActive(true);
        resultText.text = "Host disconnected.";
    }
}
