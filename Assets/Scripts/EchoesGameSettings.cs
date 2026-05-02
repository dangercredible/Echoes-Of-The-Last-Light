using System;
using UnityEngine;

/// <summary>
/// Persistent player preferences (volume + brightness). Drives audio/visual refresh hooks.
/// </summary>
public static class EchoesGameSettings
{
    const string KeyMaster = "Echoes_Settings_Master";
    const string KeyMusic = "Echoes_Settings_Music";
    const string KeySfx = "Echoes_Settings_Sfx";
    const string KeyBrightness = "Echoes_Settings_Brightness";

    static float masterVolume = 1f;
    static float musicVolume = 0.85f;
    static float sfxVolume = 0.8f;
    static float brightness = 0.55f;

    static bool loaded;

    public static float MasterVolume
    {
        get => masterVolume;
        set => Set(ref masterVolume, Mathf.Clamp01(value));
    }

    public static float MusicVolume
    {
        get => musicVolume;
        set => Set(ref musicVolume, Mathf.Clamp01(value));
    }

    public static float SfxVolume
    {
        get => sfxVolume;
        set => Set(ref sfxVolume, Mathf.Clamp01(value));
    }

    /// <summary>0 = darker, 1 = brighter (design baseline is committed per scene).</summary>
    public static float Brightness
    {
        get => brightness;
        set => Set(ref brightness, Mathf.Clamp01(value));
    }

    public static event Action SettingsChanged;

    public static void Load()
    {
        masterVolume = PlayerPrefs.GetFloat(KeyMaster, 1f);
        musicVolume = PlayerPrefs.GetFloat(KeyMusic, 0.85f);
        sfxVolume = PlayerPrefs.GetFloat(KeySfx, 0.8f);
        brightness = PlayerPrefs.GetFloat(KeyBrightness, 0.55f);
        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        brightness = Mathf.Clamp01(brightness);
        loaded = true;
        Raise();
    }

    public static void EnsureLoaded()
    {
        if (!loaded)
            Load();
    }

    static void Set(ref float field, float value)
    {
        if (Mathf.Approximately(field, value))
            return;
        field = value;
        Persist();
        Raise();
    }

    static void Persist()
    {
        PlayerPrefs.SetFloat(KeyMaster, masterVolume);
        PlayerPrefs.SetFloat(KeyMusic, musicVolume);
        PlayerPrefs.SetFloat(KeySfx, sfxVolume);
        PlayerPrefs.SetFloat(KeyBrightness, brightness);
    }

    /// <summary>Writes prefs to disk — call when leaving options or pausing.</summary>
    public static void Flush()
    {
        PlayerPrefs.Save();
    }

    static void Raise()
    {
        SettingsChanged?.Invoke();
    }

    public static void ResetToDefaults()
    {
        masterVolume = 1f;
        musicVolume = 0.85f;
        sfxVolume = 0.8f;
        brightness = 0.55f;
        Persist();
        Raise();
        Flush();
    }
}
