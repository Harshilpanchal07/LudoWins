using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AnonymousLogin : MonoBehaviour
{
    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    public async void SignIn()
    {
        await signInAnonymous();
    }

    async Task signInAnonymous()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            print("Sign in Success");
            print("Player Id:" + AuthenticationService.Instance.PlayerId);
            print("Session Id:" + AuthenticationService.Instance.SessionTokenExists);
            print("Access Id:" + AuthenticationService.Instance.AccessToken);
            string OriginalplayerID = AuthenticationService.Instance.PlayerId;
            string player_id = UniqueId.GenerateShortID(OriginalplayerID);
            PlayerPrefs.SetString("playerID", player_id);
            PlayerPrefs.SetString("loginmethod", "You are logined as a guest");
            PlayerPrefs.Save();
            CloudSave.instance.SaveDataAsString("Player ID", player_id);
            SceneManager.LoadScene("Menu");
        }
        catch (AuthenticationException ex)
        {
            print("Sign in failed!!");
            Debug.LogException(ex);
        }

    }

    public void SimpleSignOut()
    {
        // The session token will be deleted immediately, allowing for a new anonymous player to be created
        AuthenticationService.Instance.SignOut(true);
    }
}
