using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;
    public Transform shootPoint;

    [Header("Health")]
    public float health = 100f;

    [Header("Damage")]
    public float damage = 10f;
    public float headshotMultiplier = 2f;

    [Header("Range Damage Drop")]
    public float minDamageMultiplier = 0.4f;
    public float maxShootRange = 50f;

    [Header("AI")]
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

    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (shootPoint == null)
            shootPoint = transform;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        startPos = transform.position;
        startRot = transform.rotation;

        agent.stoppingDistance = 1f;
        GoPatrol();
    }

    void Update()
    {
        if (agent == null || player == null) return;
        if (!agent.isOnNavMesh) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (aggro && dist > loseRange)
        {
            aggro = false;
            GoPatrol();
            return;
        }

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

        anim.SetBool("isWalking", true);

        Vector3 dir = player.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        if (dist <= attackRange)
        {
            agent.isStopped = true;
            anim.SetBool("isWalking", false);
            Shoot(dist);
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        anim.SetBool("isWalking", true);

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
        if (patrolPoints.Length == 0) return;
        if (!agent.isOnNavMesh) return;

        agent.SetDestination(patrolPoints[index].position);
        index = (index + 1) % patrolPoints.Length;
    }

    // 🔥 ยิงพร้อมระบบดาเมจขั้นสูง
    void Shoot(float distanceToPlayer)
    {
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + fireRate;
        anim.SetTrigger("Shoot");

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
        health -= dmg;
        aggro = true;

        if (health <= 0)
        {
            anim.SetTrigger("Die");
            agent.enabled = false;
            Destroy(gameObject, 2f);
        }
    }

    public void ResetEnemy()
    {
        health = 100f;
        aggro = false;

        agent.ResetPath();
        agent.Warp(startPos);

        transform.position = startPos;
        transform.rotation = startRot;

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        GoPatrol();
    }
}