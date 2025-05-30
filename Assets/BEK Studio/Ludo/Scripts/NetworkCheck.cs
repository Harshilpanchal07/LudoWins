using UnityEngine;
using System.Net.NetworkInformation;

public class NetworkCheck : MonoBehaviour
{
    public static bool IsNetworkAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}
