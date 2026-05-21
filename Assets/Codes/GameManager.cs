using Fusion;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Game")]
    [Networked] public float CurrentTime { get; set; }
    [Networked, OnChangedRender(nameof(OnGameOverChanged))]
    public NetworkBool IsGameOver { get; set; }
    [Networked] public BombHolder CurrentBombOwner { get; set; }

    private List<BombHolder> players = new List<BombHolder>();

    [Header("Bomb")]
    [SerializeField] private BombObject bombObject;

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

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            CurrentTime = 10f;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
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

        if (!Object.HasStateAuthority) return;

        if (players.Count >= 2 && CurrentBombOwner == null)
            GiveRandomPlayerBomb();
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

        bool wasBombOwner = CurrentBombOwner == player;
        players.Remove(player);

        if (!wasBombOwner) return;

        bombObject.ClearOwner();
        CurrentBombOwner = null;

        if (players.Count >= 1)
            GiveRandomPlayerBomb();
    }

    private void GameOver()
    {
        IsGameOver = true;
    }

    private void OnGameOverChanged()
    {
        string loserName = CurrentBombOwner != null
            ? CurrentBombOwner.gameObject.name
            : "Unknown Player";

        gameOverPanel.SetActive(true);
        resultText.text = $"GAME OVER\n{loserName} Lose";
    }

    public void ShowHostDisconnectedMessage()
    {
        gameOverPanel.SetActive(true);
        resultText.text = "Host disconnected.";
    }
}
