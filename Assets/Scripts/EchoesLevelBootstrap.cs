using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(-200)]
public class EchoesLevelBootstrap : MonoBehaviour
{
    [Header("Ground")]
    [Tooltip("Total span of floor pieces along X (world units).")]
    public float groundTotalWidth = 110f;

    [Tooltip("Hole width for the pit (no collider). Kept away from the player spawn by pitCenterX.")]
    public float pitWidth = 9f;

    [Tooltip("World X center of the pit gap. Keep positive so x=0 stays on solid Ground_Left.")]
    public float pitCenterX = 36f;

    public float groundY = -4f;

    [Header("Extra content (no scaling of existing scene objects)")]
    public int extraPlatformColumns = 28;
    public float extraPlatformSpacing = 6.5f;
    public float extraPlatformMinY = -1.5f;
    public float extraPlatformMaxY = 30f;

    [Tooltip("Additional vertical wall pillars (same style as Wall_Left).")]
    public int extraWallCount = 10;

    [Tooltip("Do not spawn an auto-wall closer than this to an existing wall at x = ±11.")]
    public float avoidDefaultWallRadius = 5f;

    [Header("Grapple points (procedural)")]
    [Tooltip("Vertical bands of grapple anchors.")]
    public int grappleRows = 5;

    [Tooltip("How many grapple points per row across the level width.")]
    public int grapplePointsPerRow = 5;

    public float grappleYMin = 0.5f;
    public float grappleYMax = 36f;

    [Tooltip("Extra grapple points on top of the grid (keep low; grid alone is usually enough).")]
    public int grappleScatterCount = 0;

    [Header("Player start (left-to-right run)")]
    [Tooltip("Place the player near the left edge of the map when the level is built.")]
    public bool movePlayerToFarLeftOnBuild = true;

    [Tooltip("World X offset from the left edge of the floor (inside Ground_Left).")]
    public float spawnInsetFromLeftEdge = 4f;

    [Tooltip("Player transform Y so feet sit on the main floor (ground center Y + this).")]
    public float playerSpawnYOffsetFromGround = 1.12f;

    [Header("Editor")]
    [Tooltip("When on, the level is generated in Edit Mode so you can see it in the Scene view. Uses the original Ground sprite once, then the cached sprite below if Ground is already gone.")]
    public bool buildLayoutInEditMode = true;

    [Tooltip("Auto-filled from Ground when the layout runs; needed to rebuild after Ground was replaced.")]
    [SerializeField] Sprite cachedGroundSpriteForRebuild;

    void Awake()
    {
        if (!gameObject.scene.IsValid())
            return;

        TryBuildLayout();
    }

    public void TryBuildLayout()
    {
        if (GameObject.Find("Ground_Left") != null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying && !buildLayoutInEditMode)
            return;
#endif

        BuildLayoutCore();
    }

#if UNITY_EDITOR
    public void EditorClearProceduralObjectsAndRebuild()
    {
        if (Application.isPlaying)
            return;

        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            string n = root.name;
            if (n == "Ground_Left" || n == "Ground_Right" || n == "PitKillZone" || n == "Respawn_Pit"
                || n.StartsWith("Platform_Auto_") || n.StartsWith("Wall_Auto_")
                || n.StartsWith("GrapplePoint_Auto_"))
                UnityEditor.Undo.DestroyObjectImmediate(root);
        }

        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            SpriteRenderer sr = ground.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                cachedGroundSpriteForRebuild = sr.sprite;
        }

        if (!buildLayoutInEditMode)
            buildLayoutInEditMode = true;

        BuildLayoutCore();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEngine.SceneManagement.Scene scene = gameObject.scene;
        if (scene.IsValid())
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
    }
