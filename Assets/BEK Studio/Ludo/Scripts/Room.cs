using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    public TMP_Text RoomName;

    public void Joinroom()
    {
        ConnectRoom.Instance.JoinRoominList(RoomName.text);
    }
}
