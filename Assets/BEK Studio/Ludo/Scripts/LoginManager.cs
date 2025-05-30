using BEKStudio;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public GameObject ComingSoonPanel;
    public GoogleLogin googleLogin;
    public AnonymousLogin anonymousLogin;

    private void Start()
    {
        AudioController.Instance.PlayMusic(AudioController.Instance.backgroundClip);
    }

    public void SignInWithFacebook()
    {
        AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
        if (!NetworkCheck.IsNetworkAvailable())
        {
            NoticeManager.Show("No internet connection", true);
            return;
        }
        else
        {
            ShowPanel(ComingSoonPanel);
            ComingSoonPanel.SetActive(true);
        }
    }


    public void SignInWithGoogle()
    {
        AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
        if (!NetworkCheck.IsNetworkAvailable())
        {
            NoticeManager.Show("No internet connection", true);
            return;
        }
        else
        {
            if (googleLogin != null)
            {
                googleLogin.OnSignIn();
            }
            else
            {
                Debug.LogError("GoogleLogin script not assigned in LoginManager!");
            }
        }
    }

    public void SignInAnonymously()
    {
        AudioController.Instance.PlaySFX(AudioController.Instance.ClickButtonClip);
        if (!NetworkCheck.IsNetworkAvailable())
        {
            NoticeManager.Show("No internet connection", true);
            return;
        }
        else
        {
            if (anonymousLogin != null)
            {
                anonymousLogin.SignIn();
            }
            else
            {
                Debug.LogError("AnonymousLogin script not assigned in LoginManager!");
            }
        }
    }


    public void CloseComingSoonPanel()
    {
        AudioController.Instance.PlaySFX(AudioController.Instance.CloseClip);
        HidePanel(ComingSoonPanel);
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
}