#endif

    void BuildLayoutCore()
    {
        Vector3 playerStart = Vector3.zero;
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
            playerStart = playerObject.transform.position;

        Sprite groundSprite = null;
        GameObject groundObject = GameObject.Find("Ground");
        if (groundObject != null)
        {
            SpriteRenderer groundRenderer = groundObject.GetComponent<SpriteRenderer>();
            if (groundRenderer != null)
                groundSprite = groundRenderer.sprite;
        }

        if (groundSprite == null)
            groundSprite = cachedGroundSpriteForRebuild;
        if (groundSprite == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("EchoesLevelBootstrap: Missing ground sprite. Add a GameObject named Ground with a SpriteRenderer, or assign Cached Ground Sprite For Rebuild on LevelBootstrap.");
#endif
            return;
        }

        if (groundObject != null)
        {
            cachedGroundSpriteForRebuild = groundSprite;
            DestroyOrImmediate(groundObject);
        }

        float half = groundTotalWidth * 0.5f;
        float pitHalf = pitWidth * 0.5f;
        float pitMin = pitCenterX - pitHalf;
        float pitMax = pitCenterX + pitHalf;

        float worldLeft = -half;
        float worldRight = half;

        float leftWidth = pitMin - worldLeft;
        if (leftWidth > 0.4f)
        {
            float leftCenterX = worldLeft + leftWidth * 0.5f;
            CreateGroundChunk("Ground_Left", groundSprite, new Vector3(leftCenterX, groundY, 0f), new Vector3(leftWidth, 1f, 1f), 6);
        }

        float rightWidth = worldRight - pitMax;
        if (rightWidth > 0.4f)
        {
            float rightCenterX = pitMax + rightWidth * 0.5f;
            CreateGroundChunk("Ground_Right", groundSprite, new Vector3(rightCenterX, groundY, 0f), new Vector3(rightWidth, 1f, 1f), 6);
        }

        if (movePlayerToFarLeftOnBuild && playerObject != null)
        {
            float spawnX = worldLeft + spawnInsetFromLeftEdge;
            float spawnY = groundY + playerSpawnYOffsetFromGround;
            playerObject.transform.position = new Vector3(spawnX, spawnY, 0f);
            playerStart = playerObject.transform.position;
        }

        CreatePitKillZone(playerStart, pitMin, pitMax);

        DarknessSpreadController darkness = Object.FindFirstObjectByType<DarknessSpreadController>();
        if (darkness != null)
            darkness.levelLeftX = Mathf.Min(darkness.levelLeftX, worldLeft - 30f);

        GameObject wallTemplate = GameObject.Find("Wall_Left");
        if (wallTemplate != null)
        {
            SpriteRenderer wallRenderer = wallTemplate.GetComponent<SpriteRenderer>();
            if (wallRenderer != null && wallRenderer.sprite != null)
                SpawnExtraWalls(wallRenderer.sprite, wallTemplate.layer, wallTemplate.transform, half);
        }

        SpawnExtraPlatforms(groundSprite, half, pitMin, pitMax);
        SpawnExtraGrapplePoints(half, pitMin, pitMax);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEngine.SceneManagement.Scene scene = gameObject.scene;
            if (scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }
#endif
    }

    void DestroyOrImmediate(GameObject target)
    {
        if (target == null)
            return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.Undo.DestroyObjectImmediate(target);
        else
#endif
            Destroy(target);
    }

    void CreateGroundChunk(string chunkName, Sprite sprite, Vector3 position, Vector3 scale, int layer)
    {
        GameObject chunk = new GameObject(chunkName);
        chunk.layer = layer;
        chunk.transform.position = position;
        chunk.transform.localScale = scale;

        SpriteRenderer renderer = chunk.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = new Color(0.2f, 0.2f, 0.22f, 1f);

        BoxCollider2D collider = chunk.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.Undo.RegisterCreatedObjectUndo(chunk, "Echoes Level");
#endif
    }

    void CreatePitKillZone(Vector3 playerRespawn, float pitMin, float pitMax)
    {
        float pitCenter = (pitMin + pitMax) * 0.5f;
        GameObject pit = new GameObject("PitKillZone");
        pit.layer = 0;
        pit.transform.position = new Vector3(pitCenter, -18f, 0f);

        BoxCollider2D trigger = pit.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(Mathf.Max(4f, pitWidth + 3f), 30f);

        FallDeathZone death = pit.AddComponent<FallDeathZone>();
        GameObject respawn = new GameObject("Respawn_Pit");
        respawn.transform.position = playerRespawn;
        death.respawnPoint = respawn.transform;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.RegisterCreatedObjectUndo(pit, "Echoes Level");
            UnityEditor.Undo.RegisterCreatedObjectUndo(respawn, "Echoes Level");
        }
