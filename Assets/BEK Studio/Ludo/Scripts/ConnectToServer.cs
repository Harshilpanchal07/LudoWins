using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Photon.Pun;
using Photon.Realtime;
using BEKStudio;
using ExitGames.Client.Photon; // Required for CustomProperties

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public static ConnectToServer Instance;
    public event Action OnLobbyJoined;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //Keep this script alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Connect()
    {
        if(!NetworkCheck.IsNetworkAvailable())
        {
            NoticeManager.Show("No internet connection", true);
            return;
        }
        else { 
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.NickName = PlayerPrefs.GetString("username");
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Succesfully Joined Lobby");
        OnLobbyJoined?.Invoke(); // Call event when lobby is joined
    }
}
