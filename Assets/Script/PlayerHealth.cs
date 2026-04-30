using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    public Transform respawnPoint;

    CharacterController controller;
    FPSController fps;

    [Header("UI")]
    public Image damageOverlay;
    public Image deathOverlay;

    public float fadeSpeed = 5f;

    void Start()
    {
        currentHealth = maxHealth;

        controller = GetComponent<CharacterController>();
        fps = GetComponent<FPSController>();

        if (respawnPoint == null)
            respawnPoint = transform;
    }

    void Update()
    {
        float alpha = 1f - (currentHealth / maxHealth);

        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = Mathf.Lerp(c.a, alpha * 0.6f, Time.deltaTime * fadeSpeed);
            damageOverlay.color = c;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        controller.enabled = false;
        fps.enabled = false;

        yield return StartCoroutine(Fade(deathOverlay, 1f));

        yield return new WaitForSeconds(1f);

        currentHealth = maxHealth;

        // 🔥 ไป checkpoint ล่าสุด
        transform.position = Checkpoint.lastCheckpoint;

        // 🔥 RESET ENEMY (เวอร์ชันใหม่)
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy e in enemies)
        {
            e.ResetEnemy();
        }

        controller.enabled = true;
        fps.enabled = true;

        yield return StartCoroutine(Fade(deathOverlay, 0f));
    }

    IEnumerator Fade(Image img, float target)
    {
        if (img == null) yield break;

        float start = img.color.a;
        float time = 0f;

        while (time < 0.5f)
        {
            float a = Mathf.Lerp(start, target, time / 0.5f);

            Color c = img.color;
            c.a = a;
            img.color = c;

            time += Time.deltaTime;
            yield return null;
        }

        Color final = img.color;
        final.a = target;
        img.color = final;
    }
}