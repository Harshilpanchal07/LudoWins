using UnityEngine;
using UnityEngine.Advertisements;

public class UniityInterstitial : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string androidunitid;
    [SerializeField] private string iosunitid;
    public bool isAdReady = false;

    private string gameunitId;

    private void Awake()
    {
#if UNITY_ANDROID
        gameunitId = androidunitid;
#elif UNITY_IOS
    gameId = iosunitid;
#elif UNITY_EDITOR
    gameId = androidunitid;
#endif
    }

    public void LoadInterstitialad()
    {
        Debug.Log("Loading Interstitial Ad for unity");
        Advertisement.Load(gameunitId, this);
    }

    public void ShowInterstitialad()
    {
        Advertisement.Show(gameunitId, this);
        LoadInterstitialad();
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        isAdReady = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        isAdReady = false;
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
    }

    public void OnUnityAdsShowStart(string placementId)
    {
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
    }
}

