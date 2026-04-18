using UnityEngine;

public class DarknessSpreadController : MonoBehaviour
{
    private static Sprite cachedSquareSprite;
    [Header("References")]
    public Transform player;
    public PlayerController playerController;
    public LightLantern lantern;
    public Transform respawnPoint;
    public SpriteRenderer darknessVisual;

    [Header("Darkness Spread")]
    public float startFrontX = -10f;
    public float levelLeftX = -30f;
    public float baseSpreadSpeed = 1.6f;
    public float lanternSlowMultiplier = 0.35f;
    public float catchBuffer = 0.35f;

    [Header("Darkness Visual")]
    public float darknessHeight = 20f;
    public float darknessDepth = 0f;
    public Color fastDarknessColor = new Color(0.05f, 0.03f, 0.1f, 0.78f);
    public Color slowedDarknessColor = new Color(0.12f, 0.1f, 0.2f, 0.65f);
    public float colorLerpSpeed = 5f;
    public float pulseSpeed = 2.5f;
    public float pulseAmount = 0.08f;

    [Header("Death Handling")]
    public bool resetDarknessOnCatch = true;
    public float catchResetDelay = 0.15f;

    private float darknessFrontX;
    private float catchCooldown;
    private Vector3 playerStartPosition;

    void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.Find("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        if (playerController == null && player != null)
            playerController = player.GetComponent<PlayerController>();

        if (lantern == null && player != null)
            lantern = player.GetComponent<LightLantern>();

        if (respawnPoint == null && player != null)
            playerStartPosition = player.position;
        else if (respawnPoint != null)
            playerStartPosition = respawnPoint.position;

        darknessFrontX = startFrontX;
        EnsureDarknessVisual();
    }

    void Start()
    {
        if (!Application.isPlaying)
            return;
        if (player == null)
            return;

        float marginBehindPlayer = 22f;
        if (player.position.x <= darknessFrontX + catchBuffer)
        {
            darknessFrontX = player.position.x - marginBehindPlayer;
            startFrontX = darknessFrontX;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        if (catchCooldown > 0f)
            catchCooldown -= Time.deltaTime;

        bool lanternOn = lantern != null && lantern.IsOn;
        float speedMultiplier = lanternOn ? lanternSlowMultiplier : 1f;
        darknessFrontX += baseSpreadSpeed * speedMultiplier * Time.deltaTime;

        UpdateDarknessVisual(lanternOn);

        if (catchCooldown <= 0f && player.position.x <= darknessFrontX + catchBuffer)
            HandlePlayerCaught();
    }

    void HandlePlayerCaught()
    {
        catchCooldown = catchResetDelay;

        if (playerController != null)
        {
            PlayerHealth health = playerController.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.Kill("The darkness caught you.");
                return;
            }
            else
                playerController.Die();
        }

        player.position = respawnPoint != null ? respawnPoint.position : playerStartPosition;

        if (resetDarknessOnCatch)
            darknessFrontX = startFrontX;
    }

    void EnsureDarknessVisual()
    {
        if (darknessVisual != null)
            return;

        GameObject visualObject = new GameObject("DarknessOverlay");
        visualObject.transform.SetParent(transform);
        visualObject.transform.localRotation = Quaternion.identity;

        darknessVisual = visualObject.AddComponent<SpriteRenderer>();
        darknessVisual.sortingOrder = -10;

        darknessVisual.sprite = GetOrCreateSquareSprite();
        darknessVisual.color = fastDarknessColor;
    }

    Sprite GetOrCreateSquareSprite()
    {
        if (cachedSquareSprite != null)
            return cachedSquareSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.name = "DarknessSquare";
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        cachedSquareSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return cachedSquareSprite;
    }

    void UpdateDarknessVisual(bool lanternOn)
    {
        if (darknessVisual == null)
            return;

        float width = Mathf.Max(0.1f, darknessFrontX - levelLeftX);
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        Transform visualTransform = darknessVisual.transform;
        visualTransform.position = new Vector3(levelLeftX + width * 0.5f, 0f, darknessDepth);
        visualTransform.localScale = new Vector3(width * pulse, darknessHeight, 1f);

        Color target = lanternOn ? slowedDarknessColor : fastDarknessColor;
        darknessVisual.color = Color.Lerp(darknessVisual.color, target, Time.deltaTime * colorLerpSpeed);
    }
}
