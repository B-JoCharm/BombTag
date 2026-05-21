using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 moveInput;
    public NetworkButtons buttons;
}

public enum PlayerInputButton
{
    PassBomb = 0
}