using UnityEngine;


public class BombHolder : MonoBehaviour
{
    public bool HasBomb { get; private set; }

    public void ReceiveBomb()
    {
        HasBomb = true;
    }

    public void RemoveBomb()
    {
        HasBomb = false;
    }
}