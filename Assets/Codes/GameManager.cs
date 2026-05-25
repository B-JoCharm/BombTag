using Fusion;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Game")]
    [Networked] public float CurrentTime { get; set; }
    [Networked] public float GameDuration { get; set; }
    [Networked, OnChangedRender(nameof(OnGameStartedChanged))]
    public NetworkBool IsGameStarted { get; set; }
    [Networked, OnChangedRender(nameof(OnGameOverChanged))]
    public NetworkBool IsGameOver { get; set; }
    [Networked] public BombHolder CurrentBombOwner { get; set; }

    private List<BombHolder> players = new List<BombHolder>();
    public IReadOnlyList<BombHolder> Players => players;

    private List<int> availableCharacters = new List<int> { 0, 1, 2, 3 };

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

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

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            GameDuration = 30f;
    }

    public void SetGameDuration(float duration)
    {
        if (!Object.HasStateAuthority) return;
        GameDuration = duration;
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
        players.RemoveAll(p =>
        {
            if (p == null || p.Object == null || !p.Object.IsValid)
            {
                if (p != null && !availableCharacters.Contains(p.CharacterIndex))
                    availableCharacters.Add(p.CharacterIndex);
                return true;
            }
            return false;
        });

        if (players.Contains(player)) return;

        players.Add(player);

        if (Object != null && Object.IsValid && Object.HasStateAuthority && availableCharacters.Count > 0)
        {
            player.SpawnIndex = players.Count - 1;

            int randomIdx = Random.Range(0, availableCharacters.Count);
            player.CharacterIndex = availableCharacters[randomIdx];
            availableCharacters.RemoveAt(randomIdx);
        }
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

        if (!availableCharacters.Contains(player.CharacterIndex))
            availableCharacters.Add(player.CharacterIndex);

        if (!wasBombOwner) return;

        bombObject.ClearOwner();
        CurrentBombOwner = null;

        if (players.Count >= 1 && IsGameStarted && !IsGameOver)
            GiveRandomPlayerBomb();
    }

    public bool TryStartGame()
    {
        if (Object == null || !Object.IsValid || !Object.HasStateAuthority) return false;

        players.RemoveAll(p => p == null || p.Object == null || !p.Object.IsValid);

        if (players.Count < 2) return false;

        foreach (var player in players)
        {
            if (!player.IsReady) return false;
        }

        IsGameStarted = true;
        CurrentTime = GameDuration;
        GiveRandomPlayerBomb();
        return true;
    }

    private void GameOver()
    {
        IsGameOver = true;
    }

    public Vector3 GetSpawnPoint(int index)
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return Vector3.zero;
        return spawnPoints[Mathf.Clamp(index, 0, spawnPoints.Length - 1)].position;
    }

    private void OnGameStartedChanged()
    {
        if (IsGameStarted)
        {
            waitingRoomUI.gameObject.SetActive(false);
            gamePanel.SetActive(true);
            gameOverPanel.SetActive(false);

            if (Object.HasStateAuthority && spawnPoints != null)
            {
                foreach (var player in players)
                    player.transform.position = GetSpawnPoint(player.SpawnIndex);
            }
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

        if (spawnPoints != null)
        {
            foreach (var player in players)
                player.transform.position = GetSpawnPoint(player.SpawnIndex);
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
