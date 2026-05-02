using UnityEngine;

/// <summary>
/// Stores per-scene baseline colors from <see cref="EchoesSceneAtmosphere"/> so brightness can scale around them.
/// </summary>
public static class EchoesVisualBaseline
{
    static bool hasBaseline;
    static Color cameraClear = new Color(0.035f, 0.038f, 0.072f, 1f);
    static Color ambient = new Color(0.065f, 0.07f, 0.11f, 1f);

    public static bool HasBaseline => hasBaseline;

    public static void Commit(Color cameraClearBaseline, Color ambientBaseline)
    {
        cameraClear = cameraClearBaseline;
        ambient = ambientBaseline;
        hasBaseline = true;
        EchoesBrightness.ApplyFromSettings();
    }

    public static void ApplyBrightness(float brightness01)
    {
        Color camBase = hasBaseline ? cameraClear : new Color(0.045f, 0.052f, 0.092f, 1f);
        Color ambBase = hasBaseline ? ambient : new Color(0.072f, 0.078f, 0.12f, 1f);

        Camera cam = Camera.main;
        if (cam != null)
        {
            float dim = Mathf.Lerp(0.22f, 1f, brightness01);
            float lift = Mathf.Lerp(0f, 0.14f, brightness01);
            Color c = camBase * dim + new Color(lift, lift, lift * 1.05f, 0f);
            c.r = Mathf.Clamp01(c.r);
            c.g = Mathf.Clamp01(c.g);
            c.b = Mathf.Clamp01(c.b);
            c.a = 1f;
            cam.backgroundColor = c;
        }

        float ambMul = Mathf.Lerp(0.3f, 1.28f, brightness01);
        Color a = ambBase * ambMul;
        a.r = Mathf.Clamp01(a.r);
        a.g = Mathf.Clamp01(a.g);
        a.b = Mathf.Clamp01(a.b);
        RenderSettings.ambientLight = a;
    }
}

/// <summary>
/// Applies brightness from <see cref="EchoesGameSettings"/> after baseline exists.
/// </summary>
public static class EchoesBrightness
{
    static bool subscribed;

    public static void ApplyFromSettings()
    {
        EchoesGameSettings.EnsureLoaded();
        EchoesVisualBaseline.ApplyBrightness(EchoesGameSettings.Brightness);
    }

    public static void Subscribe()
    {
        if (subscribed)
            return;
        EchoesGameSettings.SettingsChanged += ApplyFromSettings;
        subscribed = true;
    }

    public static void Unsubscribe()
    {
        if (!subscribed)
            return;
        EchoesGameSettings.SettingsChanged -= ApplyFromSettings;
        subscribed = false;
    }
}
