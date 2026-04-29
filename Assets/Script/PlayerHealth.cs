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
    public Image damageOverlay; // 🔴 จอแดง
    public Image deathOverlay;  // 🖤 จอดำ

    public float fadeSpeed = 5f;

    void Start()
    {
        currentHealth = maxHealth;

        controller = GetComponent<CharacterController>();
        fps = GetComponent<FPSController>();
    }

    void Update()
    {
        // 🔴 จอแดงตามเลือด
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

        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("💀 Player Dead!");
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        // 🔒 ปิดการควบคุม
        controller.enabled = false;
        fps.enabled = false;

        // 🖤 fade ดำ
        yield return StartCoroutine(Fade(deathOverlay, 1f));

        yield return new WaitForSeconds(1f);

        // 🔄 รีค่า
        currentHealth = maxHealth;

        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        // 🔓 เปิดควบคุม
        controller.enabled = true;
        fps.enabled = true;

        // 🖤 fade กลับ
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