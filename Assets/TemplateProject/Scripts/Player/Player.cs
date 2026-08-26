using System.Collections;
using UnityEngine;

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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        StartDirectionCheck();
    }

    private void Update()
    {
        Move();
        Jump();
        CheckPassThrough();
    }

    #region Movement
    private void Move()
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
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // If the character is in the air, maintain the current horizontal velocity
            rb.linearVelocity = new Vector2(moveInput * (moveSpeed / 2), rb.linearVelocity.y);
        }

        // Always check and update the dust effect
        Dust();
    }

    private void Dust()
    {
        if (IsGrounded() && moveInput != 0)
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

    private void Jump()
    {
        // Button was pressed this frame and character is grounded
        if (UserInput.instance.controls.Jumping.Jump.WasPressedThisFrame() && IsGrounded())
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
        if (!IsJumping && CheckForLand())
        {
            anim.SetTrigger("land");
            resetTriggerCoroutine = StartCoroutine(Reset());
        }

        // Draw ground check (assuming it's for debugging purposes)
        DrawGroundCheck();
    }
    #endregion

    #region Ground/Landed Check
    private bool IsGrounded()
    {
        groundHit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, extraHeight, whatIsGround);

        return groundHit.collider != null;
    }

    private bool CheckForLand()
    {
        if (IsFalling && IsGrounded())
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
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
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
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, coll.bounds.size, 0);
        foreach (Collider2D collider in colliders)
        {
            PlatformEffector2D effector = collider.GetComponent<PlatformEffector2D>();
            if (effector != null)
            {
                return effector;
            }
        }
        return null;
    }
    #endregion

    #region Debug Functions
    private void DrawGroundCheck()
    {
        Color rayColor = IsGrounded() ? Color.green : Color.red;

        Debug.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, coll.bounds.extents.y + extraHeight), Vector2.right * (coll.bounds.extents.x * 2), rayColor);
    }
    #endregion
}
