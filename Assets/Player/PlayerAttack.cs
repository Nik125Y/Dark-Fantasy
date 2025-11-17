using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public float attackCooldown = 0.4f;
    public float attackActiveTime = 0.15f;

    private Animator animator;
    private bool canAttack = true;

    private GameObject attackHitbox;
    private PlayerControls controls;

    private void Awake()
    {
        //animator = GetComponent<Animator>();
        animator = GetComponentInChildren<Animator>();

        controls = new PlayerControls();
        //controls.Player.Attack.performed += ctx => TryAttack();
        controls.Player.Attack.started += ctx => TryAttack();

        attackHitbox = transform.Find("AttackHitbox")?.gameObject;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void TryAttack()
    {
        if (!canAttack) return;

        // always find hitbox here
        //attackHitbox = transform.Find("AttackHitbox")?.gameObject;
        if (attackHitbox == null)
        {
            //Debug.LogError("AttackHitbox not found under Player!");
            return;
        }

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;

        animator.SetTrigger("Attack");

        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackActiveTime);
        attackHitbox.SetActive(false);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}

