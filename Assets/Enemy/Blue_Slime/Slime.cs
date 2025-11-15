using UnityEngine;

public class Slime : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 4f;
    public float direction = 1f; // 1 - ������, -1 - �����
    public float checkDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Attack")]
    public float attackRange = 1f;
    public float attackCooldown = 1.5f;
    public int damage = 1;
    public LayerMask playerLayer;

    [Header("State")]
    public bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        Move();
        CheckForAttack();
    }

    void Move()
    {
        // ��������� ������� ����� ����� �������
        Vector2 checkPos = new Vector2(transform.position.x + direction * checkDistance, transform.position.y - 0.5f);
        bool groundAhead = Physics2D.Raycast(checkPos, Vector2.down, 0.8f, groundLayer);

        if (!groundAhead)
        {
            Flip();
            return;
        }

        // ��������� �������������� ��������
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        // ���� �� �������, �� "������������" ����� �� �������
        if (Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            anim.SetBool("isJumping", true);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void Flip()
    {
        direction *= -1f;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    void CheckForAttack()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (player != null && Time.time - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Time.time;
            anim.SetTrigger("isAttacking");

            // ����� ����� �������� (����� ����� ������� ��������)
            player.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("isDead");
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1.0f); // ������ ����� ��������
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}