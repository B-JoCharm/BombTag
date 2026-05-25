using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;

    [Header("Map Bounds")]
    [SerializeField] private float mapHalfWidth = 9f;
    [SerializeField] private float mapHalfHeight = 5f;

    [Header("Characters")]
    [SerializeField] private RuntimeAnimatorController[] characterAnimators;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;

    public static BombHolder LocalBombHolder { get; private set; }

    private BombHolder myBombHolder;
    private BombHolder nearbyPlayerBombHolder;
    private NetworkButtons _prevButtons;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D physicsCollider;
    private Vector3 previousPosition;
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        myBombHolder = GetComponent<BombHolder>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        physicsCollider = GetComponent<BoxCollider2D>();
    }

    public override void Render()
    {
        Vector3 delta = transform.position - previousPosition;
        bool isMoving = delta.magnitude > 0.001f;

        animator.SetBool(IsMovingHash, isMoving);

        if (delta.x < -0.001f)
            spriteRenderer.flipX = true;
        else if (delta.x > 0.001f)
            spriteRenderer.flipX = false;

        previousPosition = transform.position;
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
        ApplyCharacter(myBombHolder.CharacterIndex);
    }

    public void ApplyCharacter(int index)
    {
        if (characterAnimators == null || index >= characterAnimators.Length) return;
        animator.runtimeAnimatorController = characterAnimators[index];
    }

    private void Move(Vector2 moveInput)
    {
        if (moveInput == Vector2.zero) return;

        Vector2 dir = moveInput.normalized;
        float dist = speed * Runner.DeltaTime;
        Vector2 size = physicsCollider.size * 0.9f;

        if (dir.x != 0)
        {
            Vector2 origin = (Vector2)transform.position + physicsCollider.offset;
            Vector2 xDir = new(Mathf.Sign(dir.x), 0f);
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, xDir, Mathf.Abs(dir.x) * dist, obstacleLayer);
            float moveX = hit.collider != null ? Mathf.Max(0f, hit.distance - 0.02f) : Mathf.Abs(dir.x) * dist;
            transform.position += new Vector3(xDir.x * moveX, 0f, 0f);
        }

        if (dir.y != 0)
        {
            Vector2 origin = (Vector2)transform.position + physicsCollider.offset;
            Vector2 yDir = new(0f, Mathf.Sign(dir.y));
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, yDir, Mathf.Abs(dir.y) * dist, obstacleLayer);
            float moveY = hit.collider != null ? Mathf.Max(0f, hit.distance - 0.02f) : Mathf.Abs(dir.y) * dist;
            transform.position += new Vector3(0f, yDir.y * moveY, 0f);
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -mapHalfWidth, mapHalfWidth);
        pos.y = Mathf.Clamp(pos.y, -mapHalfHeight, mapHalfHeight);
        transform.position = pos;
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
