using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private BombObject bombObject;

    private BombHolder myBombHolder;
    private BombHolder nearbyPlayer;

    private void Awake()
    {
        myBombHolder = GetComponent<BombHolder>();
    }

    private void Start()
    {
        myBombHolder.ReceiveBomb();
        bombObject.SetOwner(myBombHolder);
    }

    private void Update()
    {
        Move();

        if (Input.GetKeyDown(KeyCode.X))
        {
            TryPassBomb();
        }
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(h, v, 0) * speed * Time.deltaTime);
    }

    private void TryPassBomb()
    {
        if (!myBombHolder.HasBomb) return;
        if (nearbyPlayer == null) return;

        myBombHolder.RemoveBomb();
        nearbyPlayer.ReceiveBomb();

        bombObject.SetOwner(nearbyPlayer);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BombHolder bombHolder = other.GetComponent<BombHolder>();

        if (bombHolder != null && bombHolder != myBombHolder)
        {
            nearbyPlayer = bombHolder;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BombHolder bombHolder = other.GetComponent<BombHolder>();

        if (bombHolder != null && bombHolder == nearbyPlayer)
        {
            nearbyPlayer = null;
        }
    }
}
