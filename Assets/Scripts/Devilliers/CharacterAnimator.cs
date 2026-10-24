using UnityEngine;

/// <summary>
/// Simple sprite-swap animator for a 2D platformer.
/// Sprites: 1 idle, 3 walk, 3 jump (rise, apex, fall). All sprites face RIGHT.
/// Put this on the same GameObject as your SpriteRenderer and Rigidbody2D.
/// It does NOT move the character; it only reads state and swaps sprites.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterAnimator : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite[] walkSprites = new Sprite[3];
    [Tooltip("Order: 0 = rising, 1 = apex, 2 = falling")]
    public Sprite[] jumpSprites = new Sprite[3];

    [Header("Animation Speed (seconds per frame)")]
    public float walkFrameTime = 0.12f;
    public float sprintFrameTime = 0.06f;
    public float doubleJumpFrameTime = 0.05f;

    [Header("Keys")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    [Tooltip("Optional: assign your controller's ground check transform. If empty, a point under the feet is used.")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer = ~0;
    [Tooltip("Used only if no groundCheck transform is assigned.")]
    public float feetOffset = 0.5f;

    [Header("Jump Pose Thresholds (vertical velocity)")]
    public float apexThreshold = 1.5f;

    SpriteRenderer sr;
    Rigidbody2D rb;

    bool isGrounded;
    bool wasGrounded;
    int airJumpsUsed;          // 0 = none, 1 = double jump used
    bool playingDoubleJump;
    int doubleJumpFrame;

    int walkFrame;
    float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (idleSprite == null) idleSprite = sr.sprite;
    }

    void Update()
    {
        CheckGround();
        DetectJumps();

        if (!isGrounded)
            AnimateAir();
        else if (IsMoving())
            AnimateWalk();
        else
            ShowIdle();

        wasGrounded = isGrounded;
    }

    // ---------- State detection ----------

    void CheckGround()
    {
        Vector2 point = groundCheck != null
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + Vector2.down * feetOffset;

        // Ignore the player's own collider(s)
        var hits = Physics2D.OverlapCircleAll(point, groundCheckRadius, groundLayer);
        isGrounded = false;
        foreach (var h in hits)
        {
            if (h.attachedRigidbody != rb) { isGrounded = true; break; }
        }

        if (isGrounded)
        {
            airJumpsUsed = 0;
            playingDoubleJump = false;
        }
    }

    void DetectJumps()
    {
        if (!Input.GetKeyDown(jumpKey)) return;

        // Pressed jump while airborne and we haven't used the air jump yet -> double jump.
        // (If your controller blocks the double jump in some cases, call TriggerDoubleJump()
        // from it instead and untick this detection.)
        if (!isGrounded && airJumpsUsed == 0 && !wasGrounded)
        {
            TriggerDoubleJump();
        }
    }

    /// <summary>Call this from your controller if you'd rather trigger the double-jump animation manually.</summary>
    public void TriggerDoubleJump()
    {
        airJumpsUsed = 1;
        playingDoubleJump = true;
        doubleJumpFrame = 0;
        timer = 0f;
        sr.sprite = jumpSprites[0];
    }

    bool IsMoving()
    {
        return Input.GetKey(leftKey) != Input.GetKey(rightKey); // exactly one held
    }

    bool IsSprinting()
    {
        return Input.GetKey(sprintKey);
    }

    // Facing is NOT handled here: PlayerMovement.Turn() rotates the whole transform 180° on Y,
    // which already mirrors the sprite. Flipping flipX as well would cancel it out.

    // ---------- Animation ----------

    void ShowIdle()
    {
        walkFrame = 0;
        timer = 0f;
        sr.sprite = idleSprite;
    }

    void AnimateWalk()
    {
        if (walkSprites == null || walkSprites.Length == 0) return;

        float frameTime = IsSprinting() ? sprintFrameTime : walkFrameTime;
        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer -= frameTime;
            walkFrame = (walkFrame + 1) % walkSprites.Length;
        }
        sr.sprite = walkSprites[walkFrame];
    }

    void AnimateAir()
    {
        if (jumpSprites == null || jumpSprites.Length < 3) return;

        // Double jump: quick run through the 3 jump frames, then fall back to velocity-based pose
        if (playingDoubleJump)
        {
            timer += Time.deltaTime;
            if (timer >= doubleJumpFrameTime)
            {
                timer -= doubleJumpFrameTime;
                doubleJumpFrame++;
                if (doubleJumpFrame >= jumpSprites.Length)
                {
                    playingDoubleJump = false;
                }
            }
            if (playingDoubleJump)
            {
                sr.sprite = jumpSprites[doubleJumpFrame];
                return;
            }
        }

        float vy = rb.velocity.y; // use rb.linearVelocity.y on Unity 6+
        if (vy > apexThreshold) sr.sprite = jumpSprites[0];        // rising
        else if (vy < -apexThreshold) sr.sprite = jumpSprites[2];  // falling
        else sr.sprite = jumpSprites[1];                           // apex
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 p = groundCheck != null
            ? groundCheck.position
            : transform.position + Vector3.down * feetOffset;
        Gizmos.DrawWireSphere(p, groundCheckRadius);
    }
}