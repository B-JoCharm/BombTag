using Fusion;

public class BombHolder : NetworkBehaviour
{
    [Networked]
    public NetworkBool HasBomb { get; set; }

    public void ReceiveBomb()
    {
        HasBomb = true;
    }

    public void RemoveBomb()
    {
        HasBomb = false;
    }
}