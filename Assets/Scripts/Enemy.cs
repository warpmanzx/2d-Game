
using UnityEngine;

public class Enemy : Entity
{

    private bool playerDetected;
    private bool playerInRange;
    [Header("Detection Boxes")]
    [SerializeField] private Transform DetectionBox;
    [SerializeField] private Transform Player;
    // Skelettet ska gå efter spelaren om den är inom en viss range och vända sig mot spelaren om skelettet kollar bort från spelaren

    protected override void Update()
    {
        base.Update();
        HandleAttack();
        CheckForPlayers();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(attackDamage, true);
        }
    }

    private void CheckForPlayers()
    {
        playerInRange = Physics2D.OverlapBox(DetectionBox.position, new Vector2(DetectionBox.localScale.x, DetectionBox.localScale.y), 0, whatIsTarget);
    }

    protected override void HandleAttack()
    {
        if (playerDetected)
            anim.SetTrigger("attack");
    }

    protected override void HandleMovement()
    {
        if (canMove && playerInRange)
        {
            if (Player.position.x < transform.position.x)
                rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocityY);
            else if (Player.position.x > transform.position.x)
                rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocityY);
        }
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();
        playerDetected = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsTarget);
    }

    protected override void HandleFlip()
    {
        if (Player.position.x < transform.position.x && facingRight && canFlip && playerInRange)
        {
            Flip();
        }
        else if (Player.position.x > transform.position.x && !facingRight && canFlip && playerInRange)
        {
            Flip();
        }
    }

    protected override void Die()
    {
        base.Die();
        Destroy(gameObject, 10f);
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawWireCube(DetectionBox.position, new Vector3(DetectionBox.localScale.x, DetectionBox.localScale.y, 1));
    }
}
