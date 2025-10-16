using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Damage Settings")]
    public float invincibilityTime = 1f; // time before player can be hit again
    private bool isInvincible = false;

    private Animator animator;
    private HeroController heroController;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        heroController = GetComponent<HeroController>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateHearts();
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
                hearts[i].sprite = fullHeart;
            else
                hearts[i].sprite = emptyHeart;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHearts();
        StartCoroutine(Invincibility());

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtRoutine());
        }
    }

    private IEnumerator HurtRoutine()
    {
        animator.SetTrigger("Hurt");

        // make player briefly red to indicate damage
        //spriteRenderer.color = Color.red;

        //yield return new WaitForSeconds(0.2f);
        //spriteRenderer.color = Color.white;

        // back to normal control after short delay
        yield return new WaitForSeconds(0.3f);
        animator.ResetTrigger("Hurt");
    }


    private IEnumerator Invincibility ()
    {
        isInvincible = true;

        // blink effect while invincible
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        isInvincible = false;
    }


    private void Die ()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Death");

        // disable player control and physics
        heroController.enabled = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;

        // optional: disable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DeathRoutine());
    }


    private IEnumerator DeathRoutine ()
    {
        // wait for death animation to finish
        yield return new WaitForSeconds(3f);

        // hide or destroy player
        gameObject.SetActive(false);

        if (RespawnManager.Instance != null)
        {
            StartCoroutine(RespawnManager.Instance.RespawnPlayer());
        }
    }
   

    private void OnTriggerEnter2D (Collider2D other)
    {
        // detect hazard layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Hazard"))
        {
            TakeDamage(1);
        }
    }
}
