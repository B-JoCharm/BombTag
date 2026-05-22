using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private LobbyUI lobbyUI;
    [SerializeField] private WaitingRoomUI waitingRoomUI;

    public static string LocalNickname { get; private set; }

    private NetworkRunner runner;
    private bool isConnected = false;
    private bool isLeavingVoluntarily = false;

    public void CreateGame(string nickname, string gameCode)
    {
        LocalNickname = nickname;
        StartGame(GameMode.Host, gameCode);
    }

    public void JoinGame(string nickname, string gameCode)
    {
        LocalNickname = nickname;
        StartGame(GameMode.Client, gameCode);
    }

    private async void StartGame(GameMode mode, string sessionName)
    {
        var runnerGO = new GameObject("NetworkRunner");
        runner = runnerGO.AddComponent<NetworkRunner>();
        runner.AddCallbacks(this);
        runner.ProvideInput = true;

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(gameObject.scene.buildIndex),
            SceneManager = runnerGO.AddComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            string error = mode == GameMode.Host
                ? "Room code already in use."
                : "Room doesn't exist.";
            lobbyUI.ShowError(error);
            runner = null;
            return;
        }

        lobbyUI.HideLobby();
        StartCoroutine(ShowUIAfterJoin(mode == GameMode.Host));
    }

    private IEnumerator ShowUIAfterJoin(bool isHost)
    {
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            GameManager.Instance.Object != null &&
            GameManager.Instance.Object.IsValid);

        if (!GameManager.Instance.IsGameStarted)
            waitingRoomUI.Setup(isHost);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
            isConnected = true;

        if (runner.IsServer)
        {
            Vector3 spawnPosition = new(player.RawEncoded * 2, 0, 0);

            NetworkObject networkPlayerObject = runner.Spawn(
                playerPrefab,
                spawnPosition,
                Quaternion.identity,
                player
            );

            runner.SetPlayerObject(player, networkPlayerObject);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new()
        {
            moveInput = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        data.buttons.Set(PlayerInputButton.PassBomb, Input.GetKey(KeyCode.X));

        input.Set(data);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        NetworkObject playerObject = runner.GetPlayerObject(player);
        if (playerObject == null) return;

        BombHolder leavingPlayer = playerObject.GetComponent<BombHolder>();
        GameManager.Instance.UnregisterPlayer(leavingPlayer);

        runner.Despawn(playerObject);
    }

    public void LeaveGame()
    {
        isLeavingVoluntarily = true;
        runner.Shutdown();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (!isLeavingVoluntarily && !runner.IsServer && isConnected && GameManager.Instance != null)
            GameManager.Instance.ShowHostDisconnectedMessage();

        isLeavingVoluntarily = false;
        isConnected = false;
        this.runner = null;

        waitingRoomUI.gameObject.SetActive(false);
        lobbyUI.ShowLobby();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSceneLoadDone(NetworkRunner runner) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
