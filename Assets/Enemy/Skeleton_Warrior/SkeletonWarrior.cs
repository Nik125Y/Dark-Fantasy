using UnityEngine;
using System.Collections;

public class SkeletonWarrior : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack, Dead }

    [Header("Patrol")]
    public Transform leftPoint;
    public Transform rightPoint;
    public float idleTime = 1.2f;
    public float patrolSpeed = 1.0f;

    [Header("Detection / Combat")]
    public float detectionRadius = 4f;
    public float attackRange = 0.8f;
    public float chaseSpeed = 1.6f;
    public int maxHealth = 3;
    public int damage = 1;
    public float attackCooldown = 1.2f;
    public LayerMask playerMask;

    Rigidbody2D rb;
    Animator anim;

    State state;
    Transform target;
    bool facingRight = true;
    float nextAttackTime = 0f;
    int hp;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        hp = maxHealth;
    }

    void Start()
    {
        state = State.Idle;
        StartCoroutine(IdleRoutine());
    }

    IEnumerator IdleRoutine()
    {
        anim.Play("Idle");
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(idleTime);
        if (state != State.Dead)
            state = State.Patrol;
    }

    void Update()
    {
        if (state == State.Dead) return;

        DetectPlayer();

        switch (state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.Attack: DoAttack(); break;
        }
    }

    // ------------ DETECTION --------------
    void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerMask);

        if (hit != null)
        {
            target = hit.transform;
            if (state != State.Attack)
                state = State.Chase;
        }
        else
        {
            target = null;
            if (state != State.Attack)
                state = State.Patrol;
        }
    }

    // ------------ PATROL --------------
    void DoPatrol()
    {
        anim.SetBool("isWalking", true);

        float leftX = leftPoint.position.x;
        float rightX = rightPoint.position.x;

        float dir = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);

        if (facingRight && transform.position.x >= rightX) facingRight = false;
        if (!facingRight && transform.position.x <= leftX) facingRight = true;

        ApplyFlip();
    }

    // ------------ CHASE --------------
    void DoChase()
    {
        if (target == null)
        {
            state = State.Patrol;
            return;
        }

        anim.SetBool("isWalking", true);

        float dir = Mathf.Sign(target.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * chaseSpeed, rb.linearVelocity.y);

        facingRight = dir > 0;
        ApplyFlip();

        // атакуем?
        if (Vector2.Distance(transform.position, target.position) <= attackRange)
        {
            state = State.Attack;
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ------------ ATTACK --------------
    void DoAttack()
    {
        if (target == null)
        {
            state = State.Patrol;
            return;
        }

        rb.linearVelocity = Vector2.zero;

        if (Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    IEnumerator AttackRoutine()
    {
        anim.SetTrigger("isAttacking");
        yield return new WaitForSeconds(0.25f); // момент удара

        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange + 0.1f)
        {
            PlayerHealth ph = target.GetComponent<PlayerHealth>();
            if (ph != null)
                ph.TakeDamage(damage);
        }

        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange)
            state = State.Attack;
        else
            state = State.Chase;
    }

    // ------------ DAMAGE --------------
    public void TakeDamage(int amount)
    {
        if (state == State.Dead) return;

        hp -= amount;
        anim.SetTrigger("hit");

        if (hp <= 0)
            Die();
    }

    void Die()
    {
        state = State.Dead;
        anim.SetTrigger("isDead");
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
    }

    void ApplyFlip()
    {
        float scaleX = facingRight ? 1 : -1;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * scaleX;
        transform.localScale = s;
    }
}
