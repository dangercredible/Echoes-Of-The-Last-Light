using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RoundCompleteController : MonoBehaviour
{
    public static RoundCompleteController Instance { get; private set; }
    public static bool IsShowing { get; private set; }

    [Header("UI")]
    public bool createUIIfMissing = true;
    public string titleText = "ROUND COMPLETE";
    public string bodyText = "You reached the end of the path.";
    public string hintText = "Press R to play again";

    Canvas canvas;
    GameObject panel;
    Text message;
    bool shown;

    public static RoundCompleteController InstanceOrFind()
    {
        if (Instance != null)
            return Instance;
        Instance = FindFirstObjectByType<RoundCompleteController>();
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsShowing = false;

        if (createUIIfMissing)
            EnsureUI();
        Hide();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsShowing = false;
        }
    }

    public void ShowRoundComplete()
    {
        if (shown)
            return;

        shown = true;
        IsShowing = true;
        EnsureUI();

        if (panel != null)
            panel.SetActive(true);

        if (message != null)
            message.text = $"{titleText}\n\n{bodyText}\n\n{hintText}";

        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!shown)
            return;

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            ReloadScene();
    }

    void ReloadScene()
    {
        Time.timeScale = 1f;
        shown = false;
        IsShowing = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void EnsureUI()
    {
        if (canvas != null && panel != null && message != null)
            return;

        GameObject existing = GameObject.Find("RoundCompleteUI");
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
            }
            return;
        }

        GameObject root = new GameObject("RoundCompleteUI");
        DontDestroyOnLoad(root);

        canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        root.AddComponent<GraphicRaycaster>();

        panel = new GameObject("Panel");
        panel.transform.SetParent(canvas.transform, false);
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.06f, 0.05f, 0.14f, 0.88f);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Message");
        textObj.transform.SetParent(panel.transform, false);
        message = textObj.AddComponent<Text>();
        message.alignment = TextAnchor.MiddleCenter;
        message.color = new Color(0.95f, 0.93f, 1f, 1f);
        message.fontSize = 38;
        message.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(40f, 40f);
        textRt.offsetMax = new Vector2(-40f, -40f);
    }
}
