using Fusion;

public class BombHolder : NetworkBehaviour
{
    [Networked]
    public NetworkBool HasBomb { get; set; }

    [Networked]
    public NetworkString<_32> Nickname { get; set; }

    [Networked] public NetworkBool IsReady { get; set; }
    [Networked] public int SpawnIndex { get; set; }
    [Networked, OnChangedRender(nameof(OnCharacterIndexChanged))]
    public int CharacterIndex { get; set; }

    private void OnCharacterIndexChanged()
    {
        GetComponent<PlayerController>().ApplyCharacter(CharacterIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickname(string nickname)
    {
        Nickname = nickname;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady(bool ready)
    {
        IsReady = ready;
    }

    public void ReceiveBomb()
    {
        HasBomb = true;
    }

    public void RemoveBomb()
    {
        HasBomb = false;
    }
}