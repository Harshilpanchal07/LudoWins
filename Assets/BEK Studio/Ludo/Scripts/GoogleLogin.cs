using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using System.Collections.Generic;
using Google;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoogleLogin : MonoBehaviour
{
    public Text statusText;
    public string webClientId = "161710815774-7jme9e9ejtdaa5qbsetk4gb4rc8rficb.apps.googleusercontent.com";

    private GoogleSignInConfiguration configuration;

    // Defer the configuration creation until Awake so the web Client ID
    // Can be set via the property inspector in the Editor.
    void Awake()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true,
            RequestEmail = true,
            UseGameSignIn = false
        };
    }

    public void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(
          OnAuthenticationFinished, TaskScheduler.Default);
    }

    internal async void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator =
                    task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                            (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.LogError("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.LogError("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.LogError("Canceled");
        }
        else
        {
            await SignInWithGoogleAsync(task.Result.IdToken);
            print("Session Id:" + AuthenticationService.Instance.SessionTokenExists);
            print("Access Id:" + AuthenticationService.Instance.AccessToken);
            print("Player Id:" + task.Result.IdToken);
            string OriginalplayerID = AuthenticationService.Instance.PlayerId;
            string player_id = UniqueId.GenerateShortID(OriginalplayerID);
            PlayerPrefs.SetString("playerID", player_id);
            PlayerPrefs.SetString("email", task.Result.Email);
            PlayerPrefs.SetString("loginmethod", "You are logined with google");
            PlayerPrefs.Save();
            CloudSave.instance.SaveDataAsString("email", task.Result.Email);
            CloudSave.instance.SaveDataAsString("Player ID", player_id);
            SceneManager.LoadScene("Menu");
        }
    }

    public void OnSignOut()
    {
        print("logout calling");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    async Task SignInWithGoogleAsync(string idToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGoogleAsync(idToken);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    void SimpleSignOut()
    {
        // The session token will be deleted immediately, allowing for a new anonymous player to be created
        AuthenticationService.Instance.SignOut(true);
    }
}
