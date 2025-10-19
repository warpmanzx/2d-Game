using System.Collections;
using UnityEngine;

public class Player : Entity
{
    //Nytt Input System
    private PlayerInputActions inputActions;
    private Vector2 moveInput;

    [SerializeField] private GameObject slashVFX;
    Quaternion slashRotation;

    private float xInput;

    [Header("Gravity")]
    [SerializeField] protected float normalGravity = 5f;
    [SerializeField] protected float increasedGravity = 10f;
    [SerializeField] protected float terminalVelocity = 10f;
    [Header("Dashing")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.1f;
    [SerializeField] private float dashCooldown = 0.5f;
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private int dashesRemaining;
    private int maxDashes = 1;
    TrailRenderer trailRenderer;
    [Header("WallCheck")]
    [SerializeField] private Transform wallCheckPos;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.49f, 0.3f);
    [SerializeField] private LayerMask whatIsWall;
    [Header("WallMovement")]
    [SerializeField] private float wallJumpDuration = 0.1f;
    [SerializeField] private float wallSlideSpeed = 2f;
    private bool isWallSliding;
    private bool isWallJumping;

    //WallJumping
    [SerializeField] private float xVelWalljump = 10f;
    [SerializeField] private float yVelWalljump = 20f;

    [Header("InvincibilityFrames")]
    [SerializeField] private float invulnerabilityDuration = 1.0f;
    [SerializeField] private bool invulnerable = false;
    [SerializeField] private int numberOfFlashes;

    private bool blockNormalAttackThisFrame = false;
    private bool blockUpAttackThisFrame = false;
    private bool blockDownAttackThisFrame = false;
    [Header("AttackCooldown")]
    [SerializeField] private float attackCooldown = 1.0f;
    private bool AttackOnCooldown;
    [Header("Health")]
    public HealthUI healthUI;

    protected override void Awake()
    {
        //Nytt Input System
        inputActions = new PlayerInputActions();

        base.Awake();
        trailRenderer = GetComponent<TrailRenderer>();
        dashesRemaining = maxDashes;
        jumpsRemaining = maxJumps;
        healthUI.SetMaxHearts(maxHealth);
    }

    protected override void Update()
    {
        if (isDashing)
            return;
        base.Update();
        HandleInput();
        HandleGravity();
        HandleWallSlide();

        blockUpAttackThisFrame = false;
        blockDownAttackThisFrame = false;
        blockNormalAttackThisFrame = false;
    }

    public void SpawnSlash()
    {
        // Instantierar en slash vfx åt hållet man kollar
        Quaternion slashRotation = facingRight ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
        GameObject slashInstance = Instantiate(slashVFX, new Vector3(attackPoint.position.x, attackPoint.position.y - 0.2f), slashRotation);
        Destroy(slashInstance, 0.1f);
        //if (attackingStraight)
        //{
        //    slashVFX = Instantiate(slashVFX, new Vector3(attackPoint.position.x, attackPoint.position.y -0.2f), slashRotation);
        //}
        //else if (attackingDown)
        //{
        //    slashVFX = Instantiate(slashVFX, new Vector3(), slashRotation); // WORK IN PROGRESS
        //}
        //else if (attackingUp)
        //{

        //    slashVFX = Instantiate(slashVFX, new Vector3(), slashRotation); // WORK IN PROGRESS
        //}
    }

    // Denna kallas från animation event
    protected override void HandleFlip()
    {
        base.HandleFlip();
    }

    private void HandleInput()
    {
        // Nytt Input System
        inputActions.Enable();

        inputActions.Player.Jump.performed += ctx => TryToJump();
        inputActions.Player.Attack.performed += ctx => HandleAttack();
        inputActions.Player.AttackUp.performed += ctx => HandleAttackUp();
        inputActions.Player.AttackDown.performed += ctx => HandleAttackDown();
        inputActions.Player.Dash.performed += ctx => HandleDash();

    }


    protected override void HandleAttack()
    {
        if (blockNormalAttackThisFrame)
        {
            // Blockera vanlig attack om vi nyss gjorde upp/ner-attack
            blockNormalAttackThisFrame = false;
            return;
        }
        if (!AttackOnCooldown)
        {
            blockDownAttackThisFrame = true;
            blockUpAttackThisFrame = true;
            base.HandleAttack(); // Eller spawna Slash här
            StartCoroutine(AttackCooldownCo());
        }
    }

    private void HandleAttackUp()
    {
        if (blockUpAttackThisFrame)
        {
            blockUpAttackThisFrame = false;
            return;
        }
        if (!AttackOnCooldown)
        {
            blockDownAttackThisFrame = true;
            blockNormalAttackThisFrame = true;
            anim.SetTrigger("attackUp"); // Eller spawna Slash här
            StartCoroutine(AttackCooldownCo());
        }
    }

    private void HandleAttackDown()
    {
        if (blockDownAttackThisFrame)
        {
            blockDownAttackThisFrame = false;
            return;
        }
        if (!AttackOnCooldown)
        {
            blockUpAttackThisFrame = true;
            blockNormalAttackThisFrame = true;
            anim.SetTrigger("attackDown"); // Eller spawna Slash här
            StartCoroutine(AttackCooldownCo());
        }
    }

