using System.Collections;
using UnityEngine;
using UnityEngine.Serialization; // CHANGED: nodig voor FormerlySerializedAs, gebruikt hieronder om een public field veilig te hernoemen

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private ParticleSystem trailFX;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float jumpTime = 0.1f;

    // CHANGED: hernoemd van "SchuinSpringen" naar "diagonalJump" voor consistente Engelse naamgeving.
    // FormerlySerializedAs zorgt dat een reeds ingestelde waarde in de Inspector niet verloren gaat door de rename.
    [FormerlySerializedAs("SchuinSpringen")]
    public bool diagonalJump = true; // Toggle this in the Inspector

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

    // CHANGED: toegevoegd voor de nieuwe pass-through check op basis van de Input System,
    // gebruikt om het moment te detecteren waarop "omlaag" ingedrukt wordt (edge-detect, i.p.v. elke frame dat het ingedrukt blijft).
    private bool wasDownPressed;

    // CHANGED: herbruikbare buffer voor Physics2D.OverlapBoxNonAlloc, zodat FindPlatformEffector()
    // niet langer bij elke aanroep een nieuwe array alloceert (was OverlapBoxAll).
    private readonly Collider2D[] overlapBuffer = new Collider2D[8];

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();

        // CHANGED: defensieve null-checks toegevoegd met duidelijke warnings i.p.v. later
        // een onverklaarbare NullReferenceException te krijgen.
        if (rb == null) Debug.LogWarning($"{name}: Rigidbody2D ontbreekt op Player.", this);
        if (anim == null) Debug.LogWarning($"{name}: Animator ontbreekt op Player.", this);
        if (coll == null) Debug.LogWarning($"{name}: Collider2D ontbreekt op Player.", this);
        if (trailFX == null) Debug.LogWarning($"{name}: trailFX niet toegewezen op Player.", this);

        StartDirectionCheck();
    }

    private void Update()
    {
        // CHANGED: IsGrounded() werd tot 4x per frame aangeroepen (Move, Jump x2, DrawGroundCheck).
        // De physics-query draait nu één keer per frame; het resultaat wordt doorgegeven aan wie het nodig heeft.
        bool grounded = IsGrounded();

        Move(grounded);
        Jump(grounded);
        CheckPassThrough();
        DrawGroundCheck(grounded);
    }

    #region Movement
    // CHANGED: neemt nu "grounded" als parameter i.p.v. zelf opnieuw IsGrounded() aan te roepen.
    private void Move(bool grounded)
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
        if (grounded)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // If the character is in the air, maintain the current horizontal velocity
            rb.linearVelocity = new Vector2(moveInput * (moveSpeed / 2), rb.linearVelocity.y);
        }

        // Always check and update the dust effect
        Dust(grounded); // CHANGED: grounded doorgegeven i.p.v. opnieuw physics query
    }

    // CHANGED: neemt nu "grounded" als parameter i.p.v. zelf opnieuw IsGrounded() aan te roepen.
    private void Dust(bool grounded)
    {
        if (trailFX == null) return; // CHANGED: guard tegen ontbrekende referentie

        if (grounded && moveInput != 0)
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

    // CHANGED: neemt nu "grounded" als parameter i.p.v. zelf opnieuw IsGrounded() aan te roepen.
    private void Jump(bool grounded)
    {
        // Button was pressed this frame and character is grounded
        if (UserInput.instance.controls.Jumping.Jump.WasPressedThisFrame() && grounded)
        {
            IsJumping = true;
            JumpTimeCounter = jumpTime;
            if (diagonalJump) // CHANGED: hernoemde field
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // Maintain horizontal velocity
            }
            else
            {
                rb.linearVelocity = new Vector2(0, jumpForce); // Set horizontal velocity to 0 when jumping
            }

            anim.SetTrigger("jump");
            if (trailFX != null) trailFX.Stop(); // CHANGED: guard tegen ontbrekende referentie
        }

        // Button is held
        if (UserInput.instance.controls.Jumping.Jump.IsPressed())
        {
            if (JumpTimeCounter > 0 && IsJumping)
            {
                if (diagonalJump) // CHANGED: hernoemde field
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
        if (!IsJumping && CheckForLand(grounded)) // CHANGED: grounded doorgegeven
        {
            anim.SetTrigger("land");

            // CHANGED: eventuele vorige, nog lopende reset-coroutine stoppen voordat een nieuwe start,
            // zodat snelle landingen niet meerdere coroutines tegelijk dezelfde trigger laten resetten.
            if (resetTriggerCoroutine != null) StopCoroutine(resetTriggerCoroutine);
            resetTriggerCoroutine = StartCoroutine(Reset());
        }
    }
    #endregion

    #region Ground/Landed Check
    private bool IsGrounded()
    {
        if (coll == null) return false; // CHANGED: guard tegen ontbrekende collider

        groundHit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, extraHeight, whatIsGround);

        return groundHit.collider != null;
    }

    // CHANGED: neemt nu "grounded" als parameter i.p.v. zelf opnieuw IsGrounded() aan te roepen.
    private bool CheckForLand(bool grounded)
    {
        if (IsFalling && grounded)
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
    // CHANGED: legacy Input.GetKeyDown(KeyCode.S / DownArrow) vervangen door de al bestaande
    // "Move"-action uit de nieuwe Input System (moveInput.y), zodat het hele project consistent
    // de nieuwe Input System gebruikt i.p.v. legacy en nieuw door elkaar.
    private void CheckPassThrough()
    {
        bool isDownPressed = UserInput.instance.moveInput.y < -0.5f;

        if (isDownPressed && !wasDownPressed) // edge-detect, zelfde gedrag als het oude GetKeyDown
        {
            StartCoroutine(PassThrough());
        }

        wasDownPressed = isDownPressed;
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

    // CHANGED: gebruikt Physics2D.OverlapBoxNonAlloc met een herbruikbare buffer i.p.v.
    // OverlapBoxAll, dat bij elke aanroep een nieuwe array alloceerde.
    private PlatformEffector2D FindPlatformEffector()
    {
        int count = Physics2D.OverlapBoxNonAlloc(transform.position, coll.bounds.size, 0, overlapBuffer);
        for (int i = 0; i < count; i++)
        {
            PlatformEffector2D effector = overlapBuffer[i].GetComponent<PlatformEffector2D>();
            if (effector != null)
            {
                return effector;
            }
        }
        return null;
    }
    #endregion

    #region Debug Functions
    // CHANGED: neemt nu "grounded" als parameter i.p.v. zelf opnieuw IsGrounded() aan te roepen
    // (dit was de 4e overbodige aanroep per frame).
    private void DrawGroundCheck(bool grounded)
    {
        if (coll == null) return; // CHANGED: guard tegen ontbrekende collider

        Color rayColor = grounded ? Color.green : Color.red;

        Debug.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + extraHeight), rayColor);
        Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, coll.bounds.extents.y + extraHeight), Vector2.right * (coll.bounds.extents.x * 2), rayColor);
    }
    #endregion
}
