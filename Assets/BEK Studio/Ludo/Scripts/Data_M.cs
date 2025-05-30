using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetPlayerData : MonoBehaviour
{
    public void ResetPrefsAndReloadScene()
    {
        // Log to ensure the function is called
        Debug.Log("Resetting PlayerPrefs and reloading the scene.");

        // Delete all saved preferences (reset)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();  // Make sure to save changes

        // Optionally log the reset process
        Debug.Log("PlayerPrefs have been deleted.");

        // Reload the current scene to reset everything
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}



