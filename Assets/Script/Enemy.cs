using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;

    [Header("Stats")]
    public float health = 100f;
    public float detectRange = 15f;
    public float attackRange = 2f;
    public float damage = 10f;

    float attackCooldown = 1.5f;
    float nextAttackTime = 0f;

    void Start()
    {
        // 🔥 บังคับให้ Enemy ไปอยู่บน NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            Debug.LogError("❌ Enemy not on NavMesh!");
        }
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return; // ❗ กัน error

        float distance = Vector3.Distance(transform.position, player.position);

        // 👀 เห็นผู้เล่น
        if (distance <= detectRange)
        {
            agent.SetDestination(player.position);

            anim.SetBool("isWalking", true);

            // 💀 เข้าใกล้ = โจมตี
            if (distance <= attackRange)
            {
                Attack();
            }
        }
        else
        {
            anim.SetBool("isWalking", false);
        }
    }

    void Attack()
    {
        if (!agent.isOnNavMesh) return; // ❗ กัน error

        agent.SetDestination(transform.position);

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            anim.SetTrigger("Attack");

            Debug.Log("Enemy Attack!");
        }
    }

    // ❤️ รับดาเมจ
    public void TakeDamage(float amount)
    {
        health -= amount;

        Debug.Log("Enemy HP: " + health); // 🔥 เอาไว้เช็ค

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        anim.SetTrigger("Die");

        if (agent != null)
            agent.enabled = false;

        Destroy(gameObject, 2f);
    }
}