using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class UnityBanner : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
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

        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
    }

    public void LoadBannerad()
    {
        BannerLoadOptions bannerLoadOptions = new BannerLoadOptions {
            loadCallback = BannerLoaded,
            errorCallback = BannerError
        };

        Debug.Log("Loading Banner Ad for unity");
        Advertisement.Banner.Load(gameunitId, bannerLoadOptions);
    }


    public void ShowBannerad()
    {
        BannerOptions bannerShowOptions = new BannerOptions
        {
            showCallback = Bannershow,
            clickCallback = Bannerclick,
            hideCallback = BannerHidden
        };
        Advertisement.Banner.Show(gameunitId, bannerShowOptions);
    }
    public void HideBannerad()
    {
        Advertisement.Banner.Hide();
    }

    private void BannerHidden()
    {
    }
    private void Bannerclick()
    {
    }
    private void Bannershow()
    {
    }
    private void BannerError(string message)
    {
    }
    private void BannerLoaded()
    {
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
    }
}