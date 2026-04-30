using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;
    public Transform shootPoint;

    public float health = 100f;

    public float detectRange = 15f;
    public float attackRange = 10f;
    public float loseRange = 25f;

    public float fireRate = 1f;
    float nextFireTime;

    public float accuracy = 0.8f;
    public float shootRange = 50f;

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
        agent.updatePosition = true;
        agent.updateRotation = true;

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
            ChaseAndFight(dist);
        else
            Patrol();
    }

    void ChaseAndFight(float dist)
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
            Shoot();
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

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[index].position);

        index = (index + 1) % patrolPoints.Length;
    }

    void Shoot()
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

        Debug.DrawRay(shootPoint.position, dir * shootRange, Color.red, 1f);

        if (Physics.Raycast(shootPoint.position, dir, out RaycastHit hit, shootRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var hp = hit.transform.GetComponentInParent<PlayerHealth>();
                if (hp != null)
                    hp.TakeDamage(10);
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

    // 🔥 RESET ENEMY
    public void ResetEnemy()
    {
        health = 100f;
        aggro = false;

        if (agent != null)
        {
            agent.enabled = false;
            transform.position = startPos;
            transform.rotation = startRot;
            agent.enabled = true;
        }

        GoPatrol();
    }
}