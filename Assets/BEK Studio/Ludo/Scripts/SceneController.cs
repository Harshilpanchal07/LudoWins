using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Authentication;

public class SplashScreenManager : MonoBehaviour
{
    [SerializeField] private float duration = 2f;
    [SerializeField] private float moveDistance = 70f; // How much the object moves
    [SerializeField] private float moveDuration = 0.6f; // How long the movement takes
    [SerializeField] private GameObject dice1; // Assign the GameObject in Inspector
    [SerializeField] private GameObject dice2; // Assign the GameObject in Inspector

    private const string FirstTimeKey = "isFirstTime";

    private void Start()
    {
        MoveDownUp(dice1);
        MoveUpDown(dice2);
        Invoke(nameof(CheckFirstTimeLogin), duration);
    }

    private void CheckFirstTimeLogin()
    {
        if (!PlayerPrefs.HasKey(FirstTimeKey))
        {
            // First time opening the game
            Debug.Log("First time launch detected. Redirecting to Login.");
            PlayerPrefs.SetInt(FirstTimeKey, 1);
            PlayerPrefs.Save();
            SceneEffect.instance.SceneTransfer("Login");
        }
        else
        {
            // Not first time, go to Menu
            Debug.Log("Returning user. Redirecting to Menu.");
            SceneEffect.instance.SceneTransfer("Menu");
        }

        //if (!AuthenticationService.Instance.SessionTokenExists)
        //{
        //    // First time opening the game
        //    Debug.Log("First time launch detected. Redirecting to Login.");
        //    PlayerPrefs.SetInt(FirstTimeKey, 1);
        //    PlayerPrefs.Save();
        //    SceneEffect.instance.SceneTransfer("Login");
        //}
        //else
        //{
        //    // Not first time, go to Menu
        //    Debug.Log("Returning user. Redirecting to Menu.");
        //    SceneEffect.instance.SceneTransfer("Menu");
        //}
    }


    public void MoveUpDown(GameObject obj)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 upPos = startPos + Vector3.up * moveDistance;
        Vector3 downPos = startPos - Vector3.up * (moveDistance / 2);

        LTSeq sequence = LeanTween.sequence();
        sequence.append(LeanTween.moveY(obj, upPos.y, moveDuration).setEaseInOutQuad()); // Move Up
        sequence.append(LeanTween.moveY(obj, startPos.y, moveDuration).setEaseInOutQuad()); // Back to Origin
        sequence.append(LeanTween.moveY(obj, downPos.y, moveDuration).setEaseInOutQuad()); // Move Down (-25%)
        sequence.append(LeanTween.moveY(obj, startPos.y, moveDuration).setEaseInOutQuad()); // Back to Origin

        // Add a small delay before restarting to make it smoother
        sequence.append(moveDuration * 0.1f);

        // Restart the function for infinite looping
        sequence.append(() => MoveUpDown(obj));
    }


    public void MoveDownUp(GameObject obj)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 downPos = startPos - Vector3.up * moveDistance;
        Vector3 upPos = startPos + Vector3.up * (moveDistance / 2);

        LTSeq sequence = LeanTween.sequence();
        sequence.append(LeanTween.moveY(obj, downPos.y, moveDuration).setEaseInOutQuad()); // Move Down
        sequence.append(LeanTween.moveY(obj, startPos.y, moveDuration).setEaseInOutQuad()); // Back to Origin
        sequence.append(LeanTween.moveY(obj, upPos.y, moveDuration).setEaseInOutQuad()); // Move Up (Half)
        sequence.append(LeanTween.moveY(obj, startPos.y, moveDuration).setEaseInOutQuad()); // Back to Origin

        // Add a small delay before restarting to make it smoother
        sequence.append(moveDuration * 0.1f);

        // Restart the function for infinite looping
        sequence.append(() => MoveDownUp(obj));
    }
}


