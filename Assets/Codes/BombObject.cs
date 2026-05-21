using UnityEngine;

public class BombObject : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new(0, 0.8f, 0);

    private BombHolder owner;

    public void ClearOwner()
    {
        owner = null;
    }

    public void SetOwner(BombHolder newOwner)
    {
        if (owner != null)
            owner.RemoveBomb();

        owner = newOwner;
        owner.ReceiveBomb();

        GameManager.Instance.CurrentBombOwner = owner;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.Object == null || !GameManager.Instance.Object.IsValid) return;

        BombHolder bombOwner = GameManager.Instance.CurrentBombOwner;

        if (bombOwner == null) return;

        transform.position = bombOwner.transform.position + offset;
    }
}
