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
<<<<<<< HEAD
    public float attackRange = 2f;
    public float damage = 10f;

    float attackCooldown = 1.5f;
    float nextAttackTime = 0f;

    void Start()
    {
        // 🔥 บังคับให้ Enemy ไปอยู่บน NavMesh
=======
    public float attackRange = 10f;
    public float damage = 10f;

    [Header("Shooting")]
    public float fireRate = 1f;
    float nextFireTime = 0f;

    [Range(0f, 1f)]
    public float accuracy = 0.8f;
    public float shootRange = 50f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    int currentPoint = 0;
    public float waitTime = 2f;
    float waitCounter = 0f;

    bool isAggro = false;

    void Start()
    {
>>>>>>> parent of 9449a69 (Revert "Ver.2")
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
<<<<<<< HEAD
        else
        {
            Debug.LogError("❌ Enemy not on NavMesh!");
        }
=======

        if (shootPoint == null)
            shootPoint = transform;

        GoToNextPoint();
>>>>>>> parent of 9449a69 (Revert "Ver.2")
    }

    void Update()
    {
        if (!agent.isOnNavMesh) return; // ❗ กัน error

        float distance = Vector3.Distance(transform.position, player.position);

<<<<<<< HEAD
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
=======
        // 🔥 ถ้าเห็น player หรือโดนยิง
        if (distance <= detectRange || isAggro)
        {
            HandleCombat(distance);
        }
        else
        {
            Patrol();
        }
    }

    void HandleCombat(float distance)
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);

        if (distance > attackRange + 1f)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetBool("isWalking", true);
>>>>>>> parent of 9449a69 (Revert "Ver.2")
        }
        else
        {
            anim.SetBool("isWalking", false);

            Shoot();
        }
    }

<<<<<<< HEAD
    void Attack()
=======
    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        anim.SetBool("isWalking", true);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitCounter += Time.deltaTime;

            if (waitCounter >= waitTime)
            {
                GoToNextPoint();
                waitCounter = 0f;
            }
        }
    }

    void GoToNextPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPoint].position);

        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    void Shoot()
>>>>>>> parent of 9449a69 (Revert "Ver.2")
    {
        if (!agent.isOnNavMesh) return; // ❗ กัน error

        agent.SetDestination(transform.position);

<<<<<<< HEAD
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
=======
        if (anim != null)
            anim.SetTrigger("Shoot");

        Vector3 target;
        Collider col = player.GetComponent<Collider>();

        if (col != null)
            target = col.bounds.center;
        else
            target = player.position;

        Vector3 direction = (target - shootPoint.position).normalized;

        float spread = 1f - accuracy;
        direction += new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            Random.Range(-spread, spread)
        );

        // 🔴 เส้น Ray (ดูใน Scene)
        Debug.DrawRay(shootPoint.position, direction * shootRange, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, direction, out hit, shootRange))
        {
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

    public void TakeDamage(float amount)
    {
        health -= amount;
        isAggro = true;
>>>>>>> parent of 9449a69 (Revert "Ver.2")

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