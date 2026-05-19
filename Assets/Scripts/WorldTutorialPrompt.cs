using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Trigger volume that shows short world-space instruction text when the player enters.
/// </summary>
public class WorldTutorialPrompt : MonoBehaviour
{
    [TextArea(8, 24)]
    public string messageText = "";

    [Tooltip("Collider size if none exists — assigned on Start.")]
    public Vector2 triggerSize = new Vector2(5f, 4f);

    public float hideDelayAfterExit = 0.45f;
    public Vector2 canvasWorldSize = new Vector2(9f, 2.8f);
    public float worldScale = 0.028f;

    [Header("Label layout")]
    [Tooltip("Stack letters vertically (one character per line).")]
    public bool verticalCharacters = false;

    [Range(14f, 56f)]
    public float fontSize = 28f;

    [Tooltip("Canvas world size when Vertical Characters is on (narrow, tall).")]
    public Vector2 canvasWorldSizeVertical = new Vector2(2.7f, 9.5f);

    Canvas promptCanvas;
    TextMeshProUGUI label;

    void Start()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
        }

        box.size = triggerSize;

        BuildPromptUi();
        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(false);
    }

    void BuildPromptUi()
    {
        GameObject root = new GameObject("PromptRoot");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = new Vector3(0f, 1.35f, 0f);

        promptCanvas = root.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.WorldSpace;
        promptCanvas.sortingOrder = 500;

        RectTransform canvasRt = root.GetComponent<RectTransform>();
        canvasRt.sizeDelta = verticalCharacters ? canvasWorldSizeVertical : canvasWorldSize;
        canvasRt.localScale = Vector3.one * worldScale;
        canvasRt.localRotation = Quaternion.identity;

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(root.transform, false);
        Image panel = panelObj.AddComponent<Image>();
        panel.color = new Color(0.04f, 0.045f, 0.09f, 0.82f);
        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(root.transform, false);
        label = textObj.AddComponent<TextMeshProUGUI>();
        label.text = FormatForDisplay(messageText);
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;
        label.color = new Color(0.92f, 0.9f, 0.82f, 1f);
        label.textWrappingMode = verticalCharacters ? TextWrappingModes.NoWrap : TextWrappingModes.Normal;
        label.margin = verticalCharacters ? new Vector4(10f, 12f, 10f, 12f) : new Vector4(18f, 14f, 18f, 14f);

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = verticalCharacters ? new Vector2(8f, 8f) : new Vector2(12f, 10f);
        textRt.offsetMax = verticalCharacters ? new Vector2(-8f, -8f) : new Vector2(-12f, -10f);
    }

    string FormatForDisplay(string raw)
    {
        if (!verticalCharacters || string.IsNullOrEmpty(raw))
            return raw;

        var sb = new StringBuilder(raw.Length * 2);
        foreach (char c in raw)
        {
            if (c == '\r')
                continue;
            if (c == '\n')
            {
                sb.Append('\n');
                continue;
            }

            sb.Append(c);
            sb.Append('\n');
        }

        while (sb.Length > 0 && sb[sb.Length - 1] == '\n')
            sb.Length--;

        return sb.ToString();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        CancelInvoke(nameof(HidePrompt));
        if (label != null)
            label.text = FormatForDisplay(messageText);
        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerController>() == null)
            return;

        Invoke(nameof(HidePrompt), hideDelayAfterExit);
    }

    void HidePrompt()
    {
        if (promptCanvas != null)
            promptCanvas.gameObject.SetActive(false);
    }
}
