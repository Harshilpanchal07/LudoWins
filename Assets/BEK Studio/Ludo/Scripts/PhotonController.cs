//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using Photon.Pun;
//using Photon.Realtime;

//namespace BEKStudio
//{
//    public class PhotonController : MonoBehaviourPunCallbacks
//    {
//        public static PhotonController Instance;
//        public int roomEntryPice = 0;
//        public int botAvatar;
//        public string botName;
//        private bool isConnecting = false;

//        void Awake()
//        {
//            if (Instance == null)
//            {
//                Instance = this;
//            }
//        }

//        public string gameMode()
//        {
//            return PlayerPrefs.GetString("mode");
//        }

//        public int gameEntryPrice()
//        {
//            if (gameMode() == "online")
//            {
//                return Constants.ONLINE_ENTRY_PRICE;
//            }
//            else
//            {
//                return Constants.COMPUTER_ENTRY_PRICE;
//            }
//        }

//        // call for create room 
//        public void Connect()
//        {
//            if (!isConnecting)
//            {
//                isConnecting = true;
//                StartCoroutine(WaitUntilConnectedAndCreateRoom());
//            }
//        }

//        private IEnumerator WaitUntilConnectedAndCreateRoom()
//        {
//            Debug.Log("Attempting to connect to Photon...");

//            PhotonNetwork.KeepAliveInBackground = 60;

//            // Start connecting if not already connected
//            if (!PhotonNetwork.IsConnected)
//            {
//                PhotonNetwork.ConnectUsingSettings();
//            }

//            // Keep checking until fully connected to Master Server
//            while (PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
//            {
//                Debug.Log("Waiting for Photon to connect...");
//                yield return new WaitForSeconds(1); // Prevents infinite loop from freezing game
//            }

//            Debug.Log("Connected to Master Server! Now creating room...");
//            CreateRoom();
//        }

//        public override void OnConnectedToMaster()
//        {
//            PhotonNetwork.JoinLobby();
//        }

//        public override void OnJoinedLobby()
//        {
//            Debug.Log("Joined Photon Lobby! Ready to create/join rooms.");
//        }

//        public void CreateRoom()
//        {
//            if (!PhotonNetwork.IsConnectedAndReady)
//            {
//                PhotonNetwork.ConnectUsingSettings();
//                StartCoroutine(WaitForConnectionAndCreateRoom());
//                return;
//            }
//            CreateRoomNow();
//        }

//        IEnumerator WaitForConnectionAndCreateRoom()
//        {
//            while (!PhotonNetwork.IsConnectedAndReady)
//            {
//                yield return null;
//            }
//            CreateRoomNow();
//        }

//        private void CreateRoomNow()
//        {
//            string roomID = GenerateShortRoomID();
//            int playerCount = PlayerPrefs.GetInt("playerCount");
//            Debug.Log("Creating room with ID: " + roomID + " and player count: " + playerCount);

//            RoomOptions roomOptions = new RoomOptions
//            {
//                MaxPlayers = (byte)playerCount,
//                IsVisible = true,
//                IsOpen = true,
//                CustomRoomProperties = new ExitGames.Client.Photon.Hashtable {
//                    { "playerCount", playerCount }
//                },
//                CustomRoomPropertiesForLobby = new string[] { "playerCount" }
//            };

//            MenuController.Instance.UpdateRoomIDUI(roomID);
//            PhotonNetwork.CreateRoom(roomID, roomOptions);
//        }

//        private string GenerateShortRoomID()
//        {
//            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
//            System.Random random = new System.Random();
//            char[] stringChars = new char[6];

//            for (int i = 0; i < stringChars.Length; i++)
//            {
//                stringChars[i] = chars[random.Next(chars.Length)];
//            }

//            return new string(stringChars);
//        }

//        public override void OnPlayerEnteredRoom(Player newPlayer)
//        {
//            MenuController.Instance.UpdatePlayerListUI();
//        }

//        public override void OnPlayerLeftRoom(Player otherPlayer)
//        {
//            MenuController.Instance.UpdatePlayerListUI();
//        }

//        public override void OnCreateRoomFailed(short returnCode, string message)
//        {
//            Debug.LogError("Room creation failed: " + message);
//        }

//        public override void OnJoinedRoom()
//        {
//            Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
//            PhotonNetwork.AutomaticallySyncScene = true;
//            PhotonNetwork.NickName = PlayerPrefs.GetString("username");


//            ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable {
//                { "avatar", PlayerPrefs.GetInt("avatar") },
//                { "isMaster", PhotonNetwork.IsMasterClient ? 1 : 0 },  // 1 for Host, 0 for others
//            };

//            PhotonNetwork.SetPlayerCustomProperties(playerProperties); 
//            MenuController.Instance.UpdatePlayerListUI();
//        }

//        public void JoinRoomByID(string roomID)
//        {
//            if (!PhotonNetwork.IsConnectedAndReady)
//            {
//                Debug.LogError("Not connected to Photon. Connecting...");
//                StartCoroutine(WaitForConnectionAndJoinRoom(roomID));
//                return;
//            }

//            Debug.Log("Trying to join room: " + roomID);
//            PhotonNetwork.JoinRoom(roomID);
//        }

//        // Coroutine to wait for connection before joining a room
//        IEnumerator WaitForConnectionAndJoinRoom(string roomID)
//        {
//            PhotonNetwork.ConnectUsingSettings();

//            while (!PhotonNetwork.IsConnectedAndReady)
//            {
//                yield return null;
//            }

//            PhotonNetwork.JoinRoom(roomID);
//        }

//        // Callback when room joining fails
//        public override void OnJoinRoomFailed(short returnCode, string message)
//        {
//            Debug.LogError("Failed to join room: " + message);
//        }

//        public void CloseRoom()
//        {
//            if (PhotonNetwork.IsMasterClient)
//            {
//                PhotonNetwork.CurrentRoom.IsOpen = false;
//                PhotonNetwork.CurrentRoom.IsVisible = false;
//                StartCoroutine(KickAllPlayersAndLeave());
//                Debug.Log("Room is now closed.");
//            }
//            else
//            {
//                Debug.LogWarning("Only the room host can close the room.");
//            }
//        }

//        // Function to exit the room
//        public void ExitRoom()
//        {
//            if (PhotonNetwork.InRoom)
//            {
//                PhotonNetwork.LeaveRoom();
//                Debug.Log("Leaving room...");
//            }
//            else
//            {
//                Debug.LogWarning("You're not in any room!");
//            }
//        }

//        IEnumerator KickAllPlayersAndLeave()
//        {
//            Player[] players = PhotonNetwork.PlayerListOthers;

//            foreach (Player player in players)
//            {
//                PhotonNetwork.CloseConnection(player);
//                yield return new WaitForSeconds(0.5f);
//            }

//            PhotonNetwork.LeaveRoom();
//        }

//        public override void OnLeftRoom()
//        {
//            Debug.Log("Left the room successfully!");
//            PhotonNetwork.Disconnect();
//            SceneManager.LoadScene("Menu");
//        }


//        public override void OnMasterClientSwitched(Player newMaster)
//        {
//            Debug.Log("New host assigned: " + newMaster.NickName);
//        }

//    }
//}

