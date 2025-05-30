using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System;
using Unity.Services.CloudSave;
//using UnityEditor.Search;

namespace BEKStudio
{
    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance;
        public GameObject dontDestroyPrefab;
        public Sprite[] avatars;

        [Header("Top")]
        public Image topAvatarImg;
        public TextMeshProUGUI topCoinText;

        [Header("Main")]
        public GameObject mainBottom;
        public GameObject mainBottomHomeActive;
        public GameObject mainBottomStoreActive;

        [Header("Home")]
        public GameObject homeScreen;
        public GameObject homePanel;

        [Header("Friends")]
        public GameObject friendScreen;
        public GameObject friendPanel;

        [Header("Inside Friends Tabs")]
        public GameObject ChallengePanel;
        public GameObject inboxPanel;
        public GameObject popUpimagec;
        public GameObject popUpimagei;

        [Header("Store")]
        public GameObject storeScreen;
        public GameObject storePanel;

        [Header("Settings")]
        public GameObject settingScreen;
        public GameObject settingPanel;

        [Header("Pawn Select")]
        public GameObject pawnSelectScreen;
        public GameObject pawnSelectPanel;

        [Header("Player Count")]
        public GameObject playerCountScreen;
        public GameObject playerCountPanel;
        public TextMeshProUGUI playerCountEntryFee;

        [Header("Online")]
        public GameObject onlineScreen;
        public GameObject onlinePanel;
        public TextMeshProUGUI onlineInfoText;
        public Button onlineCancelButton;

        [Header("Username")]
        public Image userAvatar;
        public GameObject usernameScreen;
        public GameObject usernamePanel;
        public TMP_InputField usernameInput;
        public GameObject avatarSelectionPanel;
        public GameObject downButton;
        public GameObject upButton;

        [Header("Active Tabs")]
        public GameObject HomeActiveTab;
        public GameObject ShopActiveTab;

        [Header("player profile")]
        public GameObject playerProfilePanel;
        public Image playerAvatar;
        public TextMeshProUGUI UsernameText;
        public TextMeshProUGUI playerEmail;
        public TextMeshProUGUI playerEmailText;
        public TextMeshProUGUI playerID;
        public TextMeshProUGUI TotalWin;
        public TextMeshProUGUI TotalGamePlayed;
        public TextMeshProUGUI loginInfo;

        [Header("Icon")]
        public Image HomeIcon;
        public Image WalletIcon;
        public Image ShopIcon;

        [Header("Text")]
        public TextMeshProUGUI HomeText;
        public TextMeshProUGUI WalletText;
        public TextMeshProUGUI ShopText;

        private Vector3 normalScale = Vector3.one;
        private Vector3 enlargedScale = new Vector3(1.2f, 1.2f, 1.2f);
        private int normalFontSize = 36;
        private int enlargedFontSize = 45;

        private GameObject currentActiveTab; 
        private Image currentIcon;
        private TextMeshProUGUI currentText;
        public TextMeshProUGUI activeTabText;

        [Header("Room Create Popup")]
        public GameObject roomPopupPanel;
        public TMP_InputField CreateroomIDtext;
        public bool isCreatingRoom;

        [Header("Room Join Popup")]
        public GameObject JoinPanel;
        public TMP_InputField JoinroomIDtext;

        [Header("Room")]
        public GameObject RoomPanel;
        public TMP_Text RoomID;
        public GameObject LeaveRoomPanel;
        public Button StartGameBtn;

        [Header("Loading")]
        public GameObject loadingPopup;
        public TMP_Text loadingMessage;
        public Slider progressBar;

        [Header("Quit")]
        public GameObject quitPanel;

        [Header("new")]
        public ConnectToServer connectToServer;

        [Header("Join selection")]
        public GameObject Event;

        [Header("Google")]
        public GoogleLogin googlelogin;

        [Header("Anonymous")]
        public AnonymousLogin anonymouslogin;

