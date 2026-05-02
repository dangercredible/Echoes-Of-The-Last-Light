using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent audio: layered music bus + SFX bus with master/music/SFX levels from <see cref="EchoesGameSettings"/>.
/// Procedural fallbacks are gain-staged for headroom (no clipping).
/// </summary>
public class EchoesAudioDirector : MonoBehaviour
{
    public static EchoesAudioDirector Instance { get; private set; }

    const float MenuMusicLevelMul = 1f;
    const float SfxHeadroomMul = 0.92f;

    [Header("Optional custom clips (override procedural)")]
    [SerializeField] AudioClip ambientMusicClip;
    [SerializeField] AudioClip uiClickClip;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip grappleClip;

    [Header("Mix staging (inspector defaults; player prefs override perceived level)")]
    [SerializeField] [Range(0f, 1f)] float musicBusDesignMax = 0.52f;
    [SerializeField] [Range(0f, 1f)] float sfxBusDesignMax = 0.52f;

    AudioSource musicSource;
    AudioSource sfxSource;

    static AudioClip cachedGameplayAmbient;
    static AudioClip cachedMenuAmbient;
    const int MenuAmbientRevision = 2;
    static int cachedMenuAmbientRevision = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EchoesBrightness.Subscribe();
        EchoesGameSettings.Load();
        EchoesBrightness.ApplyFromSettings();

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.priority = 64;
        musicSource.reverbZoneMix = 0f;
        musicSource.dopplerLevel = 0f;
        musicSource.panStereo = 0f;
        musicSource.mute = false;
        musicSource.ignoreListenerPause = true;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.priority = 64;
        sfxSource.reverbZoneMix = 0f;
        sfxSource.dopplerLevel = 0f;
        sfxSource.ignoreListenerPause = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyMusicClipForActiveScene();
        musicSource.Play();

        RefreshVolumes();

        gameObject.AddComponent<EchoesUiSoundBootstrap>();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        EchoesGameSettings.SettingsChanged -= RefreshVolumes;
        EchoesBrightness.Unsubscribe();

