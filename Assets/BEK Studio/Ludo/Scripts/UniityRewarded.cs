using UnityEngine;
using UnityEngine.Advertisements;

public class UniityRewarded : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string androidunitid;
    [SerializeField] private string iosunitid;

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

    public void LoadRewardedad()
    {
        Debug.Log("Loading Rewarded Ad for unity");
        Advertisement.Load(gameunitId, this);
    }

    public void ShowRewardedad()
    {
        Advertisement.Show(gameunitId, this);
        LoadRewardedad();
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
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
        Debug.Log("Rewarded ad completed");
        AdsManager.instance.GrantMoney(100);
    }
}
