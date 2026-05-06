using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator anim;
    public Transform shootPoint;

    [Header("Muzzle Flash (Image)")]
    public SpriteRenderer muzzleFlashSprite;
    public float muzzleFlashDuration = 0.05f;
    public float muzzleFlashScale = 0.1f;
    float muzzleFlashTimer;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip shootSound;

    [Header("Health")]
    public float health = 100f;
    float maxHealth;

    [Header("Boss")]
    public bool isBoss = false;

    [Header("Damage")]
    public float damage = 10f;
    public float headshotMultiplier = 2f;

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
    bool isDead;

    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        maxHealth = health;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        startPos = transform.position;
        startRot = transform.rotation;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        agent.stoppingDistance = 1f;

        if (muzzleFlashSprite != null)
            muzzleFlashSprite.enabled = false;

        GoPatrol();
    }

    void Update()
    {
        if (isDead || player == null || !agent.isOnNavMesh) return;

        if (muzzleFlashSprite != null && muzzleFlashSprite.enabled)
        {
            muzzleFlashTimer -= Time.deltaTime;
            if (muzzleFlashTimer <= 0f)
                muzzleFlashSprite.enabled = false;
        }

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

    void ShowMuzzleFlash()
    {
        if (muzzleFlashSprite == null) return;

        muzzleFlashSprite.enabled = true;

        float randomAngle = Random.Range(0f, 360f);
        muzzleFlashSprite.transform.localRotation = Quaternion.Euler(0f, 0f, randomAngle);

        float randomScale = muzzleFlashScale * Random.Range(0.8f, 1.2f);
        muzzleFlashSprite.transform.localScale = Vector3.one * randomScale;

        muzzleFlashTimer = muzzleFlashDuration;
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

        ShowMuzzleFlash();

        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        Vector3 target = player.position + Vector3.up * 1.5f;
        Vector3 dir = (target - shootPoint.position).normalized;

        float spread = 1f - accuracy;
        Vector2 rnd = Random.insideUnitCircle * spread;

        dir += new Vector3(rnd.x, rnd.y * 0.2f, 0);
        dir = dir.normalized;

        Debug.DrawRay(shootPoint.position, dir * 50f, Color.red, 1f);

        if (Physics.Raycast(shootPoint.position, dir, out RaycastHit hit, 50f))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var hp = hit.transform.GetComponentInParent<PlayerHealth>();
                if (hp != null)
                    hp.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;

        health -= dmg;
        aggro = true;

        if (health <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        if (anim != null)
            anim.SetTrigger("Die");

        if (isBoss && GameManager.instance != null)
            GameManager.instance.BossKilled();

        agent.enabled = false;
        Destroy(gameObject, 2f);
    }

    public void ResetEnemy()
    {
        isDead = false;
        health = maxHealth;
        aggro = false;

        agent.enabled = true;
        agent.Warp(startPos);
        agent.ResetPath();

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