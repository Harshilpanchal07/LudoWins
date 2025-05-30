using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Advertisements;

public class UnityAdsMain : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] private string androidid;
    [SerializeField] private string iosid;
    [SerializeField] private bool isteast;

    private string gameId;


    private void Awake()
    {
#if UNITY_ANDROID
    gameId = androidid;
#elif UNITY_IOS
    gameId = iosid;
#elif UNITY_EDITOR
    gameId = androidid;
#endif

        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(gameId, isteast, this);
        }
    }


    public void OnInitializationComplete()
    {
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
    }
}
