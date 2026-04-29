using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;
    public Transform shootPoint;

    [Header("Stats")]
    public float health = 100f;
    public float detectRange = 15f;
    public float attackRange = 10f;
    public float damage = 10f;

    [Header("Shooting")]
    public float fireRate = 1f;
    float nextFireTime = 0f;

    [Range(0f, 1f)]
    public float accuracy = 0.85f;
    public float shootRange = 50f;

    bool isAggro = false;

    void Start()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        if (shootPoint == null)
            shootPoint = transform;
    }

    void Update()
    {
        if (!agent.isOnNavMesh || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectRange || isAggro)
        {
            // 👀 หันหน้า
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 10f
            );

            // 🏃 เดินเฉพาะตอน "ไกลจริง"
            if (distance > attackRange + 1f)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
                anim.SetBool("isWalking", true);
            }
            else
            {
                // 🛑 อยู่ในระยะ → หยุดยิง
                agent.isStopped = true;
                anim.SetBool("isWalking", false);

                Shoot();
            }
        }
        else
        {
            agent.isStopped = true;
            anim.SetBool("isWalking", false);
        }
    }

    void Shoot()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;

        if (anim != null)
            anim.SetTrigger("Shoot");

        // 🎯 เล็ง "กลางตัวจริง"
        Vector3 target;
        Collider col = player.GetComponent<Collider>();

        if (col != null)
            target = col.bounds.center;
        else
            target = player.position + Vector3.up * 1.5f;

        Vector3 direction = (target - shootPoint.position).normalized;

        // 🎲 spread
        float spread = 1f - accuracy;
        direction += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            Random.Range(-spread, spread)
        );

        RaycastHit hit;

        if (Physics.Raycast(shootPoint.position, direction, out hit, shootRange))
        {
            // 🔴 ยิงโดน
            Debug.DrawLine(shootPoint.position, hit.point, Color.red, 1f);

            Debug.Log("Enemy ยิงโดน: " + hit.transform.name);

            if (hit.transform.CompareTag("Player"))
            {
                PlayerHealth ph = hit.transform.GetComponentInParent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                }
            }
        }
        else
        {
            // 🔵 ยิงพลาด
            Debug.DrawLine(
                shootPoint.position,
                shootPoint.position + direction * shootRange,
                Color.blue,
                1f
            );
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;

        isAggro = true; // 🔥 ยิง = โกรธทันที

        Debug.Log("Enemy HP: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (anim != null)
            anim.SetTrigger("Die");

        if (agent != null)
            agent.enabled = false;

        Destroy(gameObject, 2f);
    }
}