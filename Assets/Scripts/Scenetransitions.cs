using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles fade-based scene transitions for Echoes of the Last Light.
/// Attach to a persistent GameObject alongside a Canvas ? Image (full-screen black).
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Fade Settings")]
    public float fadeDuration = 1.2f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Assign a full-screen black UI Image in the Inspector.
    [Header("References")]
    public Image fadeOverlay;

    private bool isTransitioning;

    void Awake()
    {
        // Singleton — only one instance survives scene loads.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start fully transparent.
        if (fadeOverlay != null)
            fadeOverlay.color = new Color(0f, 0f, 0f, 0f);
    }

    /// <summary>
    /// Call this to transition to any scene by name.
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    /// <summary>
    /// Convenience overload using build index.
    /// </summary>
    public void TransitionToScene(int buildIndex)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeAndLoad(buildIndex));
    }

    IEnumerator FadeAndLoad(string sceneName)
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(0f, 1f));             // Fade to black.

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitUntil(() => load.isDone);         // Wait for load.

        yield return StartCoroutine(Fade(1f, 0f));             // Fade back in.
        isTransitioning = false;
    }

    IEnumerator FadeAndLoad(int buildIndex)
    {
        isTransitioning = true;
        yield return StartCoroutine(Fade(0f, 1f));

        AsyncOperation load = SceneManager.LoadSceneAsync(buildIndex);
        yield return new WaitUntil(() => load.isDone);

        yield return StartCoroutine(Fade(1f, 0f));
        isTransitioning = false;
    }

    IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        if (fadeOverlay == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeCurve.Evaluate(elapsed / fadeDuration);
            fadeOverlay.color = new Color(0f, 0f, 0f, Mathf.Lerp(fromAlpha, toAlpha, t));
            yield return null;
        }

        fadeOverlay.color = new Color(0f, 0f, 0f, toAlpha);
    }
}