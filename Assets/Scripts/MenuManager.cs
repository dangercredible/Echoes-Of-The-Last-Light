using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu actions and resilient runtime wiring for Start/Quit buttons.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Optional explicit button refs (auto-found if left empty)")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        // Keep menu functional even if scene OnClick bindings were not set.
        if (startButton == null)
        {
            var startObj = GameObject.Find("StartButton");
            if (startObj != null) startButton = startObj.GetComponent<Button>();
        }

        if (quitButton == null)
        {
            var quitObj = GameObject.Find("QUIT");
            if (quitObj != null) quitButton = quitObj.GetComponent<Button>();
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
            startButton.onClick.AddListener(StartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    public void StartGame()
    {
        // Load gameplay scene configured for this project.
        SceneManager.LoadScene("gamescene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
