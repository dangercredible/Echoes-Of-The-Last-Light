using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 18f;

    [Header("Double Jump")]
    public int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Air Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.8f;
    private bool isDashing;
    private bool canDash = true;
    private float dashTimer;
    private float dashCooldownTimer;

    [Header("Slide")]
    public float slideSpeed = 12f;
    public float slideDuration = 0.4f;
    public float slideCooldown = 0.6f;
    public float slideColliderHeightMultiplier = 0.5f;
    private bool isSliding;
    private float slideTimer;
    private float slideCooldownTimer;
    private CapsuleCollider2D capsuleCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    [Header("Glide")]
    public float glideGravity = 0.35f;
    public float glideMaxFallSpeed = -2.5f;
    public float maxGlideStamina = 5f;
    public float glideStaminaRecoveryRate = 1.5f;
    private bool isGlideHeld;
    private bool isGliding;
    private float currentGlideStamina;

    [Header("Wall Jump")]
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.1f;
    public float wallSlideFallSpeed = -2.5f;
    public float wallJumpHorizontalForce = 11f;
    public float wallJumpVerticalForce = 9f;
    public float wallJumpMoveLockTime = 0.15f;
    public float wallCoyoteTime = 0.12f;
    public float wallStickTime = 0.15f;
    private bool isTouchingWall;
    private bool isWallSliding;
    private int wallSide;
    private float wallJumpMoveLockTimer;
    private float wallCoyoteCounter;
    private float wallStickCounter;
    private int lastWallSide;

    [Header("Grapple")]
    public float grappleRange = 10f;
    public float grappleMinRopeLength = 1.5f;
    public float grappleMaxRopeLength = 12f;
    public float grappleReelSpeed = 8f;
    public float grappleSwingAirControl = 20f;
    public float grappleCooldown = 0.25f;
    public LayerMask grappleBlockerLayer;
    public LineRenderer grappleLine;
    public int grappleLineSegments = 18;
    public float ropeSag = 0.75f;
    private bool isGrappling;
    private bool grappleHeld;
    private float grappleCooldownTimer;
    private LightGrapplePoint grappleTarget;

    [Header("Lantern")]
    public LightLantern lantern;

    [Header("Animation")]
    public Animator animator;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.1f;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private float baseGravityScale;
    private bool facingRight = true;
    private Vector2 moveInput;
    private ContactFilter2D wallFilter;
    private readonly RaycastHit2D[] wallHits = new RaycastHit2D[2];
    private DistanceJoint2D grappleJoint;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        baseGravityScale = rb.gravityScale;
        lantern = lantern != null ? lantern : GetComponentInChildren<LightLantern>();
        animator = animator != null ? animator : GetComponentInChildren<Animator>();
        grappleJoint = GetComponent<DistanceJoint2D>();
        if (grappleJoint == null)
            grappleJoint = gameObject.AddComponent<DistanceJoint2D>();

        grappleJoint.autoConfigureConnectedAnchor = false;
        grappleJoint.autoConfigureDistance = false;
        grappleJoint.maxDistanceOnly = true;
        grappleJoint.enableCollision = false;
        grappleJoint.enabled = false;

        if (capsuleCollider != null)
        {
            originalColliderSize = capsuleCollider.size;
            originalColliderOffset = capsuleCollider.offset;
        }

        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(wallLayer);
        wallFilter.useTriggers = false;

        currentGlideStamina = maxGlideStamina;
    }

    void Update()
    {
        ApplyKeyboardFallbackInput();
        UpdateGroundedState();
        UpdateWallState();
        UpdateTimers();
        HandleJumpAttempt();
        UpdateGrappleState();
        UpdateGlideState();
        FlipSprite();
        UpdateAnimatorState();
    }

    void FixedUpdate()
    {
        if (isDashing)
            return;

        if (isGrappling)
        {
            if (grappleJoint != null && grappleJoint.enabled)
            {
                float nextDistance = grappleJoint.distance - (moveInput.y * grappleReelSpeed * Time.fixedDeltaTime);
                grappleJoint.distance = Mathf.Clamp(nextDistance, grappleMinRopeLength, grappleMaxRopeLength);
            }

            if (Mathf.Abs(moveInput.x) > 0.05f)
                rb.AddForce(new Vector2(moveInput.x * grappleSwingAirControl, 0f), ForceMode2D.Force);

            return;
        }

        if (isSliding)
        {
            rb.linearVelocity = new Vector2((facingRight ? 1f : -1f) * slideSpeed, rb.linearVelocity.y);
            return;
        }

        float horizontal = wallJumpMoveLockTimer > 0f ? 0f : moveInput.x * moveSpeed;
        float vertical = rb.linearVelocity.y;
        if (isWallSliding)
        {
            bool stickActive = wallStickCounter > 0f && (isTouchingWall || wallCoyoteCounter > 0f);
            if (stickActive && vertical < 0f)
                vertical = Mathf.Max(vertical, wallSlideFallSpeed * 0.35f);
            else if (vertical < wallSlideFallSpeed)
                vertical = wallSlideFallSpeed;
        }

        rb.linearVelocity = new Vector2(horizontal, vertical);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
            isGlideHeld = true;
        }
        else
        {
            isGlideHeld = false;
            if (rb.linearVelocity.y > 0f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
            TryStartDash();
    }

    public void OnSlide(InputValue value)
    {
        if (value.isPressed)
            TryStartSlide();
    }

    public void OnGlide(InputValue value)
    {
        isGlideHeld = value.isPressed;
    }

    public void OnGrapple(InputValue value)
    {
        grappleHeld = value.isPressed;
        if (value.isPressed)
            TryStartGrapple();
        else
            StopGrapple();
    }

    public void OnLantern(InputValue value)
    {
        if (value.isPressed)
            ToggleLantern();
    }

    void ApplyKeyboardFallbackInput()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontal += 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            vertical += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            vertical -= 1f;
        moveInput = new Vector2(horizontal, vertical);

        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            jumpBufferCounter = jumpBufferTime;

        if ((Keyboard.current.spaceKey.wasReleasedThisFrame || Keyboard.current.wKey.wasReleasedThisFrame || Keyboard.current.upArrowKey.wasReleasedThisFrame) && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame)
            TryStartDash();

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.cKey.wasPressedThisFrame)
            TryStartSlide();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            grappleHeld = true;
            TryStartGrapple();
        }
        if (Keyboard.current.eKey.wasReleasedThisFrame)
        {
            grappleHeld = false;
            StopGrapple();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
            ToggleLantern();

        isGlideHeld = Keyboard.current.spaceKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
    }

    void UpdateGroundedState()
    {
        isGrounded = groundCheck != null && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            jumpsRemaining = maxJumps;
            canDash = true;
            isGliding = false;
            currentGlideStamina = Mathf.Min(maxGlideStamina, currentGlideStamina + glideStaminaRecoveryRate * Time.deltaTime);
            StopGrapple();
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            if (!isGliding)
                currentGlideStamina = Mathf.Min(maxGlideStamina, currentGlideStamina + (glideStaminaRecoveryRate * 0.35f) * Time.deltaTime);
        }
    }

    void UpdateWallState()
    {
        isWallSliding = false;
        isTouchingWall = false;
        wallSide = 0;

        if (isGrounded || capsuleCollider == null)
            return;

        Bounds bounds = capsuleCollider.bounds;
        Vector2 center = bounds.center;
        float castDistance = wallCheckDistance + 0.02f;

        int hitLeftCount = Physics2D.Raycast(center, Vector2.left, wallFilter, wallHits, castDistance);
        if (hitLeftCount > 0)
        {
            isTouchingWall = true;
            wallSide = -1;
            lastWallSide = wallSide;
        }

        int hitRightCount = Physics2D.Raycast(center, Vector2.right, wallFilter, wallHits, castDistance);
        if (hitRightCount > 0)
        {
            isTouchingWall = true;
            wallSide = 1;
            lastWallSide = wallSide;
        }

        if (isTouchingWall)
        {
            wallCoyoteCounter = wallCoyoteTime;
            if (moveInput.x * wallSide > 0.1f)
                wallStickCounter = wallStickTime;
        }
        else
        {
            wallCoyoteCounter -= Time.deltaTime;
            wallStickCounter -= Time.deltaTime;
        }

        bool hasWallGrace = wallCoyoteCounter > 0f && lastWallSide != 0;
        if ((isTouchingWall || hasWallGrace) && rb.linearVelocity.y < 0f && !isGrounded)
            isWallSliding = true;
    }

    void UpdateTimers()
    {
        jumpBufferCounter -= Time.deltaTime;
        grappleCooldownTimer -= Time.deltaTime;
        wallJumpMoveLockTimer -= Time.deltaTime;
        wallCoyoteCounter -= Time.deltaTime;
        wallStickCounter -= Time.deltaTime;

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                StopDash();
        }

        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
                canDash = true;
        }

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
                StopSlide();
        }

        if (slideCooldownTimer > 0f)
            slideCooldownTimer -= Time.deltaTime;
    }

    void HandleJumpAttempt()
    {
        if (isDashing || isSliding || isGrappling)
            return;

        if (jumpBufferCounter <= 0f)
            return;

        if (isWallSliding && !isGrounded && (isTouchingWall || wallCoyoteCounter > 0f))
        {
            WallJump();
            jumpBufferCounter = 0f;
            return;
        }

        if (coyoteTimeCounter > 0f || jumpsRemaining > 0)
        {
            Jump();
            jumpBufferCounter = 0f;
        }
    }

    void UpdateGrappleState()
    {
        if (!isGrappling || grappleTarget == null)
        {
            UpdateGrappleLine(false);
            return;
        }

        if (!grappleHeld || !grappleTarget.IsIlluminated)
        {
            StopGrapple();
            return;
        }

        UpdateGrappleLine(true);
    }

    void UpdateGlideState()
    {
        isGliding = !isGrounded && !isDashing && !isSliding && !isGrappling && isGlideHeld && rb.linearVelocity.y < 0f && currentGlideStamina > 0f;

        if (isGliding)
        {
            currentGlideStamina = Mathf.Max(0f, currentGlideStamina - Time.deltaTime);
            rb.gravityScale = glideGravity;
            if (rb.linearVelocity.y < glideMaxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, glideMaxFallSpeed);
        }
        else if (!isDashing)
        {
            rb.gravityScale = baseGravityScale;
        }
    }

    void Jump()
    {
        isGliding = false;
        StopGrapple();
        rb.gravityScale = baseGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsRemaining--;
        coyoteTimeCounter = 0f;
    }

    void WallJump()
    {
        isWallSliding = false;
        StopGrapple();
        rb.gravityScale = baseGravityScale;
        int jumpWallSide = wallSide != 0 ? wallSide : (lastWallSide != 0 ? lastWallSide : (facingRight ? 1 : -1));
        float wallJumpX = -jumpWallSide * wallJumpHorizontalForce;
        float wallJumpY = Mathf.Min(wallJumpVerticalForce, jumpForce * 0.75f);
        rb.linearVelocity = new Vector2(wallJumpX, wallJumpY);
        wallJumpMoveLockTimer = wallJumpMoveLockTime;
        jumpsRemaining = Mathf.Max(jumpsRemaining, maxJumps - 1);
        facingRight = rb.linearVelocity.x > 0f;
        wallCoyoteCounter = 0f;
        wallStickCounter = 0f;
    }

    void TryStartDash()
    {
        if (!canDash || isDashing || isGrounded || isGrappling)
            return;

        isDashing = true;
        canDash = false;
        isGliding = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        float dashX = Mathf.Abs(moveInput.x) > 0.1f ? Mathf.Sign(moveInput.x) : (facingRight ? 1f : -1f);
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashX * dashSpeed, 0f);
    }

    void StopDash()
    {
        isDashing = false;
        rb.gravityScale = baseGravityScale;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.35f, rb.linearVelocity.y);
    }

    void TryStartSlide()
    {
        if (!isGrounded || isSliding || isDashing || isGrappling || slideCooldownTimer > 0f)
            return;

        isSliding = true;
        slideTimer = slideDuration;
        slideCooldownTimer = slideCooldown;

        if (capsuleCollider != null)
        {
            capsuleCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y * slideColliderHeightMultiplier);
            float heightDelta = originalColliderSize.y - capsuleCollider.size.y;
            capsuleCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - heightDelta * 0.5f);
        }
    }

    void StopSlide()
    {
        isSliding = false;

        if (capsuleCollider != null)
        {
            capsuleCollider.size = originalColliderSize;
            capsuleCollider.offset = originalColliderOffset;
        }
    }

    void TryStartGrapple()
    {
        if (grappleCooldownTimer > 0f || isGrounded || isDashing || isSliding)
            return;

        LightGrapplePoint nearest = null;
        float bestDistance = float.MaxValue;
        Vector2 origin = transform.position;

        foreach (LightGrapplePoint point in LightGrapplePoint.ActivePoints)
        {
            if (point == null || !point.IsIlluminated)
                continue;

            float distance = Vector2.Distance(origin, point.transform.position);
            if (distance > grappleRange || distance >= bestDistance)
                continue;

            if (grappleBlockerLayer.value != 0)
            {
                RaycastHit2D hit = Physics2D.Linecast(origin, point.transform.position, grappleBlockerLayer);
                if (hit.collider != null)
                    continue;
            }

            nearest = point;
            bestDistance = distance;
        }

        if (nearest == null)
            return;

        grappleTarget = nearest;
        isGrappling = true;
        isGliding = false;
        rb.gravityScale = baseGravityScale;

        if (grappleJoint != null)
        {
            grappleJoint.connectedAnchor = grappleTarget.transform.position;
            float ropeLength = Vector2.Distance(rb.position, grappleTarget.transform.position);
            grappleJoint.distance = Mathf.Clamp(ropeLength, grappleMinRopeLength, grappleMaxRopeLength);
            grappleJoint.enabled = true;
        }
    }

    void StopGrapple()
    {
        if (!isGrappling)
            return;

        isGrappling = false;
        grappleTarget = null;
        rb.gravityScale = baseGravityScale;
        grappleCooldownTimer = grappleCooldown;
        if (grappleJoint != null)
            grappleJoint.enabled = false;
        UpdateGrappleLine(false);
    }

    void UpdateGrappleLine(bool show)
    {
        if (grappleLine == null)
            return;

        grappleLine.enabled = show;
        if (!show || grappleTarget == null)
            return;

        Vector3 start = transform.position;
        Vector3 end = grappleTarget.transform.position;
        int segments = Mathf.Max(2, grappleLineSegments);
        grappleLine.positionCount = segments;

        Vector3 midpoint = (start + end) * 0.5f;
        float dist = Vector3.Distance(start, end);
        midpoint.y -= ropeSag * Mathf.Clamp(dist / 5f, 0.4f, 1.6f);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 a = Vector3.Lerp(start, midpoint, t);
            Vector3 b = Vector3.Lerp(midpoint, end, t);
            Vector3 point = Vector3.Lerp(a, b, t);
            grappleLine.SetPosition(i, point);
        }
    }

    void ToggleLantern()
    {
        if (lantern != null)
            lantern.ToggleLantern();
    }

    void UpdateAnimatorState()
    {
        if (animator == null)
            return;

        animator.SetFloat("moveX", Mathf.Abs(moveInput.x));
        animator.SetFloat("velocityY", rb.linearVelocity.y);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isSliding", isSliding);
        animator.SetBool("isDashing", isDashing);
        animator.SetBool("isGliding", isGliding);
        animator.SetBool("isWallSliding", isWallSliding);
        animator.SetBool("isGrappling", isGrappling);
    }

    void FlipSprite()
    {
        if (moveInput.x > 0f && !facingRight)
        {
            facingRight = true;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
        else if (moveInput.x < 0f && facingRight)
        {
            facingRight = false;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
    }

    public void Die()
    {
        StopSlide();
        isDashing = false;
        isGliding = false;
        rb.gravityScale = baseGravityScale;
    }

    public bool IsGrounded() => isGrounded;
    public bool IsDashing() => isDashing;
    public bool IsSliding() => isSliding;
    public bool IsGliding() => isGliding;
    public bool IsWallSliding() => isWallSliding;
    public bool IsGrappling() => isGrappling;
    public float GetGlideStamina() => currentGlideStamina;
    public float GetGlideStaminaNormalized() => maxGlideStamina <= 0f ? 0f : Mathf.Clamp01(currentGlideStamina / maxGlideStamina);
}