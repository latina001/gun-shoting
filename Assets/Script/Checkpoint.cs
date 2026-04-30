using UnityEngine;
using TMPro;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    public static Vector3 lastCheckpoint;

    [Header("UI")]
    public GameObject checkpointText;
    public TMP_Text textLabel;

    bool used = false;

    void Start()
    {
        lastCheckpoint = transform.position;

        if (checkpointText != null)
            checkpointText.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !used)
        {
            used = true;

            lastCheckpoint = transform.position;
            StartCoroutine(ShowCheckpointText());
        }
    }

    IEnumerator ShowCheckpointText()
    {
        checkpointText.SetActive(true);
        textLabel.text = "Checkpoint Reached!";

        yield return StartCoroutine(FadeText(0f, 1f, 0.5f)); // fade in

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeText(1f, 0f, 0.5f)); // fade out

        checkpointText.SetActive(false);
    }

    IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float time = 0f;
        Color color = textLabel.color;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            textLabel.color = new Color(color.r, color.g, color.b, alpha);

            time += Time.deltaTime;
            yield return null;
        }

        // กันค่าเพี้ยนตอนจบ
        textLabel.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}