using BEKStudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoticePrefab : MonoBehaviour
{
    public TMP_Text messageText;

    public void Setup(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    public void ClosePopup()
    {
        AudioController.Instance.PlaySFX(AudioController.Instance.CloseClip);
        Destroy(gameObject);
    }
}