        void Awake()
        {

            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start()
        {
            if (PlayerPrefs.HasKey("pawnColor"))
            {
                PlayerPrefs.DeleteKey("pawnColor");
            }

            GameObject dontDestroyObj = GameObject.Find("DontDestroy");
            if (dontDestroyObj == null)
            {
                dontDestroyObj = Instantiate(dontDestroyPrefab);
                dontDestroyObj.name = "DontDestroy";
                DontDestroyOnLoad(dontDestroyObj);
            }


            if (!PlayerPrefs.HasKey("firstTime"))
            {
                PlayerPrefs.SetInt("gem", Constants.START_GEM);
                CloudSave.instance.SaveDataAsInt("gem", PlayerPrefs.GetInt("gem"));
                PlayerPrefs.SetInt("firstTime", 1);
                PlayerPrefs.Save();
            }

            topAvatarImg.sprite = avatars[PlayerPrefs.GetInt("avatar")];
            UpdateGemText();

            int gamecount = Gamecount.Instance.GetGameCount();
            if (gamecount % 2 == 1)
            {
                Debug.Log("Odd number - Show Ad!");
                AdsManager.instance.ShowInterstitialAd();
            }
            else
            {
                Debug.Log("hgyg");
            }

            HandleUserDataAfterLogin();
            HomeShow();
        }

        public async void HandleUserDataAfterLogin()
        {
            bool hasUsername = PlayerPrefs.HasKey("username");
            bool hasAvatar = PlayerPrefs.HasKey("avatar");

            if (hasUsername && hasAvatar)
            {
                UsernameText.text = PlayerPrefs.GetString("username");
                topAvatarImg.sprite = avatars[PlayerPrefs.GetInt("avatar")];
                Debug.Log("User data loaded from local storage.");
                return;
            }
            else
            {

                try
                {
                    var keysToLoad = new HashSet<string> { "UserName", "avatar_index", "gem" };
                    var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(keysToLoad);

                    if (savedData.ContainsKey("UserName") && savedData.ContainsKey("avatar_index"))
                    {
                        string username = await CloudSave.instance.LoadStrData("UserName");
                        int avatarIndex = await CloudSave.instance.LoadIntData("avatar_index");

                        // Save locally too if needed
                        PlayerPrefs.SetString("username", username);
                        PlayerPrefs.SetInt("avatar", avatarIndex);
                        PlayerPrefs.Save();

                        // Update UI
                        UsernameText.text = username;
                        topAvatarImg.sprite = avatars[avatarIndex];
                        Debug.Log("User data loaded from cloud storage.");
                    }
                    else
                    {
#if UNITY_WEBGL
            string rand = Random.Range(999, 999999).ToString();
            string defaultUsername = "Player" + rand;
            int defaultAvatar = 0;

            PlayerPrefs.SetString("username", defaultUsername);
            PlayerPrefs.SetInt("avatar", defaultAvatar);
            PlayerPrefs.Save();

            cloudSave.SaveData("UserName", defaultUsername);
            cloudSave.SaveData("avatar_index", defaultAvatar.ToString());

            topUsernameText.text = defaultUsername;
            topAvatarImg.sprite = avatars[defaultAvatar];
            userAvatar.sprite = avatars[defaultAvatar];

            HomeShow();
#else
                        UsernameShow(); // For mobile or devices where manual entry is needed
#endif
                    }
                }
                catch (CloudSaveException e)
                {
                    Debug.LogError($"Cloud load failed: {e.Message}");
                    //UsernameShow();
                }
            }
        }

        public void UpdateGemText()
        {
            int gem = PlayerPrefs.GetInt("gem");
            topCoinText.text = gem == 0 ? "0" : gem.ToString("###,###,###");
        }

        public void ShowRewardedads()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            if (!NetworkCheck.IsNetworkAvailable())
            {
                NoticeManager.Show("No internet connection", true);
                Debug.Log("Network not available.");
                return;
            }
            else
            {
                AdsManager.instance.ShowRewardedAd();
            }
        }

        void HomeShow()
        {
            HideAllPanelsExcept(homeScreen);
            ShowPanel(homeScreen);
            mainBottom.SetActive(true);
            ActivateTab(HomeActiveTab, HomeIcon, HomeText, "");
        }

        void FriendShow()
        {
            HideAllPanelsExcept(friendScreen);
            popUpimagei.SetActive(false);
            ShowPanel(friendScreen);
        }

        void HomeClose()
        {
            homeScreen.SetActive(false);
        }

        void StoreShow()
        {
            HideAllPanelsExcept(storeScreen);
            ShowPanel(storeScreen);
            ActivateTab(ShopActiveTab, ShopIcon, ShopText, "Shop");
        }

