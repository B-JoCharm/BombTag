using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    public static BombHolder LocalBombHolder { get; private set; }

    private BombHolder myBombHolder;
    private BombHolder nearbyPlayerBombHolder;
    private NetworkButtons _prevButtons;

    private void Awake()
    {
        myBombHolder = GetComponent<BombHolder>();
    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Move(data.moveInput);

            bool justPressed = data.buttons.IsSet(PlayerInputButton.PassBomb)
                            && !_prevButtons.IsSet(PlayerInputButton.PassBomb);

            if (justPressed)
                TryPassBomb();

            _prevButtons = data.buttons;
        }
    }

    public override void Spawned()
    {
        gameObject.name = $"Player {Object.InputAuthority.RawEncoded}";

        if (Object.HasInputAuthority)
        {
            LocalBombHolder = myBombHolder;
            myBombHolder.RPC_SetNickname(BasicSpawner.LocalNickname);

            if (Runner.IsServer)
                myBombHolder.IsReady = true;
        }

        GameManager.Instance.RegisterPlayer(myBombHolder);
    }

    private void Move(Vector2 moveInput)
    {
        Vector3 moveDir = new Vector3(moveInput.x, moveInput.y, 0f);

        transform.Translate(moveDir.normalized * speed * Runner.DeltaTime);
    }

    private void TryPassBomb()
    {
        if (!myBombHolder.HasBomb) return;
        if (nearbyPlayerBombHolder == null) return;

        GameManager.Instance.PassBomb(nearbyPlayerBombHolder);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BombHolder bombHolder = other.GetComponent<BombHolder>();

        if (bombHolder != null && bombHolder != myBombHolder)
        {
            nearbyPlayerBombHolder = bombHolder;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BombHolder bombHolder = other.GetComponent<BombHolder>();

        if (bombHolder != null && bombHolder == nearbyPlayerBombHolder)
        {
            nearbyPlayerBombHolder = null;
        }
    }
}
