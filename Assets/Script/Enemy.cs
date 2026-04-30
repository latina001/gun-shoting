using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;
    public Transform shootPoint;

    [Header("Health Settings")]
    public float health = 100f;
    private float maxHealth; // ตัวแปรสำหรับจำค่าเลือดเริ่มต้น (Max HP)

    [Header("Damage Settings")]
    public float damage = 10f;
    public float headshotMultiplier = 2f;

    [Header("Range Damage Drop")]
    public float minDamageMultiplier = 0.4f;
    public float maxShootRange = 50f;

    [Header("AI Settings")]
    public float detectRange = 15f;
    public float attackRange = 10f;
    public float loseRange = 25f;

    public float fireRate = 1f;
    float nextFireTime;

    public float accuracy = 0.8f;

    public Transform[] patrolPoints;
    int index = 0;
    float wait;

    bool aggro;
    bool isDead = false;

    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // เก็บค่าเลือดเริ่มต้นไว้ใช้ตอน Reset (สำคัญมากสำหรับ Boss)
        maxHealth = health;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (shootPoint == null)
            shootPoint = transform;

        // บันทึกตำแหน่งและมุมหมุนเริ่มต้น
        startPos = transform.position;
        startRot = transform.rotation;

        // ตรวจสอบตำแหน่งบน NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        agent.stoppingDistance = 1f;
        GoPatrol();
    }

    void Update()
    {
        if (isDead || agent == null || player == null) return;
        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // ระบบเลิกตาม (Lose Aggro)
        if (aggro && dist > loseRange)
        {
            aggro = false;
            GoPatrol();
            return;
        }

        // ระบบตรวจจับ (Detect)
        if (dist <= detectRange)
            aggro = true;

        if (aggro)
            Chase(dist);
        else
            Patrol();
    }

    void Chase(float dist)
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (anim != null) anim.SetBool("isWalking", true);

        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        // ระยะโจมตี
        if (dist <= attackRange)
        {
            agent.isStopped = true;
            if (anim != null) anim.SetBool("isWalking", false);
            Shoot(dist);
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (anim != null) anim.SetBool("isWalking", true);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            wait += Time.deltaTime;
            if (wait > 0.5f)
            {
                GoPatrol();
                wait = 0;
            }
        }
    }

    void GoPatrol()
    {
        if (patrolPoints.Length == 0 || !agent.isOnNavMesh) return;

        agent.SetDestination(patrolPoints[index].position);
        index = (index + 1) % patrolPoints.Length;
    }

    void Shoot(float distanceToPlayer)
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        if (anim != null) anim.SetTrigger("Shoot");

        Vector3 target = player.position + Vector3.up * 1.5f;
        Vector3 dir = (target - shootPoint.position).normalized;

        float spread = 1f - accuracy;
        Vector2 randomCircle = Random.insideUnitCircle * spread;

        dir += new Vector3(randomCircle.x, randomCircle.y * 0.2f, 0);
        dir = dir.normalized;

        Debug.DrawRay(shootPoint.position, dir * maxShootRange, Color.red, 1f);

        if (Physics.Raycast(shootPoint.position, dir, out RaycastHit hit, maxShootRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var hp = hit.transform.GetComponentInParent<PlayerHealth>();
                if (hp != null)
                {
                    float finalDamage = damage;

                    // 🎯 HEADSHOT
                    if (hit.collider.name.ToLower().Contains("head"))
                    {
                        finalDamage *= headshotMultiplier;
                    }

                    // 📉 ลดดาเมจตามระยะ
                    float distPercent = Mathf.Clamp01(1f - (distanceToPlayer / maxShootRange));
                    float rangeMultiplier = Mathf.Lerp(minDamageMultiplier, 1f, distPercent);

                    finalDamage *= rangeMultiplier;

                    hp.TakeDamage(finalDamage);
                }
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        health -= dmg;
        aggro = true; // เมื่อโดนยิงจะ Aggro ทันที

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        if (anim != null) anim.SetTrigger("Die");
        agent.enabled = false;
        Destroy(gameObject, 3f);
    }

    // ฟังก์ชันรีเซ็ตตัวละคร (ใช้ตอน Boss เริ่มใหม่ หรือตายแล้วเกิดใหม่)
    public void ResetEnemy()
    {
        isDead = false;
        health = maxHealth; // คืนค่าเลือดตามเลือดสูงสุดที่บันทึกไว้ในตอน Start
        aggro = false;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(startPos); // วาร์ปกลับจุดเกิด
            if (agent.isOnNavMesh) agent.ResetPath();
        }

        transform.position = startPos;
        transform.rotation = startRot;

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        GoPatrol();
        Debug.Log(gameObject.name + " has been reset with " + health + " HP.");
    }
}