using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private ParticleSystem trailFX;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float jumpTime = 0.1f;
    public bool SchuinSpringen = true; // Toggle this in the Inspector

    [Header("Turn Check")]
    [SerializeField] private GameObject DirL;
    [SerializeField] private GameObject DirR;

    [Header("Ground Check")]
    [SerializeField] private float extraHeight = 0.25f;
    [SerializeField] private LayerMask whatIsGround;

    [HideInInspector] public bool IsFacingRight;
    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator anim;
    private float moveInput;

    private bool IsJumping;
    private bool IsFalling;
    private float JumpTimeCounter;
    private RaycastHit2D groundHit;
    private Coroutine resetTriggerCoroutine;

    // Variables for pass-through platform
    private PlatformEffector2D currentPlatformEffector;

    // CHANGED: pre-allocated buffer + filter for non-alloc overlap queries, avoids
    // per-call GC allocation from the old Physics2D.OverlapBoxAll usage
    private readonly Collider2D[] overlapResults = new Collider2D[8];
    private ContactFilter2D overlapFilter;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        StartDirectionCheck();

        // CHANGED: configure the reusable ContactFilter2D once instead of building
        // query parameters on every call
        overlapFilter = new ContactFilter2D();
        overlapFilter.NoFilter();
        overlapFilter.useTriggers = true;
    }

    private void Update()
    {
        // CHANGED: IsGrounded() now runs once per frame and the result is passed
        // into Move()/Jump()/DrawGroundCheck() instead of each method calling
        // Physics2D.BoxCast redundantly (per code-conventions.md)
        bool isGrounded = IsGrounded();

        Move(isGrounded);
        Jump(isGrounded);
        CheckPassThrough();
        DrawGroundCheck(isGrounded);
    }

    #region Movement
    private void Move(bool isGrounded)
    {
        moveInput = UserInput.instance.moveInput.x;

        // Check if the character is moving left or right
        if (moveInput != 0)
        {
            anim.SetBool("IsWalking", true);
            TurnCheck();
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }

        // Update horizontal velocity only if the character is grounded
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // If the character is in the air, maintain the current horizontal velocity
            rb.linearVelocity = new Vector2(moveInput * (moveSpeed / 2), rb.linearVelocity.y);
        }

        // Always check and update the dust effect
        Dust(isGrounded);
    }

    private void Dust(bool isGrounded)
    {
        if (isGrounded && moveInput != 0)
        {
            if (!trailFX.isPlaying)
            {
                trailFX.Play();
            }
        }
        else
        {
            if (trailFX.isPlaying)
            {
                trailFX.Stop();
            }
        }
    }

    private void Jump(bool isGrounded)
    {
        // Button was pressed this frame and character is grounded
        if (UserInput.instance.controls.Jumping.Jump.WasPressedThisFrame() && isGrounded)
        {
            IsJumping = true;
            JumpTimeCounter = jumpTime;
            if (SchuinSpringen)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // Maintain horizontal velocity
            }
            else
            {
                rb.linearVelocity = new Vector2(0, jumpForce); // Set horizontal velocity to 0 when jumping
            }

            anim.SetTrigger("jump");
            trailFX.Stop();
        }

        // Button is held
        if (UserInput.instance.controls.Jumping.Jump.IsPressed())
        {
            if (JumpTimeCounter > 0 && IsJumping)
            {
                if (SchuinSpringen)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // Maintain horizontal velocity
                }
                else
                {
                    rb.linearVelocity = new Vector2(0, jumpForce); // Maintain horizontal velocity as 0
                }
                JumpTimeCounter -= Time.deltaTime;
            }
            else if (JumpTimeCounter <= 0)
            {
                IsFalling = true;
                IsJumping = false;
            }
            else
            {
                IsJumping = false;
                anim.ResetTrigger("jump");
            }
        }

        // Button was released this frame
        if (UserInput.instance.controls.Jumping.Jump.WasReleasedThisFrame())
        {
            IsJumping = false;
            IsFalling = true;
        }

        // Check for landing
        if (!IsJumping && CheckForLand(isGrounded))
        {
            anim.SetTrigger("land");
            resetTriggerCoroutine = StartCoroutine(Reset());
        }
    }
    #endregion

    #region Ground/Landed Check
    private bool IsGrounded()
    {
        groundHit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, extraHeight, whatIsGround);

        return groundHit.collider != null;
    }

    private bool CheckForLand(bool isGrounded)
    {
        if (IsFalling && isGrounded)
        {
            IsFalling = false;
            return true;
        }
        return false;
    }

    private IEnumerator Reset()
    {
        yield return null;
        anim.ResetTrigger("land");
    }
    #endregion

    #region Turn Checks
    private void StartDirectionCheck()
    {
        IsFacingRight = DirR.transform.position.x > DirL.transform.position.x;
    }

    private void TurnCheck()
    {
        if (UserInput.instance.moveInput.x > 0 && !IsFacingRight)
        {
            Turn();
        }
        else if (UserInput.instance.moveInput.x < 0 && IsFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        IsFacingRight = !IsFacingRight;
        float rotationY = IsFacingRight ? 0f : 180f;
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }
    #endregion

    #region Pass-Through Platform
    private void CheckPassThrough()
    {
        // CHANGED: legacy UnityEngine.Input.GetKeyDown throws an InvalidOperationException
        // when Active Input Handling is set to "Input System Package (New)". Replaced with
        // Keyboard.current from the new Input System.
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            StartCoroutine(PassThrough());
        }
    }

    private IEnumerator PassThrough()
    {
        currentPlatformEffector = FindPlatformEffector();
        if (currentPlatformEffector != null)
        {
            float originalSurfaceArc = currentPlatformEffector.surfaceArc;
            currentPlatformEffector.surfaceArc = 0f; // Allow pass-through
            yield return new WaitForSeconds(0.2f); // Wait a bit to ensure player has moved down
            currentPlatformEffector.surfaceArc = originalSurfaceArc; // Restore original surfaceArc
        }
    }

    private PlatformEffector2D FindPlatformEffector()
    {
        // CHANGED: replaced deprecated Physics2D.OverlapBoxNonAlloc with the current
        // Physics2D.OverlapBox(point, size, angle, ContactFilter2D, results) overload.
        // Uses the pre-allocated overlapResults buffer, so this still does not allocate
        // per call (unlike OverlapBoxAll, which allocates a new array every frame it runs).
        int hitCount = Physics2D.OverlapBox(transform.position, coll.bounds.size, 0f, overlapFilter, overlapResults);

        for (int i = 0; i < hitCount; i++)
        {
            PlatformEffector2D effector = overlapResults[i].GetComponent<PlatformEffector2D>();
            if (effector != null)
            {
                return effector;
            }
        }
        return null;
    }
    #endregion

    #region Debug Functions
    private void DrawGroundCheck(bool isGrounded)
    {
        Color rayColor = isGrounded ? Color.green : Color.red;

        Debug.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, coll.bounds.extents.y + extraHeight), Vector2.right * (coll.bounds.extents.x * 2), rayColor);
    }
    #endregion
}
