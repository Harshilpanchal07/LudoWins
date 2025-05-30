using UnityEngine;
using UnityEngine.SceneManagement;

public class Redirect_To_Starting : MonoBehaviour
{
    void Start()
    {
        Invoke("LoadStartingScene", 4f);
    }

    void LoadStartingScene()
    {
        SceneEffect.instance.SceneTransfer("Starting");
    }
}