    public void HandleGravity()
    {
        // Limits FallSpeed
        if (!isGrounded && rb.linearVelocityY < -terminalVelocity)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, -terminalVelocity);
        }
        if (!isDashing)
        {
            if (rb.linearVelocityY < 0)
                rb.gravityScale = increasedGravity;
            else if (rb.linearVelocityY > 0)
                rb.gravityScale = normalGravity;
            else
                rb.gravityScale = normalGravity;
        }
    }

    private void HandleWallSlide() //  Om man precis nuddar väggen ska all y velocity bort så att man inte kan hoppa och glida upp på väggen
    {
        if (!isWallJumping)
        {
            if (WallCheck() && !isGrounded && rb.linearVelocityX != 0)
            {
                if (!isWallSliding)
                    rb.linearVelocityY = 0;
                isWallSliding = true;
                rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -wallSlideSpeed));
                anim.SetBool("isWallSliding", true);
                jumpsRemaining = maxJumps; // Reset jumps and dashes when wall sliding
                dashesRemaining = maxDashes;
            }
            else
            {
                isWallSliding = false;
                anim.SetBool("isWallSliding", false);
            }
        }
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, whatIsWall);
    }

    protected override void HandleMovement()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        xInput = moveInput.x;
        if (!isWallJumping)
        {
            if (canMove)
                rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocityY);
            else
                rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
        }
    }
    public void EnableFlipAndDash(bool enable)
    {
        canFlip = enable;
        canDash = enable;
    }

    // Man kan bara dubbelhoppa en gång

    override protected void TryToJump()
    {
        if (isGrounded && canJump && !isWallSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
        }
        else if (!isWallSliding && !isGrounded && canJump && jumpsRemaining > 0 && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce * 0.8f);
            jumpsRemaining--;
        }
        else if (isWallSliding && Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(WallJumpCoroutine());
        } // Om man hoppar medans man wallslidear så ska man sparka ut lätt från väggen
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
            dashesRemaining = maxDashes;
        }
    }

    // Medans man är i luften får man bara dasha en gång tills man nuddar marken igen

    private void HandleDash()
    {
        if (!isWallJumping)
        {
            if (canDash && isGrounded)
            {
                StartCoroutine(DashCo());
            }
            else if (canDash && !isGrounded && dashesRemaining > 0)
            {
                StartCoroutine(DashCo());
            }
        }
    }
    public override void EnableMovementAndJumpAndFlip(bool enable)
    {
        base.EnableMovementAndJumpAndFlip(enable);
        canDash = enable;
    }

    private IEnumerator WallJumpCoroutine()
    {
        isWallJumping = true;
        isWallSliding = false;
        anim.SetBool("isWallSliding", false);
        float wallJumpDirection = facingRight ? 1f : -1f;
        wallJumpDirection = wallJumpDirection * -1;
        rb.linearVelocity = new Vector2(wallJumpDirection * xVelWalljump, yVelWalljump);

        yield return new WaitForSeconds(wallJumpDuration);

        rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
        isWallJumping = false;
    }

    public override void TakeDamage(int dmg, bool byCollision)
    {
        if (invulnerable || isDead)
            return;
        currentHealth = currentHealth - dmg;
        healthUI.UpdateHearts(currentHealth);
        if (!isDead)
            StartCoroutine(InvulnerabilityCo());
        if (currentHealth <= 0)
        {
            Die();
            isDead = true;
            if (!isDead && !byCollision)
                SpawnHitEffect();
        }
        else if (currentHealth > 0)
        {
            if (!byCollision)
                SpawnHitEffect();
            Hurt();
        }
    }

    private IEnumerator DashCo()
    {
        dashesRemaining--;
        float xMoveBeforeDash = rb.linearVelocityX;
        rb.linearVelocity = new Vector2(0, 0);
        canDash = false;
        isDashing = true;
        anim.SetBool("isDashing", true);
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;
        trailRenderer.emitting = true;
        float dashDirection = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, rb.linearVelocityY);

        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
        anim.SetBool("isDashing", false);
        isDashing = false;
        trailRenderer.emitting = false;

        yield return new WaitForSeconds(dashCooldown);
        if (!isDead)
            canDash = true;
    }

    private IEnumerator InvulnerabilityCo()
    {
        for (int i = 0; i < numberOfFlashes; i++)
        {
            Physics2D.IgnoreLayerCollision(8, 7, true);
            invulnerable = true;
            sr.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(invulnerabilityDuration / (numberOfFlashes * 2));
            sr.color = Color.white;
            yield return new WaitForSeconds(invulnerabilityDuration / (numberOfFlashes * 2));
            invulnerable = false;
            Physics2D.IgnoreLayerCollision(8, 7, false);
        }
    }

    private IEnumerator AttackCooldownCo()
    {
        AttackOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        AttackOnCooldown = false;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
        Gizmos.DrawWireSphere(attackPointAbove.position, attackRadius);
        Gizmos.DrawWireSphere(attackPointBelow.position, attackRadius);
    }

}


// Gamla systemet
//xInput = Input.GetAxisRaw("Horizontal");

//if (Input.GetKeyDown(KeyCode.Z))
//    TryToJump();
//if (Input.GetKeyDown(KeyCode.X))
//    HandleAttack();
//if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.X)) // WORK IN PROGRESS
//    HandleAttackUp();
//if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.X)) // WORK IN PROGRESS
//    HandleAttackDown();
//if (Input.GetKeyDown(KeyCode.C))
//    HandleDash();