        if (Instance == this)
            Instance = null;
    }

    void OnEnable()
    {
        EchoesGameSettings.SettingsChanged += RefreshVolumes;
        EchoesGameSettings.EnsureLoaded();
        RefreshVolumes();
        EchoesBrightness.ApplyFromSettings();
    }

    void OnDisable()
    {
        EchoesGameSettings.SettingsChanged -= RefreshVolumes;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicClipForActiveScene();
        RefreshVolumes();
        EchoesBrightness.ApplyFromSettings();
    }

    bool IsMenuScene()
    {
        string n = SceneManager.GetActiveScene().name;
        return n == MenuManager.OptionsSceneName || n == MenuManager.MainMenuSceneName;
    }

    void ApplyMusicClipForActiveScene()
    {
        if (ambientMusicClip != null)
        {
            if (musicSource.clip != ambientMusicClip)
            {
                musicSource.clip = ambientMusicClip;
                musicSource.time = 0f;
                musicSource.Play();
            }

            return;
        }

        AudioClip want = IsMenuScene() ? GetMenuAmbient() : GetGameplayAmbient();
        if (musicSource.clip != want)
        {
            musicSource.clip = want;
            musicSource.time = 0f;
            musicSource.Play();
        }
    }

    public static void EnsureExists()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject("EchoesAudioDirector");
        go.AddComponent<EchoesAudioDirector>();
    }

    /// <summary>
    /// Main menu / options must always hear the ambient bed — call from menu Start after Awake ordering settles.
    /// </summary>
    public static void EnsureMenuMusicAudible()
    {
        EnsureExists();
        if (Instance == null || Instance.musicSource == null)
            return;

        Instance.musicSource.mute = false;
        Instance.musicSource.ignoreListenerPause = true;
        Instance.ApplyMusicClipForActiveScene();
        if (Instance.musicSource.clip != null && !Instance.musicSource.isPlaying)
            Instance.musicSource.Play();

        RefreshVolumes();
    }

    public static void RefreshVolumes()
    {
        if (Instance == null)
            return;

        Instance.ApplyVolumesInternal();
    }

    void ApplyVolumesInternal()
    {
        EchoesGameSettings.EnsureLoaded();

        float master = EchoesGameSettings.MasterVolume;
        float music = EchoesGameSettings.MusicVolume;
        float sfx = EchoesGameSettings.SfxVolume;

        float menuMul = IsMenuScene() ? MenuMusicLevelMul : 1f;
        float musicLinear = master * music * musicBusDesignMax * menuMul;
        musicSource.volume = Mathf.Clamp01(musicLinear);

        sfxSource.volume = Mathf.Clamp01(master * sfx * sfxBusDesignMax);
    }

    public static void PlayUiClick()
    {
        EchoesGameSettings.EnsureLoaded();
        if (Instance == null)
            return;
        AudioClip c = Instance.uiClickClip != null ? Instance.uiClickClip : BuildUiClickClip();
        float gain = Mathf.Clamp01(EchoesGameSettings.MasterVolume * EchoesGameSettings.SfxVolume * SfxHeadroomMul * 0.55f);
        Instance.sfxSource.PlayOneShot(c, gain);
    }

    public static void PlayJump()
    {
        EchoesGameSettings.EnsureLoaded();
        if (Instance == null)
            return;
        AudioClip c = Instance.jumpClip != null ? Instance.jumpClip : BuildJumpClip();
        float gain = Mathf.Clamp01(EchoesGameSettings.MasterVolume * EchoesGameSettings.SfxVolume * SfxHeadroomMul * 0.42f);
        Instance.sfxSource.PlayOneShot(c, gain);
    }

    public static void PlayGrappleAttach()
    {
        EchoesGameSettings.EnsureLoaded();
        if (Instance == null)
            return;
        AudioClip c = Instance.grappleClip != null ? Instance.grappleClip : BuildGrappleClip();
        float gain = Mathf.Clamp01(EchoesGameSettings.MasterVolume * EchoesGameSettings.SfxVolume * SfxHeadroomMul * 0.44f);
        Instance.sfxSource.PlayOneShot(c, gain);
    }

    static AudioClip GetGameplayAmbient()
    {
        if (cachedGameplayAmbient != null)
            return cachedGameplayAmbient;
        cachedGameplayAmbient = BuildGameplayAmbientClip();
        return cachedGameplayAmbient;
    }

    static AudioClip GetMenuAmbient()
    {
        if (cachedMenuAmbient != null && cachedMenuAmbientRevision == MenuAmbientRevision)
            return cachedMenuAmbient;
        cachedMenuAmbient = BuildMainMenuAmbientClip();
        cachedMenuAmbientRevision = MenuAmbientRevision;
        return cachedMenuAmbient;
    }

    static AudioClip BuildUiClickClip()
    {
        const int sampleRate = 44100;
        int samples = sampleRate / 38;
        AudioClip clip = AudioClip.Create("Echoes_UI_Click", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        const float f = 2400f;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            float env = (1f - t) * (1f - t);
            data[i] = Mathf.Sin(2f * Mathf.PI * f * i / sampleRate) * env * 0.26f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip BuildJumpClip()
    {
        const int sampleRate = 44100;
        int samples = sampleRate / 15;
        AudioClip clip = AudioClip.Create("Echoes_Jump", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        const float startF = 360f;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            float f = Mathf.Lerp(startF, 110f, t);
            float env = Mathf.Sin(Mathf.PI * t);
            data[i] = Mathf.Sin(2f * Mathf.PI * f * i / sampleRate) * env * 0.28f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip BuildGrappleClip()
    {
        const int sampleRate = 44100;
        int samples = sampleRate / 11;
        AudioClip clip = AudioClip.Create("Echoes_Grapple", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            float f = Mathf.Lerp(480f, 165f, t);
            float env = (1f - t);
            data[i] = Mathf.Sin(2f * Mathf.PI * f * i / sampleRate) * env * env * 0.26f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip BuildGameplayAmbientClip()
    {
        const int sampleRate = 44100;
        int samples = sampleRate * 28;
        AudioClip clip = AudioClip.Create("Echoes_Ambient_Gameplay", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        const float root = 55f;
        const float fifth = root * 1.5f;
        const float octave = root * 2f;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float lfo = Mathf.Sin(t * 0.14f) * 0.32f + Mathf.Sin(t * 0.028f) * 0.18f;
            float wobble = Mathf.Sin(t * (5f + lfo * 3f)) * 0.012f;
            float s =
                Mathf.Sin(2f * Mathf.PI * root * t * (1f + wobble)) * 0.045f
                + Mathf.Sin(2f * Mathf.PI * fifth * t * 0.997f) * 0.026f
                + Mathf.Sin(2f * Mathf.PI * octave * t * 1.003f + lfo) * 0.017f;
            float noise = (Mathf.PerlinNoise(i * 0.0019f, t * 0.35f) - 0.5f) * 0.015f;
            data[i] = Mathf.Clamp(s + noise, -0.85f, 0.85f);
        }

        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Slower, softer pad for main menu / options — still eerie, less busy.</summary>
    static AudioClip BuildMainMenuAmbientClip()
    {
        const int sampleRate = 44100;
        int samples = sampleRate * 32;
        AudioClip clip = AudioClip.Create("Echoes_Ambient_Menu_R2", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        const float root = 49f;
        const float fifth = root * 1.5f;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            float lfo = Mathf.Sin(t * 0.09f) * 0.28f + Mathf.Sin(t * 0.019f) * 0.14f;
            float s =
                Mathf.Sin(2f * Mathf.PI * root * t * (1f + lfo * 0.004f)) * 0.055f
                + Mathf.Sin(2f * Mathf.PI * fifth * t * 0.998f + lfo * 0.5f) * 0.034f;
            float noise = (Mathf.PerlinNoise(i * 0.0014f, t * 0.22f) - 0.5f) * 0.016f;
            data[i] = Mathf.Clamp(s + noise, -0.82f, 0.82f);
        }

        clip.SetData(data, 0);
        return clip;
    }
}
