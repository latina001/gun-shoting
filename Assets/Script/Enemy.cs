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
    public float attackRange = 10f; // 🎯 ระยะยิง
    public float damage = 10f;

    [Header("Shooting")]
    public float fireRate = 1f;
    float nextFireTime = 0f;

    [Range(0f, 1f)]
    public float accuracy = 0.8f;
    public float shootRange = 50f;

    bool isAggro = false;

    void Start()
    {
        // 🔥 บังคับให้ Enemy อยู่บน NavMesh
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
        if (!agent.isOnNavMesh) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectRange || isAggro)
        {
            // 👀 หันหน้าไปหาผู้เล่น
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir);

            // 🏃 ถ้าไกลเกิน → เดินเข้า
            if (distance > attackRange + 1f)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
                anim.SetBool("isWalking", true);
            }
            else
            {
                // 🛑 อยู่ในระยะยิง → หยุด
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

    // 🔫 ยิง
    void Shoot()
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;

        anim.SetTrigger("Shoot");

        // 🎯 ยิงไปกลางตัว player จริง
        Vector3 target;
        Collider col = player.GetComponent<Collider>();

        if (col != null)
            target = col.bounds.center;
        else
            target = player.position;

        Vector3 direction = (target - shootPoint.position).normalized;

        // 🎲 ความมั่ว
        float spread = 1f - accuracy;
        direction += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            Random.Range(-spread, spread)
        );

        // 🔴 debug ray
        Debug.DrawRay(shootPoint.position, direction * shootRange, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, direction, out hit, shootRange))
        {
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
    }

    // ❤️ โดนยิง
    public void TakeDamage(float amount)
    {
        health -= amount;

        isAggro = true; // 🔥 ยิง = โกรธ

        Debug.Log("Enemy HP: " + health);

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