using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneEffect : MonoBehaviour
{
    public static SceneEffect instance;
    [SerializeField] Animator transition;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SceneTransfer(string scenename)
    {
        transition.SetTrigger("end");
        SceneManager.LoadScene(scenename);
        transition.SetTrigger("start");
    }
}
