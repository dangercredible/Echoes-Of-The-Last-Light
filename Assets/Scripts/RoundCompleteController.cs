using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// End-of-level transition to the next scene (no blocking overlay).
/// </summary>
public class RoundCompleteController : MonoBehaviour
{
    public static RoundCompleteController Instance { get; private set; }
    public static bool IsShowing { get; private set; }

    [Tooltip("If empty, Overgrowth loads Shattered Docks; other levels return to Main Menu.")]
    [SerializeField] string nextSceneAfterRound = "";

    bool shown;

    public static RoundCompleteController InstanceOrFind()
    {
        if (Instance != null)
            return Instance;
        Instance = FindFirstObjectByType<RoundCompleteController>();
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        IsShowing = false;

        if (string.IsNullOrEmpty(nextSceneAfterRound))
            nextSceneAfterRound = ResolveDefaultNextScene();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsShowing = false;
        }
    }

    static string ResolveDefaultNextScene()
    {
        string active = SceneManager.GetActiveScene().name;
        if (active == MenuManager.GameplaySceneName)
            return "Theshattereddocks";
        return "MainMenu";
    }

    public void ShowRoundComplete()
    {
        if (shown)
            return;

        shown = true;
        IsShowing = false;
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(nextSceneAfterRound))
            SceneManager.LoadScene(nextSceneAfterRound);
    }
}