        void SettingShow()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            HideAllPanelsExcept(settingScreen);
            ShowPanel(settingScreen);

        }

        public void settingClose()
        {
            settingScreen.SetActive(false);
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            HomeShow();
        }

        void HideAllPanelsExcept(GameObject activePanel)
        {
            GameObject[] panels = { homeScreen, storeScreen, friendScreen};

            foreach (GameObject panel in panels)
            {
                if (panel != activePanel && panel.activeSelf)
                {
                    HidePanel(panel);
                }
            }
        }
        void MainBottomCheckTabs(string activeTab)
        {
            // Close all panels
            homeScreen.SetActive(false);
            storeScreen.SetActive(false);
            // Add more panels as needed.

            // Open the selected tab's panel
            switch (activeTab)
            {
                case "Home":
                    HomeShow();
                    break;
                case "Shop":
                    StoreShow();
                    break;
                default:
                    homeScreen.SetActive(true); // Fallback to home if anything goes wrong
                    break;
            }
        }

        public void ActivateTab(GameObject activeTab, Image icon, TextMeshProUGUI text, string tabName)
        {

            HomeActiveTab.SetActive(false);
            ShopActiveTab.SetActive(false);

            // Activate the selected tab
            activeTab.SetActive(true);

            // If the same tab is clicked again, do nothing
            if (currentActiveTab == activeTab) return;

            // Reset previous active tab
            if (currentActiveTab != null)
            {
                AnimateTabReset(currentIcon, currentText);
                currentActiveTab.SetActive(false);
            }

            // Set the new active tab
            currentActiveTab = activeTab;
            currentIcon = icon;
            currentText = text;
            currentActiveTab.SetActive(true);

            // Update the UI text to show the active tab name
            activeTabText.text = tabName;

            // Animate the new active tab
            AnimateTab(currentIcon, currentText);
        }

        void AnimateTab(Image icon, TextMeshProUGUI text)
        {
            LeanTween.scale(icon.gameObject, enlargedScale, 0.3f).setEaseOutBack();
            LeanTween.value(text.gameObject, normalFontSize, enlargedFontSize, 0.3f)
                .setOnUpdate((float value) => { text.fontSize = (int)value; });
        }

        void AnimateTabReset(Image icon, TextMeshProUGUI text)
        {
            LeanTween.scale(icon.gameObject, normalScale, 0.3f).setEaseOutBack();
            LeanTween.value(text.gameObject, enlargedFontSize, normalFontSize, 0.3f)
                .setOnUpdate((float value) => { text.fontSize = (int)value; });
        }

        public void MainHomeBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.CloseClip);
            MainBottomCheckTabs("Home");
            mainBottom.SetActive(true);
        }
        public void MainFriendBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            MainBottomCheckTabs("Friend");
        }
        public void MainStoreBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.CloseClip);
            MainBottomCheckTabs("Shop");
        }

        public void PlayWithFriendShow()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);

            // Check if device has internet
            if (!NetworkCheck.IsNetworkAvailable())
            {
                NoticeManager.Show("No internet connection", true);
                Debug.Log("Network not available.");
                return;
            }

            if (!PhotonNetwork.IsConnected)
            {
                ConnectToServer.Instance.Connect();
                ShowLoadingScreen(true, "wait for connection !");
                ConnectToServer.Instance.OnLobbyJoined += OnLobbyJoined;
            }
            else
            {
                FriendShow();
                mainBottom.SetActive(false);
                roomPopupPanel.SetActive(false);
            }
        } 

        public void friendClose()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClosePanelClip);
            MainBottomCheckTabs("Home");
            mainBottom.SetActive(true);
        }

        public void ChallangeShow()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            inboxPanel.SetActive(false);
            popUpimagei.SetActive(false);
            ChallengePanel.SetActive(true);
            popUpimagec.SetActive(true);
        }

        public void InboxShow()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            ChallengePanel.SetActive(false);
            popUpimagec.SetActive(false);
            inboxPanel.SetActive(true);
            popUpimagei.SetActive(true);
        }

        public void ShowPanel(GameObject panel)
        {
            panel.SetActive(true);
            panel.transform.localScale = Vector3.zero;
            LeanTween.scale(panel, Vector3.one, 0.3f).setEaseOutBack();
        }

        void HidePanel(GameObject panel, System.Action onComplete = null)
        {
            LeanTween.scale(panel, Vector3.zero, 0.3f).setEaseInBack().setOnComplete(() =>
            {
                panel.SetActive(false);
                onComplete?.Invoke();
            });
        }

        public void quitShow()
        {
            quitPanel.SetActive(true);
        }

        public void quitClose()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            quitPanel.SetActive(false);
        }

        public void quitYesBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            BackButtonHandler.Instance.OnQuitYes();
        }

        public void ShowProfile()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            playerAvatar.GetComponent<Image>().sprite = avatars[PlayerPrefs.GetInt("avatar")];
            UsernameText.text = PlayerPrefs.GetString("username");
            playerID.text = PlayerPrefs.GetString("playerID");
            TotalGamePlayed.text = Gamecount.Instance.GetGameCount().ToString();
            TotalWin.text = PlayerPrefs.GetInt("totalwin").ToString();
            loginInfo.text = PlayerPrefs.GetString("loginmethod");
            if (PlayerPrefs.HasKey("email"))
            {
                playerEmailText.gameObject.SetActive(true);
                playerEmail.text = PlayerPrefs.GetString("email");
            }
            else
            {
                playerEmailText.gameObject.SetActive(false);
            }
            playerProfilePanel.SetActive(true);
            playerProfilePanel.transform.localPosition = new Vector3(800, 0, 0);
            LeanTween.moveLocalX(playerProfilePanel, 0, 0.5f).setEaseOutCubic();
        }

        public void closeProfilePanel()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClosePanelClip);
            LeanTween.moveLocalX(playerProfilePanel, 800, 0.4f)
                .setEaseInCubic()
                .setOnComplete(() => playerProfilePanel.SetActive(false));
        }

        public void logout()
        {
            if (PlayerPrefs.HasKey("email"))
            {
                googlelogin.OnSignOut();
            }
            else
            {
                anonymouslogin.SimpleSignOut();
            }
            
            SceneManager.LoadScene("Login");
        }

        public void ShowRoomPopupBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            ShowPanel(roomPopupPanel);
        }

        public void CloseRoomPopupBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.CloseClip);
            HidePanel(roomPopupPanel);
        }

        public void CreateRoom()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            PlayerPrefs.SetString("mode", "online");
            PlayerPrefs.Save();
            string RoomName = CreateroomIDtext.text;
            ConnectRoom.Instance.CreateRoom(RoomName);
        }

        public void ShowRoom()
        {
            ShowPanel(RoomPanel);
            RoomID.text = PhotonNetwork.CurrentRoom.Name;
            ShowGamestartBtn();
        }

        public void ShowGamestartBtn()
        {
            StartGameBtn.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }

        public void StartGame()
        {
            AdsManager.instance.unityBanner.HideBannerad();
            SceneManager.LoadScene("Game");
        }

        public void BacktoMenu()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClosePanelClip);
            LeaveRoomPanel.SetActive(true);
        }

        public void CancelLeaveRoom()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            LeaveRoomPanel.SetActive(false);
        }

        public void LeaveRoom()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            PhotonNetwork.LeaveRoom();
            LeaveRoomPanel.SetActive(false);
            HidePanel(RoomPanel);
            MainBottomCheckTabs("Home");
            DisConnectNet();
        }

        private void OnLobbyJoined()
        {
            ShowLoadingScreen(false); // Hide loading screen
            FriendShow();
            mainBottom.SetActive(false);

            // Unsubscribe to prevent duplicate calls
            ConnectToServer.Instance.OnLobbyJoined -= OnLobbyJoined;
        }


        public void ShowLoadingScreen(bool status, string message = "")
        {
            loadingPopup.SetActive(status);
            if (status)
            {
                progressBar.value = 0f;
                loadingMessage.text = message;
                StartCoroutine(UpdateLoadingBar());
            }
        }

        private IEnumerator UpdateLoadingBar()
        {
            while (loadingPopup.activeSelf && progressBar.value < 1f)
            {
                progressBar.value += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void ShowJoinRoomPopup()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            ShowPanel(JoinPanel);
        }

        public void CloseJoinRoomPopup()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.CloseClip);
            HidePanel(JoinPanel);
        }

        public void JoinRoom()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            string roomName = JoinroomIDtext.text;
            ConnectRoom.Instance.JoinRoom(roomName);
        }

        public void ShowEvent()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            mainBottom.SetActive(false);
            HidePanel(homeScreen);
            ShowPanel(Event);
        }

        public void CloseEvent()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClosePanelClip);
            mainBottom.SetActive(true);
            HidePanel(Event);
            HomeShow();
        }

        public void ShowComSoon()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            NoticeManager.Show("Coming Soon", true);
        }

        public void DisConnectNet()
        {
            PhotonNetwork.Disconnect();
        }

        public void MainOnlineBtn()
        {
            if (onlineScreen.activeInHierarchy) return;
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);

            PlayerPrefs.SetString("mode", "online");
            PlayerPrefs.Save();
            PlayerCountShow();
        }

        public void MainVsComputerBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            if (pawnSelectScreen.activeInHierarchy) return;

            PlayerPrefs.SetString("mode", "computer");
            PlayerPrefs.Save();
            DisConnectNet();
            PawnSelectShow();
        }

        public void MainWatchVideoBtn()
        {
            //AdsManager.Instance.ShowRewardedAd();
        }

        public void StoreItemBtn(int id)
        {
            //Purchaser.Instance.BuyConsumable(id);
        }

        public void StoreRestoreBtn()
        {
            //Purchaser.Instance.RestorePurchases();
        }   

        void PawnSelectShow()
        {
            ShowPanel(pawnSelectScreen);
        }

        public void PawnSelectItemBtn(string pawnColor)
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            PlayerPrefs.SetString("pawnColor", pawnColor);
            PlayerPrefs.Save();
            PlayerCountShow();
            PawnSelectClose();
        }

        public void PawnSelectCloseBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.CloseClip);
            PlayerPrefs.DeleteKey("pawnColor");
            PawnSelectClose();
        }

        void PawnSelectClose()
        {
            HidePanel(pawnSelectScreen);
        }

        void PlayerCountShow()
        {
            playerCountEntryFee.text = ConnectRoom.Instance.gameEntryPrice().ToString();
            ShowPanel(playerCountScreen);
        }

        public void PlayerCountItemBtn(int playerCount)
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);

            int entryFee = ConnectRoom.Instance.gameEntryPrice();

            if (PlayerPrefs.GetInt("gem") < entryFee)
            {
                playerCountScreen.SetActive(false);
                NoticeManager.Show("OOPs! Out of gems, plaese purchase some gems for further play", true);
                StoreShow();
                return;
            }

            PlayerPrefs.SetInt("playerCount", playerCount);
            PlayerPrefs.Save();
            if (ConnectRoom.Instance.gameMode() == "online")
            {
                OnlineShow();
            }
            else
            {
                Gamecount.Instance.IncrementGameCount();
                SceneManager.LoadScene("Game");
            }
        }

        public void PlayerCountCloseBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.CloseClip);
            PlayerCountClose();
        }

        void PlayerCountClose()
        {
            HidePanel(playerCountScreen);
        }

        public void OnlineShow()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            onlineInfoText.text = "Connecting to server...";
            onlineScreen.SetActive(true);
        }

        public void OnlineClose()
        {
            onlineScreen.SetActive(false);
        }

        public void OnlineInfoMsg(string msg)
        {
            onlineInfoText.text = msg;
        }

        public void OnlineCancelBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            PhotonNetwork.AutomaticallySyncScene = false;

            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.Disconnect();
            }
            else
            {
                OnlineClose();
            }
        }

        void UsernameShow()
        {
            usernameScreen.SetActive(true);
        }

        public void UsernameSaveBtn()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            if (usernameInput.text.Length >= 4)
            {
                //in devices
                PlayerPrefs.SetString("username", usernameInput.text);
                PlayerPrefs.Save();

                //at cloud
                int avatar_index = PlayerPrefs.GetInt("avatar");
                CloudSave.instance.SaveDataAsString("UserName", usernameInput.text);
                CloudSave.instance.SaveDataAsInt("avatar_index", PlayerPrefs.GetInt("avatar"));

                //for ui
                topAvatarImg.sprite = avatars[PlayerPrefs.GetInt("avatar")];
                UsernameClose();
            }
        }

        public void SelectAvatar(int index)
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            PlayerPrefs.SetInt("avatar", index);
            PlayerPrefs.Save();
            userAvatar.sprite = avatars[PlayerPrefs.GetInt("avatar")];
            print(index);
            CloseAvatar();
        }

        public void ShowAvatar()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.MouseClickClip);
            avatarSelectionPanel.SetActive(true);
            downButton.SetActive(false);
            upButton.SetActive(true);
        }

        public void CloseAvatar()
        {
            AudioController.Instance.PlaySFX(AudioController.Instance.MouseClickClip);
            avatarSelectionPanel.SetActive(false);
            downButton.SetActive(true);
            upButton.SetActive(false);
        }

        void UsernameClose()
        {
            usernameScreen.SetActive(false);
            HomeShow();
        }
    }
}
