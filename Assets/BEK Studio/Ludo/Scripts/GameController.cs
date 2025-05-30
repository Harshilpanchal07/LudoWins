using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static RotateDice;


namespace BEKStudio {
    public class GameController : MonoBehaviour {
        public static GameController Instance;
        public enum GameState { NONE, READY, DICE, MOVE, MOVING, WAIT, FINISHED };
        public GameState gameState;
        public Transform waypointParent;
        public Transform playersParent;
        public Transform colorWayParent;
        public Transform pawnParent;
        public PawnController greenPawn;
        public PawnController bluePawn;
        public PawnController yellowPawn;
        public PawnController redPawn;
        public PawnController myPawnController;
        public PawnController currentPawnController;
        public List<PawnController> activePawnControllers;
        public Pawn[] greenPawns;
        public Pawn[] bluePawns;
        public Pawn[] yellowPawns;
        public Pawn[] redPawns;
        public Pawn[] allPawns;
        public Sprite[] avatars;
        Pawn[] myPawns;
        public string myPlayerColor;
        RaycastHit2D hit2D;
        public int currentDice;
        string[] pawnColors = new string[] { "Green", "Yellow", "Blue", "Red" };
        public int currentPawnID = 0;
        public bool isLocal;
        public PhotonView photonView;
        public GameObject pauseScreen;
        Pawn selectedPawn;
        [Header("Finished")]
        public GameObject finishedScreen;
        public GameObject finishedPanel;
        public Transform finishedPlayersParent;
        string winnerColor;


        // my editing
        [Header("Dice References")]
        [SerializeField] public RotateDice rotateDice; // Assign 3D dice in inspector
        public GameObject dice;
        public GameObject Gdice;
        public GameObject Ydice;
        public GameObject Bdice;
        public GameObject Rdice;
        public int diceResult;

        // Add dice button references
        [Header("Dice Buttons")]
        public Button greenDiceButton;
        public Button yellowDiceButton;
        public Button blueDiceButton;
        public Button redDiceButton;
        private bool isRolling = false; // Prevent spam clicking

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
        }

        void OnEnable() {
            isLocal = ConnectRoom.Instance.gameMode() == "computer";

            photonView = GetComponent<PhotonView>();

            myPlayerColor = getMyPawnColor();
            myPawnController = getMyPawn();
            myPawns = greenPawns;
            activePawnControllers = new List<PawnController>();

            PlayerPrefs.SetInt("gem", PlayerPrefs.GetInt("gem") - ConnectRoom.Instance.gameEntryPrice());
            PlayerPrefs.Save();

            DisableNotActivePawns();
#if UNITY_ANDROID || UNITY_IPHONE
            //AdsManager.LoadBannerAds();
#endif
        }