#endif
    }

    void SpawnExtraWalls(Sprite wallSprite, int wallLayer, Transform template, float halfWidth)
    {
        float templateY = template.position.y;
        Vector3 templateScale = template.localScale;
        int count = Mathf.Max(0, extraWallCount);
        for (int i = 0; i < count; i++)
        {
            float t = count <= 1 ? 0.5f : i / (float)(count - 1);
            float x = Mathf.Lerp(-halfWidth + 3f, halfWidth - 3f, t);
            x += (i % 3 - 1) * 0.35f;

            if (Mathf.Abs(x - pitCenterX) < pitWidth * 0.5f + 2f)
                continue;
            if (Mathf.Abs(x + 11f) < avoidDefaultWallRadius || Mathf.Abs(x - 11f) < avoidDefaultWallRadius)
                continue;
            if (Mathf.Abs(x - 2f) < avoidDefaultWallRadius)
                continue;

            GameObject wall = new GameObject($"Wall_Auto_{i}");
            wall.layer = wallLayer;
            wall.transform.position = new Vector3(x, templateY, 0f);
            wall.transform.localScale = templateScale;

            SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = wallSprite;
            renderer.color = new Color(0.32f, 0.32f, 0.4f, 1f);

            BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.Undo.RegisterCreatedObjectUndo(wall, "Echoes Level");
#endif
        }
    }

    void SpawnExtraPlatforms(Sprite sprite, float halfWidth, float pitMin, float pitMax)
    {
        float pitAvoidHalf = (pitMax - pitMin) * 0.5f + 4f;
        int columns = Mathf.Max(0, extraPlatformColumns);
        for (int i = 0; i < columns; i++)
        {
            float t = columns <= 1 ? 0.5f : i / (float)(columns - 1);
            float x = Mathf.Lerp(-halfWidth + 8f, halfWidth - 8f, t);
            x += (i % 3 - 1) * (extraPlatformSpacing * 0.15f);
            if (Mathf.Abs(x - pitCenterX) < pitAvoidHalf)
                continue;

            float wave = Mathf.Sin(i * 1.7f) * 4f + Mathf.Cos(i * 0.9f) * 2.5f;
            float y = Mathf.Lerp(extraPlatformMinY, extraPlatformMaxY, (Mathf.Abs(wave) % 1f + 0.15f + (i % 5) * 0.12f) % 1f);
            y = Mathf.Clamp(y + wave * 0.35f, extraPlatformMinY, extraPlatformMaxY);

            float w = 1.8f + (i % 4) * 0.65f;
            CreateGroundChunk($"Platform_Auto_{i}", sprite, new Vector3(x, y, 0f), new Vector3(w, 0.45f, 1f), 6);
        }
    }

    void SpawnExtraGrapplePoints(float halfWidth, float pitMin, float pitMax)
    {
        GameObject templateObject = GameObject.Find("GrapplePoint_A");
        if (templateObject == null)
            templateObject = GameObject.Find("GrapplePoint_B");
        if (templateObject == null)
            templateObject = GameObject.Find("GrapplePoint_C");

        LightGrapplePoint templatePoint = templateObject != null ? templateObject.GetComponent<LightGrapplePoint>() : null;
        SpriteRenderer templateRenderer = templateObject != null ? templateObject.GetComponent<SpriteRenderer>() : null;
        if (templatePoint == null || templateRenderer == null || templateRenderer.sprite == null)
            return;

        float pitAvoidHalf = (pitMax - pitMin) * 0.5f + 4f;
        int rows = Mathf.Max(1, grappleRows);
        int cols = Mathf.Max(1, grapplePointsPerRow);
        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            float tRow = rows <= 1 ? 0.5f : (row + 0.5f) / rows;
            float y = Mathf.Lerp(grappleYMin, grappleYMax, tRow);
            y += Mathf.Sin(row * 2.17f) * 0.85f;

            for (int col = 0; col < cols; col++)
            {
                float tCol = cols <= 1 ? 0.5f : (col + 0.5f) / cols;
                float x = Mathf.Lerp(-halfWidth + 4f, halfWidth - 4f, tCol);
                x += Mathf.Sin(row * 1.1f + col * 1.9f) * 5f;
                x += (row % 3 - 1) * 1.1f;

                if (Mathf.Abs(x - pitCenterX) < pitAvoidHalf)
                    continue;

                CreateAutoGrapplePoint(templatePoint, templateRenderer, new Vector3(x, y, 0f), index++);
            }
        }

        int scatter = Mathf.Max(0, grappleScatterCount);
        for (int s = 0; s < scatter; s++)
        {
            float u = (s * 0.618034f) % 1f;
            float v = (s * 0.379651f) % 1f;
            float x = Mathf.Lerp(-halfWidth + 6f, halfWidth - 6f, u);
            float y = Mathf.Lerp(grappleYMin + 2f, grappleYMax - 1f, v);
            x += Mathf.Sin(s * 3.1f) * 6f;
            if (Mathf.Abs(x - pitCenterX) < pitAvoidHalf)
                continue;

            CreateAutoGrapplePoint(templatePoint, templateRenderer, new Vector3(x, y, 0f), index++);
        }
    }

    void CreateAutoGrapplePoint(LightGrapplePoint templatePoint, SpriteRenderer templateRenderer, Vector3 position, int index)
    {
        GameObject pointObject = new GameObject($"GrapplePoint_Auto_{index}");
        pointObject.layer = templateRenderer.gameObject.layer;
        pointObject.transform.position = position;
        pointObject.transform.localScale = templateRenderer.transform.lossyScale;

        SpriteRenderer indicator = pointObject.AddComponent<SpriteRenderer>();
        indicator.sprite = templateRenderer.sprite;
        indicator.color = templateRenderer.color;
        indicator.sortingLayerID = templateRenderer.sortingLayerID;
        indicator.sortingOrder = templateRenderer.sortingOrder;

        LightGrapplePoint grapple = pointObject.AddComponent<LightGrapplePoint>();
        grapple.indicator = indicator;
        grapple.startIlluminated = templatePoint.startIlluminated;
        grapple.activeColor = templatePoint.activeColor;
        grapple.inactiveColor = templatePoint.inactiveColor;
        grapple.SetIlluminated(templatePoint.startIlluminated);

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.Undo.RegisterCreatedObjectUndo(pointObject, "Echoes Level");
#endif
    }
}
