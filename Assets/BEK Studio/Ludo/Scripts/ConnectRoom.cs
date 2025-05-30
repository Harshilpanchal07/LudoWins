using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using BEKStudio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using ExitGames.Client.Photon; // Required for CustomProperties

public class ConnectRoom : MonoBehaviourPunCallbacks
{
    public static ConnectRoom Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    public Transform[] slotPositions;
    public GameObject playerSlotPrefab;
    private Dictionary<int, int> activeSlots = new Dictionary<int, int>();
    private Dictionary<int, GameObject> slotInstances = new Dictionary<int, GameObject>();
    private List<RoomInfo> availableRooms = new List<RoomInfo>();


    public string gameMode()
    {
        return PlayerPrefs.GetString("mode");
    }

    public int gameEntryPrice()
    {
        if (gameMode() == "online")
        {
            return Constants.ONLINE_ENTRY_PRICE;
        }
        else
        {
            return Constants.COMPUTER_ENTRY_PRICE;
        }
    }

    public void CreateRoom(string RoomName)
    {
        RoomOptions roomOptions = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 4,
            CleanupCacheOnLeave = true,
            EmptyRoomTtl = 0,
        };

        PhotonNetwork.CreateRoom(RoomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room Created: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Players in Room: " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public void JoinRoom(string joinRoom)
    {
        if (!string.IsNullOrEmpty(joinRoom))
        {
            if (availableRooms.Count > 0) // Check if there are rooms in the lobby
            {
                bool roomExists = availableRooms.Exists(room => room.Name == joinRoom);

                if (roomExists)
                {
                    PhotonNetwork.JoinRoom(joinRoom);
                }
                else
                {
                    NoticeManager.Show("Room does not exist. Please check the Room ID.", true);
                }
            }
            else
            {
                NoticeManager.Show("No rooms available in the lobby.", true);
            }
        }
        else
        {
            NoticeManager.Show("Join Room ID is empty!", true);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Players in Room: " + PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.AutomaticallySyncScene = true;
        MenuController.Instance.ShowRoom();
        MenuController.Instance.RoomID.text = PhotonNetwork.CurrentRoom.Name;

        // Store avatar index in player properties BEFORE assigning slots
        int avatarIndex = PlayerPrefs.GetInt("avatar");
        Hashtable playerProperties = new Hashtable();
        playerProperties["avatar"] = avatarIndex; 
        playerProperties["colorID"] = getMyOrder(); 
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            AssignPlayerToSlot(player);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("avatar"))
        {
            UpdateSlotAvatar(targetPlayer);
            Debug.Log("OnPlayerProperties update");
        }
    }

    private void UpdateSlotAvatar(Player targetPlayer)
    {
        if (activeSlots.TryGetValue(targetPlayer.ActorNumber, out int slotIndex))
        {
            if (slotInstances.TryGetValue(slotIndex, out GameObject instance))
            {
                Image avatarImage = instance.GetComponentInChildren<Image>();
                if (avatarImage != null && targetPlayer.CustomProperties.ContainsKey("avatar"))
                {
                    int newAvatarIndex = (int)targetPlayer.CustomProperties["avatar"];
                    avatarImage.sprite = MenuController.Instance.avatars[newAvatarIndex];
                    Debug.Log("UpdateSlotavatar");
                }
            }
        }
        else
        {
            AssignPlayerToSlot(targetPlayer);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // Wait until avatar & colorID properties are set on newPlayer
        if (newPlayer.CustomProperties.ContainsKey("avatar") && newPlayer.CustomProperties.ContainsKey("colorID"))
        {
            AssignPlayerToSlot(newPlayer);
            Debug.Log("On player entered Room");
        }
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (activeSlots.TryGetValue(otherPlayer.ActorNumber, out int slotIndex))
        {
            // Destroy the slot instance
            if (slotInstances.TryGetValue(slotIndex, out GameObject instance))
            {
                Destroy(instance);
                slotInstances.Remove(slotIndex);
            }

            activeSlots.Remove(otherPlayer.ActorNumber);

            GameObject icon = slotPositions[slotIndex].Find("icon")?.gameObject;
            if (icon != null)
            {
                icon.SetActive(true);
            }

            GameObject invite = slotPositions[slotIndex].Find("Invite")?.gameObject;
            if (invite != null)
            {
                invite.SetActive(true);
            }
        }
    }

    void AssignPlayerToSlot(Player player)
    {
        if (activeSlots.ContainsKey(player.ActorNumber))
        {
            Debug.LogWarning($"{player.NickName} already has a slot.");
            return;
        }

        // Find available slot
        for (int i = 0; i < slotPositions.Length; i++)
        {
            if (!activeSlots.ContainsValue(i))
            {
                activeSlots[player.ActorNumber] = i;
                Debug.Log($"Assigned {player.NickName} to slot {i}");

                // Remove existing instance if any
                if (slotInstances.TryGetValue(i, out GameObject existing))
                {
                    Destroy(existing);
                    slotInstances.Remove(i);
                }

                // Create new slot instance
                GameObject slotInstance = Instantiate(playerSlotPrefab, slotPositions[i]);
                slotInstances[i] = slotInstance;

                // Set up UI
                TextMeshProUGUI usernameText = slotInstance.GetComponentInChildren<TextMeshProUGUI>();
                Image avatarImage = slotInstance.GetComponentInChildren<Image>();
                usernameText.text = player.NickName;

                if (player.CustomProperties.TryGetValue("avatar", out object avatarIndex))
                {
                    avatarImage.sprite = MenuController.Instance.avatars[(int)avatarIndex];
                }

                // Deactivate icon
                Transform iconTransform = slotPositions[i].Find("icon");
                if (iconTransform != null)
                {
                    iconTransform.gameObject.SetActive(false);
                }

                // Deactivate Invite Button
                Transform inviteButtonTransform = slotPositions[i].Find("Invite");
                if (inviteButtonTransform != null)
                {
                    inviteButtonTransform.gameObject.SetActive(false);
                }

                //Log remaining slots
                List<int> remainingSlots = new List<int>();
                for (int j = 0; j < slotPositions.Length; j++)
                {
                    if (!activeSlots.ContainsValue(j))
                    {
                        remainingSlots.Add(j);
                    }
                }

                Debug.Log("Remaining available slots: " + string.Join(", ", remainingSlots));

                break;
            }
        }
    }

    int getMyOrder()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i] == PhotonNetwork.LocalPlayer)
            {
                Debug.Log("player order" + i);
                return i;
            }
        }
        return -1;
    }

    public void JoinRoominList(string Roomname)
    {
        PhotonNetwork.JoinRoom(Roomname);
    }

    public int Showplayercount()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (SceneManager.GetActiveScene().name == "Game")
        {
            GameController.Instance.MasterClientChanged();
            Debug.Log("Master client switched to: " + newMasterClient.NickName);
        }
    }

    public void Disconnect()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.RemovedFromList = true;
            PhotonNetwork.LeaveRoom();
            return;
        }
        PhotonNetwork.LeaveRoom();
    }


    // Called automatically when the room list is updated
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        availableRooms = roomList; // Update the list with available rooms
    }
}
