using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu actions and resilient runtime wiring for Start / Options / Quit.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public const string MainMenuSceneName = "MainMenu";
    public const string GameplaySceneName = "TheOvergrowth";
    public const string OptionsSceneName = "OptionsMenu";

    [Header("Optional explicit button refs (auto-found if left empty)")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    void Awake()
    {
        EchoesAudioDirector.EnsureExists();

        if (startButton == null)
        {
            GameObject startObj = GameObject.Find("StartButton");
            if (startObj != null)
                startButton = startObj.GetComponent<Button>();
        }

        if (optionsButton == null)
        {
            GameObject optObj = GameObject.Find("OptionsButton");
            if (optObj != null)
                optionsButton = optObj.GetComponent<Button>();
        }

        if (quitButton == null)
        {
            GameObject quitObj = GameObject.Find("QUIT");
            if (quitObj != null)
                quitButton = quitObj.GetComponent<Button>();
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
            startButton.onClick.AddListener(StartGame);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveListener(OpenOptions);
            optionsButton.onClick.AddListener(OpenOptions);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void Start()
    {
        EchoesAudioDirector.EnsureMenuMusicAudible();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GameplaySceneName);
    }

    public void OpenOptions()
    {
        SceneManager.LoadScene(OptionsSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
