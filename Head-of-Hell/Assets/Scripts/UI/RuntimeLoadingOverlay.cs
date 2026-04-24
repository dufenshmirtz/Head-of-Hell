using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuntimeLoadingOverlay : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private RectTransform spinnerRoot;
    [SerializeField] private RectTransform spinnerNeedle;
    [SerializeField] private RectTransform progressTrack;
    [SerializeField] private Image progressFill;
    [SerializeField] private Image glowBar;
    [SerializeField] private string baseMessage = "Refreshing player stats";
    [SerializeField] private float dotIntervalSeconds = 0.35f;
    [SerializeField] private float spinnerSpeed = 90f;
    [SerializeField] private float pulseSpeed = 1.6f;
    [SerializeField] private float progressSmoothingSpeed = 1.8f;

    private float timer;
    private int dotCount;
    private float animationTime;
    private float currentProgress;
    private float targetProgress;

    public static RuntimeLoadingOverlay Create(TMP_FontAsset fontAsset = null, string overlayName = "RuntimeLoadingOverlay")
    {
        GameObject root = new GameObject(
            overlayName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(RuntimeLoadingOverlay)
        );

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        rootRect.sizeDelta = Vector2.zero;
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        GameObject dimmer = new GameObject("Dimmer", typeof(RectTransform), typeof(Image));
        dimmer.transform.SetParent(root.transform, false);
        RectTransform dimmerRect = dimmer.GetComponent<RectTransform>();
        dimmerRect.anchorMin = Vector2.zero;
        dimmerRect.anchorMax = Vector2.one;
        dimmerRect.offsetMin = Vector2.zero;
        dimmerRect.offsetMax = Vector2.zero;
        dimmerRect.sizeDelta = Vector2.zero;
        dimmerRect.anchoredPosition = Vector2.zero;
        dimmer.GetComponent<Image>().color = new Color(0.055f, 0.012f, 0.028f, 1f);

        GameObject aura = new GameObject("Aura", typeof(RectTransform), typeof(Image));
        aura.transform.SetParent(root.transform, false);
        RectTransform auraRect = aura.GetComponent<RectTransform>();
        auraRect.anchorMin = new Vector2(0.5f, 0.5f);
        auraRect.anchorMax = new Vector2(0.5f, 0.5f);
        auraRect.sizeDelta = new Vector2(1800f, 1200f);
        auraRect.anchoredPosition = Vector2.zero;
        Image auraImage = aura.GetComponent<Image>();
        auraImage.color = new Color(0.35f, 0.06f, 0.10f, 0.10f);

        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(620f, 300f);
        panelRect.anchoredPosition = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.11f, 0.06f, 0.08f, 0.97f);

        GameObject panelBorder = new GameObject("PanelBorder", typeof(RectTransform), typeof(Image));
        panelBorder.transform.SetParent(panel.transform, false);
        RectTransform borderRect = panelBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(10f, 10f);
        borderRect.offsetMax = new Vector2(-10f, -10f);
        panelBorder.GetComponent<Image>().color = new Color(0.66f, 0.26f, 0.35f, 0.28f);

        GameObject topAccent = new GameObject("TopAccent", typeof(RectTransform), typeof(Image));
        topAccent.transform.SetParent(panel.transform, false);
        RectTransform topAccentRect = topAccent.GetComponent<RectTransform>();
        topAccentRect.anchorMin = new Vector2(0f, 1f);
        topAccentRect.anchorMax = new Vector2(1f, 1f);
        topAccentRect.pivot = new Vector2(0.5f, 1f);
        topAccentRect.sizeDelta = new Vector2(0f, 12f);
        topAccentRect.anchoredPosition = new Vector2(0f, -18f);
        topAccent.GetComponent<Image>().color = new Color(0.78f, 0.30f, 0.38f, 0.9f);

        GameObject spinner = new GameObject("SpinnerRoot", typeof(RectTransform));
        spinner.transform.SetParent(panel.transform, false);
        RectTransform spinnerRect = spinner.GetComponent<RectTransform>();
        spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
        spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
        spinnerRect.pivot = new Vector2(0.5f, 0.5f);
        spinnerRect.sizeDelta = new Vector2(100f, 100f);
        spinnerRect.anchoredPosition = new Vector2(0f, 78f);

        GameObject spinnerRing = new GameObject("SpinnerRing", typeof(RectTransform), typeof(Image));
        spinnerRing.transform.SetParent(spinner.transform, false);
        RectTransform ringRect = spinnerRing.GetComponent<RectTransform>();
        ringRect.anchorMin = new Vector2(0.5f, 0.5f);
        ringRect.anchorMax = new Vector2(0.5f, 0.5f);
        ringRect.sizeDelta = new Vector2(100f, 100f);
        ringRect.anchoredPosition = Vector2.zero;
        spinnerRing.GetComponent<Image>().color = new Color(0.45f, 0.17f, 0.23f, 0.6f);

        GameObject spinnerCore = new GameObject("SpinnerCore", typeof(RectTransform), typeof(Image));
        spinnerCore.transform.SetParent(spinner.transform, false);
        RectTransform coreRect = spinnerCore.GetComponent<RectTransform>();
        coreRect.anchorMin = new Vector2(0.5f, 0.5f);
        coreRect.anchorMax = new Vector2(0.5f, 0.5f);
        coreRect.sizeDelta = new Vector2(28f, 28f);
        coreRect.anchoredPosition = Vector2.zero;
        spinnerCore.GetComponent<Image>().color = new Color(0.95f, 0.79f, 0.70f, 1f);

        GameObject needle = new GameObject("SpinnerNeedle", typeof(RectTransform), typeof(Image));
        needle.transform.SetParent(spinner.transform, false);
        RectTransform needleRect = needle.GetComponent<RectTransform>();
        needleRect.anchorMin = new Vector2(0.5f, 0.5f);
        needleRect.anchorMax = new Vector2(0.5f, 0.5f);
        needleRect.sizeDelta = new Vector2(10f, 40f);
        needleRect.anchoredPosition = new Vector2(0f, 20f);
        needleRect.pivot = new Vector2(0.5f, 0.08f);
        needle.GetComponent<Image>().color = new Color(0.92f, 0.43f, 0.41f, 1f);

        GameObject title = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        title.transform.SetParent(panel.transform, false);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(520f, 74f);
        titleRect.anchoredPosition = new Vector2(0f, 6f);

        TextMeshProUGUI text = title.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 42f;
        text.color = new Color(0.95f, 0.86f, 0.76f, 1f);
        if (fontAsset != null)
            text.font = fontAsset;
        text.text = "Refreshing player stats";

        GameObject subtitle = new GameObject("HintText", typeof(RectTransform), typeof(TextMeshProUGUI));
        subtitle.transform.SetParent(panel.transform, false);
        RectTransform subtitleRect = subtitle.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.pivot = new Vector2(0.5f, 0.5f);
        subtitleRect.sizeDelta = new Vector2(520f, 56f);
        subtitleRect.anchoredPosition = new Vector2(0f, -52f);

        TextMeshProUGUI hint = subtitle.GetComponent<TextMeshProUGUI>();
        hint.alignment = TextAlignmentOptions.Center;
        hint.fontSize = 22f;
        hint.color = new Color(0.84f, 0.74f, 0.70f, 1f);
        if (fontAsset != null)
            hint.font = fontAsset;
        hint.text = "Calibrating fresh charts, metrics, and profile insight";

        GameObject progressTrackObject = new GameObject("ProgressTrack", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
        progressTrackObject.transform.SetParent(panel.transform, false);
        RectTransform progressTrackRect = progressTrackObject.GetComponent<RectTransform>();
        progressTrackRect.anchorMin = new Vector2(0.5f, 0.5f);
        progressTrackRect.anchorMax = new Vector2(0.5f, 0.5f);
        progressTrackRect.pivot = new Vector2(0.5f, 0.5f);
        progressTrackRect.sizeDelta = new Vector2(460f, 12f);
        progressTrackRect.anchoredPosition = new Vector2(0f, -105f);
        progressTrackObject.GetComponent<Image>().color = new Color(0.23f, 0.10f, 0.13f, 1f);

        GameObject progressFillObject = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
        progressFillObject.transform.SetParent(progressTrackObject.transform, false);
        RectTransform progressFillRect = progressFillObject.GetComponent<RectTransform>();
        progressFillRect.anchorMin = new Vector2(0f, 0f);
        progressFillRect.anchorMax = new Vector2(0f, 1f);
        progressFillRect.pivot = new Vector2(0f, 0.5f);
        progressFillRect.anchoredPosition = Vector2.zero;
        progressFillRect.sizeDelta = new Vector2(260f, 0f);
        Image progressFillImage = progressFillObject.GetComponent<Image>();
        progressFillImage.color = new Color(0.83f, 0.33f, 0.39f, 1f);

        GameObject glow = new GameObject("GlowBar", typeof(RectTransform), typeof(Image));
        glow.transform.SetParent(progressTrackObject.transform, false);
        RectTransform glowRect = glow.GetComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0f, 0f);
        glowRect.anchorMax = new Vector2(0f, 1f);
        glowRect.pivot = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(24f, 0f);
        glowRect.anchoredPosition = Vector2.zero;
        Image glowImage = glow.GetComponent<Image>();
        glowImage.color = new Color(1f, 0.88f, 0.80f, 0.75f);

        RuntimeLoadingOverlay overlay = root.GetComponent<RuntimeLoadingOverlay>();
        overlay.statusText = text;
        overlay.hintText = hint;
        overlay.spinnerRoot = spinnerRect;
        overlay.spinnerNeedle = needleRect;
        overlay.progressTrack = progressTrackRect;
        overlay.progressFill = progressFillImage;
        overlay.glowBar = glowImage;
        overlay.baseMessage = "Refreshing player stats";
        overlay.currentProgress = 0f;
        overlay.targetProgress = 0f;
        overlay.UpdateLabel(force: true);
        overlay.ApplyProgressVisuals();

        DontDestroyOnLoad(root);
        root.SetActive(false);
        return overlay;
    }

    public void Show(string message = null)
    {
        if (!string.IsNullOrWhiteSpace(message))
            baseMessage = message;

        dotCount = 0;
        timer = 0f;
        animationTime = 0f;
        currentProgress = 0f;
        targetProgress = 0f;
        UpdateLabel(force: true);
        ApplyProgressVisuals();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetProgress(float progress, string message = null, string hint = null)
    {
        targetProgress = Mathf.Clamp01(progress);

        if (!string.IsNullOrWhiteSpace(message))
            baseMessage = message;

        if (hintText != null && hint != null)
            hintText.text = hint;

        UpdateLabel(force: true);
    }

    public float CurrentProgress => currentProgress;

    private void Update()
    {
        if (!gameObject.activeSelf || statusText == null)
            return;

        timer += Time.unscaledDeltaTime;
        animationTime += Time.unscaledDeltaTime;
        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, progressSmoothingSpeed * Time.unscaledDeltaTime);

        if (timer < dotIntervalSeconds)
        {
            AnimateVisuals();
            return;
        }

        timer = 0f;
        dotCount = (dotCount + 1) % 4;
        UpdateLabel(force: false);
        AnimateVisuals();
    }

    private void UpdateLabel(bool force)
    {
        if (statusText == null)
            return;

        string dots = new string('.', dotCount);
        statusText.text = force || dotCount == 0
            ? baseMessage
            : baseMessage + dots;
    }

    private void AnimateVisuals()
    {
        if (spinnerRoot != null)
            spinnerRoot.Rotate(0f, 0f, -spinnerSpeed * Time.unscaledDeltaTime);

        if (spinnerNeedle != null)
        {
            float needleAngle = Mathf.Sin(animationTime * 2.2f) * 18f;
            spinnerNeedle.localRotation = Quaternion.Euler(0f, 0f, needleAngle);
        }

        ApplyProgressVisuals();

        if (glowBar != null && progressFill != null)
        {
            Color glowColor = glowBar.color;
            glowColor.a = Mathf.Lerp(0.35f, 0.9f, 0.5f + 0.5f * Mathf.Sin(animationTime * 2.4f));
            glowBar.color = glowColor;
        }

        if (hintText != null)
        {
            Color hintColor = hintText.color;
            hintColor.a = Mathf.Lerp(0.72f, 1f, 0.5f + 0.5f * Mathf.Sin(animationTime * 1.7f));
            hintText.color = hintColor;
        }
    }

    private void ApplyProgressVisuals()
    {
        if (progressFill == null)
            return;

        float trackWidth = progressTrack != null ? progressTrack.rect.width : 0f;
        if (trackWidth <= 0f && progressTrack != null)
            trackWidth = progressTrack.sizeDelta.x;

        RectTransform fillRect = progressFill.rectTransform;
        float fillWidth = trackWidth * currentProgress;
        fillRect.sizeDelta = new Vector2(fillWidth, fillRect.sizeDelta.y);

        if (glowBar == null)
            return;

        RectTransform glowRect = glowBar.rectTransform;
        float clampedX = Mathf.Clamp(fillWidth, 0f, Mathf.Max(0f, trackWidth));
        glowRect.anchoredPosition = new Vector2(clampedX, 0f);
        glowRect.gameObject.SetActive(fillWidth > 0.001f);
    }
}
