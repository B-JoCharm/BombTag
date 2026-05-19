using UnityEngine;

public class BombObject : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0);

    private BombHolder owner;

    public void SetOwner(BombHolder newOwner)
    {
        owner = newOwner;
        GameManager.Instance.currentBombOwner = newOwner;
    }

    private void Update()
    {
        if (owner == null) return;

        transform.position = owner.transform.position + offset;
    }
}