using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Ensures every <see cref="Button"/> in loaded scenes gets a satisfying click sound.
/// </summary>
public class EchoesUiSoundBootstrap : MonoBehaviour
{
    static EchoesUiSoundBootstrap instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        HookButtonsInScene(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HookButtonsInScene(scene);
    }

    /// <summary>Call after spawning UI at runtime (e.g. options sliders) so click hooks attach.</summary>
    public static void HookSceneNow(Scene scene)
    {
        HookButtonsInScene(scene);
    }

    static void HookButtonsInScene(Scene scene)
    {
        if (!scene.IsValid())
            return;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Button btn in root.GetComponentsInChildren<Button>(true))
            {
                if (btn == null)
                    continue;
                if (btn.GetComponent<EchoesUiClickHook>() != null)
                    continue;
                btn.gameObject.AddComponent<EchoesUiClickHook>();
            }

            foreach (Slider slider in root.GetComponentsInChildren<Slider>(true))
            {
                if (slider == null)
                    continue;
                if (slider.GetComponent<EchoesUiClickHook>() != null)
                    continue;
                slider.gameObject.AddComponent<EchoesUiClickHook>();
            }
        }
    }
}
