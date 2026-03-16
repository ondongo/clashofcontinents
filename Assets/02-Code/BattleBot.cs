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

    private float currentHealth;
    private float cooldownTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        BattleBot target = FindClosestEnemy();
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist > attackRange)
        {
            Vector3 dir = (target.transform.position - transform.position).normalized;
            dir.y = 0f;

            transform.position += dir * moveSpeed * Time.deltaTime;

            if (dir.sqrMagnitude > 0.001f)
                transform.forward = dir;
        }
        else
        {
            if (cooldownTimer <= 0f)
            {
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
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Destroy(gameObject);
    }
}