using UnityEngine;
using GoogleMobileAds.Api;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
using BEKStudio;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class AdsManager : MonoBehaviour
{

    public static AdsManager instance;

    [Header("Unity")]
    public UnityAdsMain unityAdsMain;
    public UnityBanner unityBanner;
    public UniityRewarded unityRewarded;
    public UniityInterstitial unityInterstitial;

    private bool showGoogleAdNext = true; // Start with Google
    //public string appId = "ca-app-pub-3940256099942544~3347511713";
    public string appId = "ca-app-pub-9642675772845791~7402775467";

#if UNITY_ANDROID
    //test ids
    //string bannerId = "ca-app-pub-3940256099942544/6300978111";
    //string interstitialId = "ca-app-pub-3940256099942544/1033173712";
    //string rewardedId = "ca-app-pub-3940256099942544/5224354917";

    //original ids
    string bannerId = "ca-app-pub-9642675772845791/6590690002";
    string interstitialId = "ca-app-pub-9642675772845791/7810851454";
    string rewardedId = "ca-app-pub-9642675772845791/9109073953";
#endif

    BannerView BannerView;
    InterstitialAd InterstitialAd;
    RewardedAd RewardedAd;
    //NativeAd NativeAd;


    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(initStatus =>
        {
            print("Admob Initialized");
        });
    }

    private void Start()
    {
        LoadBannerAds();
        LoadRewardedAd();
        LoadInterstitialAd();
        unityBanner.LoadBannerad();
        unityInterstitial.LoadInterstitialad();
        unityRewarded.LoadRewardedad();
    }


    #region banner
    public void LoadBannerAds()
    {
        //create add
        CreateBannerview();
        
        //listen any event
        ListenTOBanner();

        if(BannerView == null)
        {
            CreateBannerview();
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        print("Succesfully Load Ads");
        BannerView.LoadAd(adRequest);
    }

    public void CreateBannerview()
    {
        if (BannerView != null)
        {
            DestroyBannerAds();
        }
        BannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);
    }

    public void ListenTOBanner()
    {
        BannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + BannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        BannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
            unityBanner.ShowBannerad();

            // Start coroutine to wait before reloading banner ads
            StartCoroutine(LoadBannerAd());
        };
        // Raised when the ad is estimated to have earned money.
        BannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Banner view paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        BannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        BannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        BannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        BannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
            unityBanner.ShowBannerad();

            StartCoroutine(LoadBannerAd());
        };
    }

    private IEnumerator LoadBannerAd()
    {
        yield return new WaitForSeconds(5f);
        LoadBannerAds();
        unityBanner.LoadBannerad();
    }

    public void DestroyBannerAds()
    {
        if (BannerView != null)
        {
            print("Banner Destroyed");
            BannerView.Destroy();
            BannerView = null;
        }
    }

    #endregion

    #region Interstitial

    public void LoadInterstitialAd()
    {

        if (InterstitialAd != null)
        {
            InterstitialAd.Destroy();
            InterstitialAd = null;
        }
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        InterstitialAd.Load(interstitialId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                print("Interstitial ad failed to load" + error);
                return;
            }

            print("Interstitial ad loaded !!" + ad.GetResponseInfo());

            InterstitialAd = ad;
            InterstitialEvent(InterstitialAd);
        });

    }
    public void ShowInterstitialAd()
    {

        if (InterstitialAd != null && InterstitialAd.CanShowAd())
        {
            InterstitialAd.Show();
        }
        else if (unityInterstitial.isAdReady){
            unityInterstitial.ShowInterstitialad();
        }
        else
        {
            print("Intersititial ad not ready!!");
            LoadInterstitialAd();
            unityInterstitial.LoadInterstitialad();
        }
    }
    public void InterstitialEvent(InterstitialAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Interstitial ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
            StartCoroutine(LoadInterstitial());
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
            StartCoroutine(LoadInterstitial());
        };
    }

    private IEnumerator LoadInterstitial()
    {
        yield return new WaitForSeconds(5f);
        LoadInterstitialAd();
        unityInterstitial.LoadInterstitialad();
    }

    #endregion

    #region Rewarded

    public void LoadRewardedAd()
    {

        if (RewardedAd != null)
        {
            RewardedAd.Destroy();
            RewardedAd = null;
        }
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedAd.Load(rewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                print("Rewarded failed to load" + error);
                return;
            }

            print("Rewarded ad loaded !!");
            RewardedAd = ad;
            RewardedAdEvents(RewardedAd);
        });
    }

    public void ShowRewardedAd()
    {
        if (showGoogleAdNext)
        {
            if (RewardedAd != null && RewardedAd.CanShowAd())
            {
                RewardedAd.Show((Reward reward) =>
                {
                    LoadRewardedAd();
                    showGoogleAdNext = false;
                });
            }
            else
            {
                // Try Unity Ad
                TryShowUnityAd();
            }
        }
        else
        {
            if (unityRewarded != null)
            {
                unityRewarded.ShowRewardedad();
                showGoogleAdNext = true;
            }
            else
            {
                // Try Google Ad
                TryShowGoogleAd();
            }
        }
    }

    private void TryShowUnityAd()
    {
        if (unityRewarded != null)
        {
            unityRewarded.ShowRewardedad();
            showGoogleAdNext = true;
        }
        else
        {
            NoticeManager.Show("No Ads Available", true);
            Debug.Log("No Unity Ads available either");
        }
    }

    private void TryShowGoogleAd()
    {
        if (RewardedAd != null && RewardedAd.CanShowAd())
        {
            RewardedAd.Show((Reward reward) =>
            {
                showGoogleAdNext = false;
            });
        }
        else
        {
            NoticeManager.Show("No Ads Available" , true);
            Debug.Log("No Google Ads available either");
        }
    }

    public void RewardedAdEvents(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Rewarded ad paid {0} {1}." +
                adValue.Value +
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
            GrantMoney(100);
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    #endregion

    #region extra

    public void GrantMoney(int gem)
    {
        int currentMoney = PlayerPrefs.GetInt("gem");
        currentMoney += gem;
        PlayerPrefs.SetInt("gem", currentMoney);
        PlayerPrefs.Save();
        MenuController.Instance.topCoinText.text = currentMoney.ToString();
        CloudSave.instance.SaveDataAsInt("gem", currentMoney);
    }
    #endregion
}
