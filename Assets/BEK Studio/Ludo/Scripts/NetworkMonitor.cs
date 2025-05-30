using UnityEngine;
using System.Collections;

public class NetworkMonitor : MonoBehaviour
{
    private bool isShowingNotice = false;

    private void Start()
    {
        StartCoroutine(CheckNetworkLoop());
    }

    IEnumerator CheckNetworkLoop()
    {
        while (true)
        {
            if (!NetworkCheck.IsNetworkAvailable())
            {
                if (!isShowingNotice)
                {
                    NoticeManager.Show("No internet connection", false);
                    Debug.Log("Network not found");
                    isShowingNotice = true;
                }
            }
            else
            {
                if (isShowingNotice)
                {
                    Debug.Log("Network found");
                    NoticeManager.Close();
                    isShowingNotice = false;
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
}
