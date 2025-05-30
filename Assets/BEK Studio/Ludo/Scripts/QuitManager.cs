using BEKStudio;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonHandler : MonoBehaviour
{
    public static BackButtonHandler Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }

    }

    void HandleBackButton()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Menu")
        {
            if(MenuController.Instance.playerProfilePanel.activeSelf)
            {
                MenuController.Instance.closeProfilePanel();
            }
            else
            {
                MenuController.Instance.quitShow();
            }
        }
        else if (currentScene == "Game")
        {
            GameController.Instance.PauseBtn();
        }
    }

    public void OnQuitYes()
    {
        Application.Quit(); // Quit the game
    }
}
