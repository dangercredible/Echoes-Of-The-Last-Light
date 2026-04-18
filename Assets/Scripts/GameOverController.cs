using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    static GameOverController instance;
    public static GameOverController InstanceOrFind()
    {
        if (instance != null)
            return instance;
        instance = FindFirstObjectByType<GameOverController>();
        return instance;
    }

    [Header("UI")]
    public bool createUIIfMissing = true;
    public string titleText = "GAME OVER";
    public string restartHint = "Press R to Restart";
    public string healthPrefix = "Health";

    Canvas canvas;
    GameObject panel;
    Text message;
    Text healthText;
    PlayerHealth trackedHealth;

    bool gameEnded;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        if (createUIIfMissing)
            EnsureUI();

        Hide();
    }

    void Update()
    {
        UpdateHealthHUD();

        if (!gameEnded)
            return;

        if (Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    public void GameOver(string reason)
    {
        if (gameEnded)
            return;

        gameEnded = true;
        EnsureUI();

        if (panel != null)
            panel.SetActive(true);

        if (message != null)
            message.text = $"{titleText}\n\n{reason}\n\n{restartHint}";

        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        gameEnded = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void UpdateHealthHUD()
    {
        EnsureUI();

        if (trackedHealth == null || trackedHealth.gameObject == null)
        {
            GameObject player = GameObject.Find("Player");
            trackedHealth = player != null ? player.GetComponent<PlayerHealth>() : null;
        }

        if (healthText == null)
            return;

        if (trackedHealth == null)
        {
            healthText.text = $"{healthPrefix}: --";
            return;
        }

        if (trackedHealth.IsDead)
            healthText.text = $"{healthPrefix}: 0/{trackedHealth.maxHealth} (DEAD)";
        else
            healthText.text = $"{healthPrefix}: {trackedHealth.CurrentHealth}/{trackedHealth.maxHealth}";
    }

    void EnsureUI()
    {
        if (canvas != null && panel != null && message != null && healthText != null)
            return;

        GameObject existing = GameObject.Find("GameOverUI");
        if (existing != null)
        {
            canvas = existing.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                Transform panelTransform = canvas.transform.Find("Panel");
                if (panelTransform != null)
                {
                    panel = panelTransform.gameObject;
                    Transform messageTransform = panelTransform.Find("Message");
                    if (messageTransform != null)
                        message = messageTransform.GetComponent<Text>();
                }

                Transform healthTransform = canvas.transform.Find("HealthText");
                if (healthTransform != null)
                    healthText = healthTransform.GetComponent<Text>();
            }
            return;
        }

        GameObject root = new GameObject("GameOverUI");
        DontDestroyOnLoad(root);

        canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        root.AddComponent<GraphicRaycaster>();

        panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Message");
        textObj.transform.SetParent(panel.transform, false);
        message = textObj.AddComponent<Text>();
        message.alignment = TextAnchor.MiddleCenter;
        message.color = Color.white;
        message.fontSize = 40;
        message.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(40f, 40f);
        textRt.offsetMax = new Vector2(-40f, -40f);

        GameObject healthObj = new GameObject("HealthText");
        healthObj.transform.SetParent(canvas.transform, false);
        healthText = healthObj.AddComponent<Text>();
        healthText.alignment = TextAnchor.UpperLeft;
        healthText.color = Color.white;
        healthText.fontSize = 24;
        healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform healthRt = healthObj.GetComponent<RectTransform>();
        healthRt.anchorMin = new Vector2(0f, 1f);
        healthRt.anchorMax = new Vector2(0f, 1f);
        healthRt.pivot = new Vector2(0f, 1f);
        healthRt.anchoredPosition = new Vector2(20f, -20f);
        healthRt.sizeDelta = new Vector2(420f, 40f);
    }
}

