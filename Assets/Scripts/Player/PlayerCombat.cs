using System;
using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private Player core;

    [Header("Heal Settings")]
    [SerializeField] private float chargeSpeed = 0.2f;
    [SerializeField] private float healFillSpeed = 1.5f;
    private float currentHealCharge = 0f;
    private bool isRegenerating = false;
    private bool isHealButtonHeld = false;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 0.25f;
    private bool canAttack = true;

    [Header("Restrictions")]
    [SerializeField] private float batDetectionRadius = 5f;

    private void Start()
    {
        core = Player.Instance;
        GameInput.Instance.OnPlayerAttack += OnAttackInput;
        GameInput.Instance.OnPlayerHealHoldStarted += StartHealing;
        GameInput.Instance.OnPlayerHealHoldEnded += StopHealing;
    }

    private void Update()
    {
        if (!isRegenerating && currentHealCharge < core.maxHealth)
        {
            currentHealCharge += Time.deltaTime * chargeSpeed;
            currentHealCharge = Mathf.Min(currentHealCharge, core.maxHealth);
            core.InvokeHealProgressEvent(currentHealCharge / core.maxHealth);
        }
    }

    private void OnAttackInput(object sender, EventArgs e)
    {
        if (!canAttack || !core.IsAlive()) return;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;
        bool isRight = UnityEngine.Random.value > 0.3f;

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger(isRight ? "AttackRight" : "AttackLeft");

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void StartHealing(object sender, EventArgs e)
    {
        if (core.Health >= core.maxHealth || IsBatNearby()) return;

        isHealButtonHeld = true;
        if (!isRegenerating && currentHealCharge >= 1f && core.Health < core.maxHealth)
        {
            StartCoroutine(GradualHealRoutine());
        }
    }

    private void StopHealing(object sender, EventArgs e) => isHealButtonHeld = false;

    private IEnumerator GradualHealRoutine()
    {
        isRegenerating = true;
        float internalHealthAccumulator = 0f;

        while (isHealButtonHeld && currentHealCharge > 0 && core.Health < core.maxHealth && !IsBatNearby())
        {
            float chargeToSpend = Time.deltaTime * healFillSpeed;
            chargeToSpend = Mathf.Min(chargeToSpend, currentHealCharge);

            currentHealCharge -= chargeToSpend;
            internalHealthAccumulator += chargeToSpend;

            if (internalHealthAccumulator >= 1f)
            {
                int healAmount = Mathf.FloorToInt(internalHealthAccumulator);

                core.Heal(healAmount);

                internalHealthAccumulator -= healAmount;
            }

            core.InvokeHealProgressEvent(currentHealCharge / core.maxHealth);
            yield return null;
        }
        isRegenerating = false;
    }

    private bool IsBatNearby()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, batDetectionRadius);
        foreach (var col in colliders)
        {
            if (col.GetComponent<BatAI>() != null) return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPlayerAttack -= OnAttackInput;
            GameInput.Instance.OnPlayerHealHoldStarted -= StartHealing;
            GameInput.Instance.OnPlayerHealHoldEnded -= StopHealing;
        }
    }
}