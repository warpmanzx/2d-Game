using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Entity : MonoBehaviour
{
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer sr;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected int currentHealth;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFeedbackDuration = 0.2f;
    private Coroutine damageFeedbackCoroutine;

    [Header("Attack details")]
    [SerializeField] protected int attackDamage = 1;
    [SerializeField] protected float attackRadius;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected Transform attackPointAbove;
    [SerializeField] protected Transform attackPointBelow;
    [SerializeField] protected LayerMask whatIsTarget;

    [Header("Movement details")]
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float jumpForce = 8f;
    [SerializeField] protected int jumpsRemaining;
    [SerializeField] protected float recoilForce = 5f;
    protected int maxJumps = 1;

    [Header("GroundCheck")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    protected bool isGrounded;

    [Header("Effects")]
    [SerializeField] private GameObject hitFlashEffectPrefab;

    private Material originalMaterial;

    protected int facingDir = 1;
    protected bool isDead = false;
    protected bool canMove = true;
    protected bool facingRight = true;
    protected bool canJump = true;
    protected bool canFlip = true;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        originalMaterial = sr.sharedMaterial;
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        HandleCollision();
        HandleMovement();
        HandleAnimations();
        HandleFlip();
    }

    public virtual void DamageTargets()
    {
        Collider2D[] colliderTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);
        HashSet<Entity> damagedEntities = new HashSet<Entity>(); // Håller koll på vilka som redan fått skada
        Entity attacker = GetComponent<Entity>();

        foreach (Collider2D target in colliderTargets)
        {
            Entity entityTarget = target.GetComponent<Entity>();

            // Ignorera om target är null, samma som attacker, eller redan fått skada
            if (entityTarget == null || entityTarget == attacker || damagedEntities.Contains(entityTarget))
                continue;

            // Ge recoil på den man slår
            Vector2 recoilDirTarget = (entityTarget.transform.position - transform.position).normalized;
            entityTarget.ApplyRecoil(recoilDirTarget);

            // Ge skada
            entityTarget.TakeDamage(attacker.attackDamage, false);
            damagedEntities.Add(entityTarget);

            // Debug-loggar
            Debug.Log(entityTarget.name + " tog " + attacker.attackDamage + " skada av " + attacker.name);
            Debug.Log(entityTarget.name + " hp är nu " + entityTarget.currentHealth);
        }
    }

    public void ApplyRecoil(Vector2 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Nollställ hastighet först
            rb.AddForce(direction.normalized * recoilForce, ForceMode2D.Impulse);
        }
    }
    public virtual void DamageTargetsAbove()
    {
        Collider2D[] colliderTargets = Physics2D.OverlapCircleAll(attackPointAbove.position, attackRadius, whatIsTarget);

        foreach (Collider2D target in colliderTargets)
        {
            Entity attacker = GetComponent<Entity>();
            Entity entityTarget = target.GetComponent<Entity>();
            entityTarget.TakeDamage(attacker.attackDamage, false);
            Debug.Log(entityTarget.name + " tog " + attacker.attackDamage + " skada av " + attacker.name);
            Debug.Log(entityTarget.name + " hp är nu " + entityTarget.currentHealth);
        }
    }

    public virtual void DamageTargetsBelow()
    {
        Collider2D[] colliderTargets = Physics2D.OverlapCircleAll(attackPointBelow.position, attackRadius, whatIsTarget);

        foreach (Collider2D target in colliderTargets)
        {
            Entity attacker = GetComponent<Entity>();
            Entity entityTarget = target.GetComponent<Entity>();
            entityTarget.TakeDamage(attacker.attackDamage, false);
            Debug.Log(entityTarget.name + " tog " + attacker.attackDamage + " skada av " + attacker.name);
            Debug.Log(entityTarget.name + " hp är nu " + entityTarget.currentHealth);
        }
    }

    public virtual void TakeDamage(int dmg, bool byCollision)
    {
        if (isDead)
            return;
        currentHealth = currentHealth - dmg;
        if (currentHealth <= 0)
        {
            if (!isDead && !byCollision)
            {
                SpawnHitEffect();
                PlayDamageFeedback();
            }
            Die();
            isDead = true;
        }
        else if (currentHealth > 0)
        {
            if (!byCollision)
            {
                SpawnHitEffect();
                PlayDamageFeedback();
            }
            Hurt();
        }
    }

    protected virtual void Hurt()
    {
        anim.SetTrigger("hurt");
    }

    protected virtual void Die()
    {
        anim.SetTrigger("die");
    }

    protected virtual void SpawnHitEffect()
    {
        if (hitFlashEffectPrefab != null)
        {
            Instantiate(hitFlashEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    protected virtual void PlayDamageFeedback()
    {
        if (damageFeedbackCoroutine != null)
            StopCoroutine(damageFeedbackCoroutine);
        damageFeedbackCoroutine = StartCoroutine(DamageFeedbackCo());

    }

    private IEnumerator DamageFeedbackCo()
    {
        sr.material = damageMaterial;
        yield return new WaitForSeconds(damageFeedbackDuration);

        sr.material = originalMaterial;
    }

    public virtual void EnableMovementAndJumpAndFlip(bool enable)
    {
        canMove = enable;
        canJump = enable;
        canFlip = enable;
    }

    protected void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
    }
    
    protected virtual void HandleAttack()
    {
        anim.SetTrigger("attack");
    }

    protected virtual void TryToJump()
    {
        if (isGrounded && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
        }
    }
    protected virtual void HandleMovement()
    {

    }

    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
    }
    // Vänder spelobjektet beroende på rörelseriktningen
    protected virtual void HandleFlip()
    {
        if (rb.linearVelocityX > 0 && !facingRight && canFlip || rb.linearVelocityX < 0 && facingRight && canFlip)
            Flip();
    }

    protected void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir = facingDir * -1;
    }

    // Ritar en linje för att visa groundchecken i scenen
    // Ritar även en cirkel för att visa attackområdet
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}


//public virtual void DamageTargets() // GAMMAL KOD
//{
//    Collider2D[] colliderTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);

//    foreach (Collider2D target in colliderTargets)
//    {
//        Entity attacker = GetComponent<Entity>();
//        Entity entityTarget = target.GetComponent<Entity>();
//        entityTarget.TakeDamage(attacker.attackDamage, false);
//        Debug.Log(entityTarget.name + " tog " + attacker.attackDamage + " skada av " + attacker.name);
//        Debug.Log(entityTarget.name + " hp är nu " + entityTarget.currentHealth);
//    }
//}