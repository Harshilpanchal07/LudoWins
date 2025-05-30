using UnityEngine;

public class Gamecount : MonoBehaviour
{
    public static Gamecount Instance;

    private const string GamePlayCountKey = "GamePlayCount";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncrementGameCount()
    {
        int currentCount = PlayerPrefs.GetInt(GamePlayCountKey, 0);
        currentCount++;
        PlayerPrefs.SetInt(GamePlayCountKey, currentCount);
        PlayerPrefs.Save();

        Debug.Log("Games Played: " + currentCount);
    }

    public int GetGameCount()
    {
        return PlayerPrefs.GetInt(GamePlayCountKey, 0);
    }
}