        void Update()
        {
            if (gameState != GameState.MOVE || gameState == GameState.WAIT) return;

            if (Input.GetMouseButtonDown(0) && currentPawnController == myPawnController)
            {
                hit2D = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit2D.collider != null)
                {
                    if (hit2D.collider.tag == "Pawn" && hit2D.collider.name.StartsWith(myPlayerColor))
                    {
                        if (hit2D.collider.GetComponent<Pawn>().inBase && currentDice != 5) return;

                        if (hit2D.collider.GetComponent<Pawn>().moveCount + (currentDice + 1) > 56) return;
                        myPawnController.HighlightDices(false);
                        gameState = GameState.MOVING;
                        selectedPawn = hit2D.collider.GetComponent<Pawn>();
                        hit2D.collider.GetComponent<Pawn>().Move(currentDice + 1);
                    }
                }
            }
        }

        public void PauseBtn() {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            pauseScreen.SetActive(true);
        }

        public void PauseYesBtn() {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.Disconnect();
            LeanTween.cancelAll();
            //AdsManager.Instance.ShowInterstitialAd();
            SceneManager.LoadScene("Menu");
        }

        public void PauseNoBtn() {
            AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
            pauseScreen.SetActive(false);
        }

        [PunRPC]
        void RPCPawnSelect(string arg) {
            Pawn p = allPawns.Where(x => x.name == arg).FirstOrDefault();
            if (p == null) return;

            selectedPawn = p;
            currentPawnController.HighlightDices(false);
            gameState = GameState.MOVING;
            p.Move(currentDice + 1);
        }

        string getMyPawnColor() {
            if (isLocal) {
                return PlayerPrefs.GetString("pawnColor");
            } else {
                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++) {
                    if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer) {
                        return pawnColors[i];
                    }
                }
            }

            return "";
        }

        int getPlayerCount() {
            if (isLocal) {
                return PlayerPrefs.GetInt("playerCount");
            } else {
                return PhotonNetwork.PlayerList.Length;
            }
        }

        PawnController getMyPawn() {
            if (myPlayerColor == "Green") return greenPawn;
            if (myPlayerColor == "Yellow") return yellowPawn;
            if (myPlayerColor == "Blue") return bluePawn;
            if (myPlayerColor == "Red") return redPawn;

            return null;
        }

        void DisableNotActivePawns()
        {
            List<string> colors = pawnColors.ToList();
            List<string> disabledColors = new List<string>();

            int removeCount = 4 - getPlayerCount();

            if (isLocal)
            {
                if (removeCount == 2)
                {

                    string pairedColor = GetPairedColor(myPlayerColor);

                    // Collect colors that are neither the player's color nor the paired color
                    List<string> colorsToDisable = new List<string>();
                    foreach (string color in pawnColors)
                    {
                        if (color != myPlayerColor && color != pairedColor)
                        {
                            colorsToDisable.Add(color);
                        }
                    }
                    // Add to disabledColors and remove from active colors
                    foreach (string color in colorsToDisable)
                    {
                        disabledColors.Add(color);
                        colors.Remove(color);
                    }
                }
                else
                {
                    int rand = UnityEngine.Random.Range(0, colors.Count);

                    for (int i = 0; i < removeCount; i++)
                    {
                        rand = UnityEngine.Random.Range(0, colors.Count);

                        while (colors[rand] == myPlayerColor)
                        {
                            rand = UnityEngine.Random.Range(0, colors.Count);
                        }

                        disabledColors.Add(colors[rand]);
                        colors.Remove(colors[rand]);
                    }
                }
            }
            else
            {
                for (int i = getPlayerCount(); i < 4; i++)
                {
                    if (i < colors.Count)
                    {
                        disabledColors.Add(colors[i]);
                        colors.RemoveAt(i);
                        i--;
                    }
                }
            }

            for (int i = 0; i < disabledColors.Count; i++)
            {
                if (disabledColors[i] == "Green")
                {
                    greenPawn.DisablePawn();
                }
                else if (disabledColors[i] == "Yellow")
                {
                    yellowPawn.DisablePawn();
                }
                else if (disabledColors[i] == "Blue")
                {
                    bluePawn.DisablePawn();
                }
                else if (disabledColors[i] == "Red")
                {
                    redPawn.DisablePawn();
                }
            }

            for (int i = 0; i < colors.Count; i++)
            {
                if (colors[i] == "Green")
                {
                    activePawnControllers.Add(greenPawn);
                    greenPawn.isBot = !PhotonNetwork.IsConnectedAndReady && colors[i] != myPlayerColor;
                    if (!isLocal)
                    {
                        greenPawn.SetUserInfo(PhotonNetwork.PlayerList[i].NickName, (int)PhotonNetwork.PlayerList[i].CustomProperties["avatar"]);
                    }
                    else
                    {
                        greenPawn.SetUserInfo();
                    }
                }
                else if (colors[i] == "Yellow")
                {
                    activePawnControllers.Add(yellowPawn);
                    yellowPawn.isBot = !PhotonNetwork.IsConnectedAndReady && colors[i] != myPlayerColor;
                    if (!isLocal)
                    {
                        yellowPawn.SetUserInfo(PhotonNetwork.PlayerList[i].NickName, (int)PhotonNetwork.PlayerList[i].CustomProperties["avatar"]);
                    }
                    else
                    {
                        yellowPawn.SetUserInfo();
                    }
                }
                else if (colors[i] == "Blue")
                {
                    activePawnControllers.Add(bluePawn);
                    bluePawn.isBot = !PhotonNetwork.IsConnectedAndReady && colors[i] != myPlayerColor;
                    if (!isLocal)
                    {
                        bluePawn.SetUserInfo(PhotonNetwork.PlayerList[i].NickName, (int)PhotonNetwork.PlayerList[i].CustomProperties["avatar"]);
                    }
                    else
                    {
                        bluePawn.SetUserInfo();
                    }
                }
                else if (colors[i] == "Red")
                {
                    activePawnControllers.Add(redPawn);
                    redPawn.isBot = !PhotonNetwork.IsConnectedAndReady && colors[i] != myPlayerColor;
                    if (!isLocal)
                    {
                        redPawn.SetUserInfo(PhotonNetwork.PlayerList[i].NickName, (int)PhotonNetwork.PlayerList[i].CustomProperties["avatar"]);
                    }
                    else
                    {
                        redPawn.SetUserInfo();
                    }
                }
            }

            foreach (PawnController pController in activePawnControllers)
            {
                if (pController != myPawnController)
                {
                    pController.DisableColliders();
                }
            }

            SetActivePawn();
        }

        private string GetPairedColor(string myPlayerColor)
        {
            switch (myPlayerColor)
            {
                case "Green":
                    return "Blue";
                case "Blue":
                    return "Green";
                case "Yellow":
                    return "Red";
                case "Red":
                    return "Yellow";
                default:
                    Debug.LogError("Unknown player color: " + myPlayerColor);
                    return null;
            }
        }

        void SetActivePawn() {
            if (currentPawnController != null) {
                currentPawnController.time = 10;
                currentPawnController.canPlayAgain = false;
            }

            if (isLocal) {
                currentPawnController = activePawnControllers[currentPawnID];
            } else {
                currentPawnController = getCurrentPawnController();
            }
            
            currentPawnController.time = 10;
            currentPawnController.canPlayAgain = false;

            ChangeGameState(GameState.READY);

            if (isLocal) {
                if (currentPawnController != myPawnController) {
                    currentPawnController.StartTimer(true);
                    currentPawnController.Play();
                } else {
                    currentPawnController.StartTimer(true);
                }
            } else if(currentPawnController == myPawnController){
                currentPawnController.StartTimer(true);
            }
        }

        PawnController getCurrentPawnController() {
            int colorID = (int)PhotonNetwork.MasterClient.CustomProperties["colorID"];

            if (colorID == 0) {
                return greenPawn;
            } else if (colorID == 1) {
                return yellowPawn;
            } else if (colorID == 2) {
                return bluePawn;
            } else if (colorID == 3) {
                return redPawn;
            }

            return null;
        }

        public void ChangeGameState(GameState newState) {
            gameState = newState;

            if (newState == GameState.FINISHED) {
                if (!isLocal) {
                    if (PhotonNetwork.IsMasterClient) {
                        photonView.RPC("WinnerColorRPC", RpcTarget.OthersBuffered, winnerColor);
                    }
                }

                FinishedShow();
                AudioController.Instance.PlaySFX(AudioController.Instance.winningClip);
            }
        }

        [PunRPC]
        void WinnerColorRPC(string color) {
            CheckForFinish(color);
        }

        public void CheckGameStatus() {
            ChangeGameState(GameState.WAIT);

            CheckPawnsForSameWay();
        }

        string isSomeoneFinished() {
            Pawn[] collectedGreenPaws = greenPawns.Where(x => x.isCollected).ToArray();
            Pawn[] collectedBluePaws = bluePawns.Where(x => x.isCollected).ToArray();
            Pawn[] collectedYellowPaws = yellowPawns.Where(x => x.isCollected).ToArray();
            Pawn[] collectedRedPaws = redPawns.Where(x => x.isCollected).ToArray();


            if (collectedGreenPaws.Length == 4) {
                return "Green";
            }

            if (collectedYellowPaws.Length == 4) {
                return "Yellow";
            }

            if (collectedBluePaws.Length == 4) {
                return "Blue";
            }


            if (collectedRedPaws.Length == 4) {
                return "Red";
            }

            return "";
        }

        void CheckPawnsForSameWay() {
            bool wait = false;
            foreach (PawnController pController in activePawnControllers) {
                if (pController == currentPawnController) continue;
                
                Pawn[] currentPawns = currentPawnController.pawns;
                Pawn[] activePawns = pController.pawns;

                foreach(Pawn currentPawn in currentPawns) {
                    foreach (Pawn activePawn in activePawns) {
                        if (currentPawn.currentWayID != activePawn.currentWayID) continue;
                        if (currentPawn.inBase || activePawn.inBase) continue;
                        if (currentPawn.isProtected || activePawn.isProtected) continue;
                        if (currentPawn.isCollected || activePawn.isCollected) continue;
                        if (currentPawn.inColorWay || activePawn.inColorWay) continue;

                        wait = true;
                        activePawn.ReturnToBase();
                        AudioController.Instance.PlaySFX(AudioController.Instance.pawnkillClip);
                        currentPawnController.canPlayAgain = true;
                    }
                }
            }

            if (!wait) {
                CheckForFinish();
            }
        }

        public Dictionary<PawnController, (int[], int[], int[])> GetAllPlayerPawnPositions()
        {
            Dictionary<PawnController, (int[], int[], int[])> playerPawnData = new Dictionary<PawnController, (int[], int[], int[])>();

            foreach (PawnController player in activePawnControllers)
            {
                int[] pawnPositions = new int[4];
                int[] moveCounts = new int[4];
                int[] scores = new int[4];

                //System.Text.StringBuilder debugInfo = new System.Text.StringBuilder();
                //debugInfo.Append($"[{player.pawnColor}] : {{");

                for (int i = 0; i < player.pawns.Length; i++)
                {
                    Pawn pawn = player.pawns[i];

                    if (!pawn.inBase && !pawn.isCollected)
                    {
                        pawnPositions[i] = pawn.currentWayID; 
                        moveCounts[i] = pawn.moveCount;
                        scores[i] = pawn.score;
                    }
                    else
                    {
                        pawnPositions[i] = -1; 
                        moveCounts[i] = 0;
                        scores[i] = 0;
                    }


                    //debugInfo.Append($"[Pawn{i + 1} - pos: {pawnPositions[i]}, move: {moveCounts[i]}, score: {scores[i]}]");
                    //if (i < player.pawns.Length - 1)
                    //    debugInfo.Append(", ");

                }

                //debugInfo.Append(" }");
                //Debug.Log(debugInfo.ToString());
                playerPawnData[player] = (pawnPositions, moveCounts, scores);
            }

            return playerPawnData;
        }

        public void CheckForFinish(string color = "") {
            if (!string.IsNullOrEmpty(color)) {
                winnerColor = color;
                AudioController.Instance.PlaySFX(AudioController.Instance.pawnkillClip);
            } else {
                winnerColor = isSomeoneFinished();
            }

            if (!string.IsNullOrEmpty(winnerColor)) {
                ChangeGameState(GameState.FINISHED);
            } else {
                if (currentPawnController.canPlayAgain) {
                    currentPawnController.time = 10;
                    ChangeGameState(GameState.READY);
                    if (currentPawnController != myPawnController && isLocal) {
                        currentPawnController.Play();
                    }
                    return;
                }

                ChangePlayer();
            }
        }

        public void ChangePlayer() {
            currentPawnController.profileTimeImg.fillAmount = 0;
            currentPawnController.time = 10;
            currentPawnController.canPlayAgain = false;
            currentPawnController.StopAnimation();

            if (isLocal) {
                currentPawnID = (currentPawnID + 1) % getPlayerCount();
                SetActivePawn();
                //Debug.Log("Current Pawn ID: " + currentPawnID);
            } else {
                if (photonView.IsMine && PhotonNetwork.IsMasterClient && currentPawnController == myPawnController) {
                    StartCoroutine(SwitchMasterDelay());
                    //Debug.Log("Switching Master Client Online");
                }
            }
        }


        IEnumerator SwitchMasterDelay() {
            //i change this line
            yield return new WaitForSecondsRealtime(1f);
            PhotonNetwork.SetMasterClient(PhotonNetwork.MasterClient.GetNext());
        }

        public void MasterClientChanged() {
            SetActivePawn();
        }

        public async void GameDiceBtn(string color)
        {
            if (currentPawnController.pawnColor != color) return;
            if (gameState != GameState.READY) return;
            if (isRolling) return; // Prevent multiple clicks
            if (!isLocal && !PhotonNetwork.IsMasterClient) return;
            AudioController.Instance.PlaySFX(AudioController.Instance.diceClip);

            isRolling = true; // Lock the button while rolling

            GameObject selectedDice = null;
            switch (color.ToLower())
            {
                case "green":
                    selectedDice = Gdice;
                    break;
                case "yellow":
                    selectedDice = Ydice;
                    break;
                case "blue":
                    selectedDice = Bdice;
                    break;
                case "red":
                    selectedDice = Rdice;
                    break;
            }
            if (selectedDice != null && isRolling)
            {
                diceResult = await rotateDice.StartRoll(selectedDice);

                isRolling = false; // Unlock after checking movement
                //Debug.Log("diceresult : " + diceResult);
            }
            else
            {
                //Debug.LogError($"[GameDiceBtn] No dice assigned for color: {color}");
            }

            gameState = GameState.DICE;
            currentDice = diceResult - 1;

            if (isLocal)
            {
                LeanTween.value(0, 1, 0.5f).setOnComplete(() => {
                    currentPawnController.CheckAvailableMovements(currentDice == 5);
                    isRolling = false; // Unlock after checking movement
                });
            }
            else
            {
                photonView.RPC("RPCDice", RpcTarget.AllBuffered, currentDice);
                isRolling = false; // Unlock after sending RPC
            }
        }

        [PunRPC]
        void RPCDice(int arg)
        {
            currentDice = arg;
            //currentPawnController.PlayDiceAnimation();

            if (photonView.IsMine)
            {
                currentPawnController.CheckAvailableMovements(currentDice == 5);
            }
        }

        public void CheckRoomPlayers(Player leftPlayer) {
            if (gameState == GameState.FINISHED) return;

            int colorID = (int)leftPlayer.CustomProperties["colorID"];

            if (colorID == 0) {
                if (activePawnControllers.Contains(greenPawn)) {
                    activePawnControllers.Remove(greenPawn);
                }
                greenPawn.DisablePawn();
            } else if (colorID == 1) {
                if (activePawnControllers.Contains(yellowPawn)) {
                    activePawnControllers.Remove(yellowPawn);
                }
                yellowPawn.DisablePawn();
            } else if (colorID == 2) {
                if (activePawnControllers.Contains(bluePawn)) {
                    activePawnControllers.Remove(bluePawn);
                }
                bluePawn.DisablePawn();
            } else if (colorID == 3) {
                if (activePawnControllers.Contains(redPawn)) {
                    activePawnControllers.Remove(redPawn);
                }
                redPawn.DisablePawn();
            }

            if (PhotonNetwork.PlayerList.Length == 1) {
                winnerColor = myPlayerColor;
                ChangeGameState(GameState.FINISHED);
            }
        }

        //public void FinishedShow() {
        //    if (pauseScreen.activeInHierarchy) {
        //        pauseScreen.SetActive(false);
        //    }

        //    List<GameObject> activePlayerForPanel = new List<GameObject>();
        //    PhotonNetwork.AutomaticallySyncScene = false;

        //    finishedPanel.transform.localScale = Vector3.zero;
        //    finishedScreen.SetActive(true);

        //    GameObject winnerObject = null;

        //    foreach (PawnController p in activePawnControllers) {
        //        if (p.pawnColor == "Green") {
        //            activePlayerForPanel.Add(finishedPlayersParent.GetChild(0).gameObject);
        //            finishedPlayersParent.GetChild(0).gameObject.SetActive(true);
        //            finishedPlayersParent.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = greenPawn.usernameText.text;
        //            finishedPlayersParent.GetChild(0).GetChild(0).GetComponent<Image>().sprite = greenPawn.avatarImg.sprite;

        //            if (winnerColor == p.pawnColor) {
        //                winnerObject = finishedPlayersParent.GetChild(0).gameObject;
        //                LeanTween.scale(finishedPlayersParent.GetChild(0).gameObject, new Vector3(1.05f, 1.05f, 1.05f), 0.3f).setLoopPingPong();
        //            }
        //        } else if (p.pawnColor == "Yellow") {
        //            activePlayerForPanel.Add(finishedPlayersParent.GetChild(2).gameObject);
        //            finishedPlayersParent.GetChild(2).gameObject.SetActive(true);
        //            finishedPlayersParent.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = yellowPawn.usernameText.text;
        //            finishedPlayersParent.GetChild(2).GetChild(0).GetComponent<Image>().sprite = yellowPawn.avatarImg.sprite;

        //            if (winnerColor == p.pawnColor) {
        //                winnerObject = finishedPlayersParent.GetChild(2).gameObject;
        //                LeanTween.scale(finishedPlayersParent.GetChild(2).gameObject, new Vector3(1.05f, 1.05f, 1.05f), 0.3f).setLoopPingPong();
        //            }
        //        } else if (p.pawnColor == "Blue") {
        //            activePlayerForPanel.Add(finishedPlayersParent.GetChild(1).gameObject);
        //            finishedPlayersParent.GetChild(1).gameObject.SetActive(true);
        //            finishedPlayersParent.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = bluePawn.usernameText.text;
        //            finishedPlayersParent.GetChild(1).GetChild(0).GetComponent<Image>().sprite = bluePawn.avatarImg.sprite;

        //            if (winnerColor == p.pawnColor) {
        //                winnerObject = finishedPlayersParent.GetChild(1).gameObject;
        //                LeanTween.scale(finishedPlayersParent.GetChild(1).gameObject, new Vector3(1.05f, 1.05f, 1.05f), 0.3f).setLoopPingPong();
        //            }
        //        } else if (p.pawnColor == "Red") {
        //            activePlayerForPanel.Add(finishedPlayersParent.GetChild(3).gameObject);
        //            finishedPlayersParent.GetChild(3).gameObject.SetActive(true);
        //            finishedPlayersParent.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = redPawn.usernameText.text;
        //            finishedPlayersParent.GetChild(3).GetChild(0).GetComponent<Image>().sprite = redPawn.avatarImg.sprite;

        //            if (winnerColor == p.pawnColor) {
        //                winnerObject = finishedPlayersParent.GetChild(3).gameObject;
        //                LeanTween.scale(finishedPlayersParent.GetChild(3).gameObject, new Vector3(1.05f, 1.05f, 1.05f), 0.3f).setLoopPingPong();
        //            }
        //        }
        //    }

        //    LeanTween.scale(finishedPanel, Vector3.one, 0.2f).setEaseOutBack().setOnStart(() => {
        //        for (int i = 0; i < activePlayerForPanel.Count; i++) {
        //            GameObject g = activePlayerForPanel[i];
        //            LeanTween.alphaCanvas(g.GetComponent<CanvasGroup>(), 1, 0.5f).setDelay(i * 0.25f);
        //        }
        //    });

        //    int gamePrice = ConnectRoom.Instance.gameEntryPrice();
        //    int winnerPrice = 50;

        //    winnerPrice = PlayerPrefs.GetInt("playerCount") * gamePrice;
        //    if (winnerColor == myPlayerColor) {
        //        PlayerPrefs.SetInt("gem", PlayerPrefs.GetInt("gem") + winnerPrice);
        //        PlayerPrefs.Save();
        //    }

        //    for (int i = 0; i < activePlayerForPanel.Count; i++) {
        //        GameObject g = activePlayerForPanel[i];
        //        Transform txt = g.transform.Find("gem");

        //        txt.GetComponent<TextMeshProUGUI>().text = gamePrice.ToString("###,###,###");

        //        if (g == winnerObject) {
        //            LeanTween.value(gamePrice, winnerPrice, 2f).setOnUpdate((float var) => {
        //                txt.GetComponent<TextMeshProUGUI>().text = var.ToString("###,###");
        //            });
        //        } else {
        //            LeanTween.value(gamePrice, 0, 2f).setOnUpdate((float var) => {
        //                txt.GetComponent<TextMeshProUGUI>().text = var.ToString("###,###");
        //            }).setOnComplete(() => {
        //                txt.GetComponent<TextMeshProUGUI>().text = "0";
        //            });
        //        }
        //    }
        //}

        public void FinishedShow()
        {
            if (pauseScreen.activeInHierarchy)
            {
                pauseScreen.SetActive(false);
            }

            List<GameObject> activePlayerForPanel = new List<GameObject>();
            PhotonNetwork.AutomaticallySyncScene = false;

            finishedPanel.transform.localScale = Vector3.zero;
            finishedScreen.SetActive(true);

            GameObject winnerObject = null;

            // Activate and set up only the winner's panel based on winnerColor
            switch (winnerColor)
            {
                case "Green":
                    GameObject greenPanel = finishedPlayersParent.GetChild(0).gameObject;
                    greenPanel.SetActive(true);
                    greenPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = greenPawn.usernameText.text;
                    greenPanel.transform.GetChild(0).GetComponent<Image>().sprite = greenPawn.avatarImg.sprite;
                    activePlayerForPanel.Add(greenPanel);
                    winnerObject = greenPanel;
                    LeanTween.scale(greenPanel, new Vector3(1.05f, 1.05f, 1.05f), 0.3f).setLoopPingPong();
                    break;
                case "Yellow":
                    GameObject yellowPanel = finishedPlayersParent.GetChild(2).gameObject;
                    yellowPanel.SetActive(true);
                    yellowPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = yellowPawn.usernameText.text;
                    yellowPanel.transform.GetChild(0).GetComponent<Image>().sprite = yellowPawn.avatarImg.sprite;
                    activePlayerForPanel.Add(yellowPanel);
                    winnerObject = yellowPanel;
                    LeanTween.scale(yellowPanel, new Vector3(1.05f, 1.05f, 1.05f), 0.3f).setLoopPingPong();
                    break;
                case "Blue":
                    GameObject bluePanel = finishedPlayersParent.GetChild(1).gameObject;
                    bluePanel.SetActive(true);
                    bluePanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = bluePawn.usernameText.text;
                    bluePanel.transform.GetChild(0).GetComponent<Image>().sprite = bluePawn.avatarImg.sprite;
                    activePlayerForPanel.Add(bluePanel);
                    winnerObject = bluePanel;
                    LeanTween.scale(bluePanel, new Vector3(1.05f, 1.05f, 1.05f), 0.3f).setLoopPingPong();
                    break;
                case "Red":
                    GameObject redPanel = finishedPlayersParent.GetChild(3).gameObject;
                    redPanel.SetActive(true);
                    redPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = redPawn.usernameText.text;
                    redPanel.transform.GetChild(0).GetComponent<Image>().sprite = redPawn.avatarImg.sprite;
                    activePlayerForPanel.Add(redPanel);
                    winnerObject = redPanel;
                    LeanTween.scale(redPanel, new Vector3(1.05f, 1.05f, 1.05f), 0.3f).setLoopPingPong();
                    break;
            }

            LeanTween.scale(finishedPanel, Vector3.one, 0.2f).setEaseOutBack().setOnStart(() => {
                // Only the winner's panel will fade in
                foreach (GameObject g in activePlayerForPanel)
                {
                    LeanTween.alphaCanvas(g.GetComponent<CanvasGroup>(), 1, 0.5f);
                }
            });

            int gamePrice = ConnectRoom.Instance.gameEntryPrice();
            int winnerPrice = PlayerPrefs.GetInt("playerCount") * gamePrice;

            if (winnerColor == myPlayerColor)
            {
                PlayerPrefs.SetInt("gem", PlayerPrefs.GetInt("gem") + winnerPrice);

                // Check if "totalwin" key exists
                if (PlayerPrefs.HasKey("totalwin"))
                {
                    PlayerPrefs.SetInt("totalwin", PlayerPrefs.GetInt("totalwin") + 1);
                }
                else
                {
                    PlayerPrefs.SetInt("totalwin", 1);
                }
                PlayerPrefs.Save();
            }

            foreach (GameObject g in activePlayerForPanel)
            {
                Transform txt = g.transform.Find("Gem");
                txt.GetComponent<TextMeshProUGUI>().text = gamePrice.ToString("###,###,###");

                LeanTween.value(gamePrice, winnerPrice, 2f).setOnUpdate((float var) => {
                    txt.GetComponent<TextMeshProUGUI>().text = var.ToString("###,###");
                });
            }
        }

        public void FinishedMenuBtn() {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.Disconnect();
            LeanTween.cancelAll();
            SceneManager.LoadScene("Menu");
        }
    }
}