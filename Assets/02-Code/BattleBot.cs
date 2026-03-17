using UnityEngine;

public class BattleBot : MonoBehaviour
{
    [Header("Team")]
    public int team = 0;

    [Header("Stats")]
    public float moveSpeed = 2.5f;
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.8f;

    [Header("Optional Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveBoolName = "IsMoving";
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string dieTriggerName = "Die";
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDestroyDelay = 1.25f;

    private float currentHealth;
    private float cooldownTimer;
    private bool isDead = false;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private bool useDefaultHashes =
        true; // seulement valable si tu gardes les noms par défaut

    private void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (isDead) return;

        cooldownTimer -= Time.deltaTime;

        BattleBot target = FindClosestEnemy();
        if (target == null)
        {
            SetMoving(false);
            return;
        }

        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist > attackRange)
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            dir.y = 0f;

            transform.position += dir * moveSpeed * Time.deltaTime;

            if (dir.sqrMagnitude > 0.001f)
                transform.forward = dir;

            SetMoving(true);
        }
        else
        {
            SetMoving(false);

            if (cooldownTimer <= 0f)
            {
                PlayAttackAnimation();
                target.TakeDamage(attackDamage);
                cooldownTimer = attackCooldown;
            }
        }
    }

    private BattleBot FindClosestEnemy()
    {
        BattleBot[] all = FindObjectsOfType<BattleBot>();
        BattleBot best = null;
        float bestDist = Mathf.Infinity;

        foreach (BattleBot bot in all)
        {
            if (bot == null || bot == this) continue;
            if (bot.team == team) continue;
            if (bot.isDead) continue;

            float dist = Vector3.Distance(transform.position, bot.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = bot;
            }
        }

        return best;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        SetMoving(false);
        PlayDeathAnimation();

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            c.enabled = false;

        if (destroyOnDeath)
            Destroy(gameObject, deathDestroyDelay);
    }

    private void SetMoving(bool value)
    {
        if (animator == null) return;

        if (useDefaultHashes && moveBoolName == "IsMoving")
            animator.SetBool(IsMovingHash, value);
        else
            animator.SetBool(moveBoolName, value);
    }

    private void PlayAttackAnimation()
    {
        if (animator == null) return;

        if (useDefaultHashes && attackTriggerName == "Attack")
            animator.SetTrigger(AttackHash);
        else
            animator.SetTrigger(attackTriggerName);
    }

    private void PlayDeathAnimation()
    {
        if (animator == null) return;

        if (useDefaultHashes && dieTriggerName == "Die")
            animator.SetTrigger(DieHash);
        else
            animator.SetTrigger(dieTriggerName);
    }
}