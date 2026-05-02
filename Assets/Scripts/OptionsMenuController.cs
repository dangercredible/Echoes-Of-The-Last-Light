using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Options scene: hides legacy placeholder UI, builds a clean overlay with sliders + Back.
/// </summary>
public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] string mainMenuSceneName = "MainMenu";

    void Start()
    {
        EchoesAudioDirector.EnsureExists();
        EchoesGameSettings.EnsureLoaded();
        EchoesAudioDirector.EnsureMenuMusicAudible();

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
            HideLegacyMenuChrome(canvas);

        ClearStalePanelIfAny();
        BuildOptionsUi();
        EchoesUiSoundBootstrap.HookSceneNow(SceneManager.GetActiveScene());
        EchoesBrightness.ApplyFromSettings();
    }

    void OnDestroy()
    {
        EchoesGameSettings.Flush();
    }

    static void HideLegacyMenuChrome(Canvas canvas)
    {
        foreach (Transform t in canvas.transform)
        {
            if (t.name == "GameTitle" || t.name == "ButtonGroup")
                t.gameObject.SetActive(false);
        }
    }

    static void ClearStalePanelIfAny()
    {
        GameObject stale = GameObject.Find("OptionsSettingsPanel");
        if (stale != null)
            Destroy(stale);
    }

    public void GoBack()
    {
        EchoesGameSettings.Flush();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void BuildOptionsUi()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        GameObject panel = new GameObject("OptionsSettingsPanel");
        panel.transform.SetParent(canvas.transform, false);
        panel.transform.SetAsLastSibling();

        RectTransform panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        GameObject overlayRoot = new GameObject("OptionsOverlayCanvas");
        overlayRoot.transform.SetParent(panel.transform, false);
        RectTransform overlayRt = overlayRoot.AddComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        Canvas overlayCanvas = overlayRoot.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = 480;
        overlayRoot.AddComponent<GraphicRaycaster>();

        GameObject dim = new GameObject("Dim");
        dim.transform.SetParent(overlayRoot.transform, false);
        Image dimImg = dim.AddComponent<Image>();
        dimImg.color = new Color(0.02f, 0.025f, 0.06f, 0.55f);
        dimImg.raycastTarget = false;
        RectTransform dimRt = dim.GetComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;

        GameObject card = new GameObject("SettingsCard");
        card.transform.SetParent(overlayRoot.transform, false);
        RectTransform cardRt = card.AddComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.5f);
        cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.sizeDelta = new Vector2(760f, 520f);

        Image cardBg = card.AddComponent<Image>();
        cardBg.color = new Color(0.06f, 0.065f, 0.11f, 0.94f);
        cardBg.raycastTarget = true;

        VerticalLayoutGroup vertical = card.AddComponent<VerticalLayoutGroup>();
        vertical.padding = new RectOffset(36, 36, 32, 28);
        vertical.spacing = 18f;
        vertical.childAlignment = TextAnchor.UpperCenter;
        vertical.childControlHeight = true;
        vertical.childForceExpandHeight = false;
        vertical.childControlWidth = true;
        vertical.childForceExpandWidth = true;

        CreateHeading(card.transform, "OPTIONS", font);

        CreateSliderRow(card.transform, "Master Volume", font, EchoesGameSettings.MasterVolume,
            v => EchoesGameSettings.MasterVolume = v);

        CreateSliderRow(card.transform, "Music Volume", font, EchoesGameSettings.MusicVolume,
            v => EchoesGameSettings.MusicVolume = v);

        CreateSliderRow(card.transform, "SFX Volume", font, EchoesGameSettings.SfxVolume,
            v => EchoesGameSettings.SfxVolume = v);

        CreateSliderRow(card.transform, "Brightness", font, EchoesGameSettings.Brightness,
            v => EchoesGameSettings.Brightness = v);

        CreateBackButton(card.transform, font);
    }

    static void CreateHeading(Transform parent, string text, TMP_FontAsset font)
    {
        GameObject go = new GameObject("Heading");
        go.transform.SetParent(parent, false);
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.minHeight = 44f;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 34;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.9f, 0.88f, 0.82f, 1f);
        tmp.textWrappingMode = TextWrappingModes.Normal;
        if (font != null)
            tmp.font = font;
    }

    void CreateBackButton(Transform parent, TMP_FontAsset font)
    {
        GameObject row = new GameObject("BackRow");
        row.transform.SetParent(parent, false);
        LayoutElement rowLe = row.AddComponent<LayoutElement>();
        rowLe.minHeight = 56f;
        rowLe.flexibleWidth = 1f;

        GameObject btnGo = new GameObject("BackButton");
        btnGo.transform.SetParent(row.transform, false);
        LayoutElement btnLe = btnGo.AddComponent<LayoutElement>();
        btnLe.minHeight = 52f;
        btnLe.preferredHeight = 52f;
        btnLe.flexibleWidth = 1f;

        Image img = btnGo.AddComponent<Image>();
        img.color = new Color(0.14f, 0.15f, 0.22f, 1f);

        Button btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.22f, 0.24f, 0.34f, 1f);
        colors.pressedColor = new Color(0.18f, 0.2f, 0.3f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(GoBack);

        RectTransform btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.anchorMin = Vector2.zero;
        btnRt.anchorMax = Vector2.one;
        btnRt.offsetMin = Vector2.zero;
        btnRt.offsetMax = Vector2.zero;

        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(btnGo.transform, false);
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "BACK";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.88f, 0.86f, 0.95f, 1f);
        tmp.textWrappingMode = TextWrappingModes.Normal;
        if (font != null)
            tmp.font = font;

        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
    }

    static void CreateSliderRow(Transform parent, string label, TMP_FontAsset font, float initialValue,
        UnityAction<float> onValueChanged)
    {
        GameObject row = new GameObject("Row_" + label.GetHashCode());
        row.transform.SetParent(parent, false);
        LayoutElement rowLe = row.AddComponent<LayoutElement>();
        rowLe.minHeight = 50f;
        rowLe.flexibleWidth = 1f;

        HorizontalLayoutGroup horizontal = row.AddComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 16f;
        horizontal.childAlignment = TextAnchor.MiddleLeft;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandHeight = true;
        horizontal.childControlWidth = true;
        horizontal.childForceExpandWidth = true;

        GameObject labelGo = new GameObject("Label");
        labelGo.transform.SetParent(row.transform, false);
        LayoutElement labelLe = labelGo.AddComponent<LayoutElement>();
        labelLe.minWidth = 220f;
        labelLe.preferredWidth = 220f;
        labelLe.flexibleWidth = 0f;

        TextMeshProUGUI tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = new Color(0.86f, 0.84f, 0.92f, 1f);
        tmp.textWrappingMode = TextWrappingModes.Normal;
        if (font != null)
            tmp.font = font;

        GameObject sliderRoot = new GameObject("SliderRoot");
        sliderRoot.transform.SetParent(row.transform, false);
        LayoutElement sliderRootLe = sliderRoot.AddComponent<LayoutElement>();
        sliderRootLe.flexibleWidth = 1f;
        sliderRootLe.minHeight = 34f;

        Slider slider = BuildHandleSlider(sliderRoot.transform, initialValue, onValueChanged);
        slider.SetValueWithoutNotify(initialValue);
    }

    static Slider BuildHandleSlider(Transform parent, float initial, UnityAction<float> onValueChanged)
    {
        GameObject root = new GameObject("Slider");
        root.transform.SetParent(parent, false);
        RectTransform rootRt = root.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        Slider slider = root.AddComponent<Slider>();
        slider.transition = Selectable.Transition.None;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(root.transform, false);
        Image bgImg = background.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.11f, 0.16f, 0.94f);
        RectTransform bgRt = background.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(root.transform, false);
        RectTransform haRt = handleArea.AddComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero;
        haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(10f, 4f);
        haRt.offsetMax = new Vector2(-10f, -4f);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.93f, 0.91f, 1f, 1f);
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(22f, 22f);

        slider.fillRect = null;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;
        slider.navigation = new Navigation { mode = Navigation.Mode.None };

        slider.SetValueWithoutNotify(initial);
        slider.onValueChanged.AddListener(onValueChanged);
        return slider;
    }
}
