using UnityEngine;
using TMPro;

public class NoticeManager : MonoBehaviour
{
    public static NoticeManager Instance;
    public GameObject noticePrefab;
    private GameObject currentNotice;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowNotice(string message, bool showokbutton)
    {
        if (noticePrefab == null)
        {
            Debug.LogWarning("Notice prefab not assigned!");
            return;
        }

        // Destroy any previous notice if still exists
        if (currentNotice != null)
        {
            Destroy(currentNotice);
        }

        Canvas mainCanvas = GameObject.Find("MainCanvas").GetComponent<Canvas>();
        currentNotice = Instantiate(noticePrefab, mainCanvas.transform);
        currentNotice.SetActive(true);

        GameObject NoticeText = GameObject.FindWithTag("Notice_text");
        GameObject OKButton = GameObject.FindWithTag("Notice_ok");

        if (OKButton != null)
        {
            OKButton.gameObject.SetActive(showokbutton);
        }
        else
        {
            Debug.LogWarning("OK Button not found in the Notice prefab!");
        }

        NoticePrefab popupScript = currentNotice.GetComponent<NoticePrefab>();
        popupScript.Setup(message);
    }

    public static void Show(string message, bool showokbutton)
    {
        if (Instance != null)
            Instance.ShowNotice(message, showokbutton);
        else
            Debug.LogWarning("NoticeManager not initialized yet.");
    }

    public static void Close()
    {
        if (Instance != null && Instance.currentNotice != null)
        {
            Destroy(Instance.currentNotice);
            Instance.currentNotice = null;
        }
    }
}

