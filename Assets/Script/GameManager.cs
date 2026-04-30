using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    public GameObject winPanel;
    public TMP_Text timeText;
    public TMP_Text resultText;
    public TMP_Text timerText; // 🔥 เพิ่ม

    [Header("Timer")]
    public bool timerRunning = true;
    float timer;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (!timerRunning) return;

        timer += Time.deltaTime;

        // 🔥 แสดงเวลาระหว่างเล่น
        if (timerText != null)
            timerText.text = "Time: " + timer.ToString("F2");
    }

    public void BossKilled()
    {
        timerRunning = false;

        if (winPanel != null)
            winPanel.SetActive(true);

        if (resultText != null)
            resultText.text = "YOU WIN!";

        if (timeText != null)
            timeText.text = "Time: " + timer.ToString("F2") + " sec";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}