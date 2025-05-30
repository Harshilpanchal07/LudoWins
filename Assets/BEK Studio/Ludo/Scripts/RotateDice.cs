using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BEKStudio;
using Photon.Pun;
using System.Linq;

public class RotateDice : MonoBehaviourPun
{
    // Animation settings for fine-tuning the dice roll feel
    public float rotationDuration = 1f;
    public float smoothTransitionDuration = 0.8f;
    public float rotationSpeed = 80f;
    public int diceValue;

    // Internal state tracking
    private bool isRolling = false;


    // Delegate for communicating the result
    public delegate void DiceRollComplete(int result);
    public System.Action<int> OnDiceRollComplete;

    // Structure to hold valid dice face rotations and their corresponding values
    public class DiceFace
    {
        public Vector3 rotation;
        public int value;

        public DiceFace(Vector3 rot, int val)
        {
            rotation = rot;
            value = val;
        }
    }

    // List of all valid dice face rotations and their values
    private List<DiceFace> validFaces = new List<DiceFace>();

    void Start()
    {
        // For X-axis rotations (left-right)
        validFaces.Add(new DiceFace(new Vector3(0, 0, 0), 3));
        validFaces.Add(new DiceFace(new Vector3(90, 0, 0), 1));
        validFaces.Add(new DiceFace(new Vector3(180, 0, 0), 4));
        validFaces.Add(new DiceFace(new Vector3(270, 0, 0), 6));

        // For Y-axis rotations (forward-backward)
        validFaces.Add(new DiceFace(new Vector3(0, 90, 0), 5));
        validFaces.Add(new DiceFace(new Vector3(0, 270, 0), 2));
    }


    void Update()
    {
        // Handle touch input
        if (Input.touchCount > 0 && !isRolling)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
                {
                    // Call the async roll method
                    HandleDiceRollAsync(gameObject);
                }
            }
        }
    }
    private async void HandleDiceRollAsync(GameObject diceObject)
    {
        // Wait for the dice roll to complete and get the result
        int result = await StartRoll(diceObject);
    }
    public async Task<int> StartRoll(GameObject diceObject)
    {
        if (isRolling) return diceValue;
        diceValue = await RollDiceAsync(diceObject);
        return diceValue;
    }

    public void StartRotation(GameObject diceObject, int diceValue)
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonView dicePhotonView = diceObject.GetComponent<PhotonView>();

            if (dicePhotonView != null)
            {
                dicePhotonView.RPC("SyncDiceResult", RpcTarget.Others, diceValue, diceObject.transform.rotation);
                //Debug.Log("Dice Online Runned");
            }
            else
            {
                //Debug.LogError("No PhotonView found on dice!");
            }
        }
    }

    [PunRPC]
    public void SyncDiceResult(int diceValue, Quaternion rotation)
    {
        transform.rotation = rotation;
        Debug.Log("Synced Dice Value: " + diceValue);
    }

    void EnsureValidFaces()
    {
        if (validFaces.Count == 0)
        {
            //Reinitialize the list if it's empty
            validFaces.Add(new DiceFace(new Vector3(90, 0, 0), 1));
            validFaces.Add(new DiceFace(new Vector3(0, 270, 0), 2));
            validFaces.Add(new DiceFace(new Vector3(0, 0, 0), 3));
            validFaces.Add(new DiceFace(new Vector3(180, 0, 0), 4));
            validFaces.Add(new DiceFace(new Vector3(0, 90, 0), 5));
            validFaces.Add(new DiceFace(new Vector3(270, 0, 0), 6));
        }
    }

    private List<DiceFace> GetValidFaces()
    {
        List<DiceFace> faces = new List<DiceFace>
    {
        new DiceFace(new Vector3(90, 0, 0), 1),
        new DiceFace(new Vector3(0, 270, 0), 2),
        new DiceFace(new Vector3(0, 0, 0), 3),
        new DiceFace(new Vector3(180, 0, 0), 4),
        new DiceFace(new Vector3(0, 90, 0), 5),
        new DiceFace(new Vector3(270, 0, 0), 6),
    };

        return faces;
    }

    public DiceFace GetDiceFace()
    {
        List<DiceFace> validFaces = GetValidFaces();
        if (validFaces.Count == 0) return null;

        float randomChance = UnityEngine.Random.Range(0f, 1f);

        PawnController currentPlayer = GameController.Instance.currentPawnController;
        Dictionary<PawnController, (int[], int[], int[])> allPawnData = GameController.Instance.GetAllPlayerPawnPositions();

        if (randomChance < 0.4f)  // 40% chance for Attack/Defense Dice
        {
            if (ShouldUseNiceDice(currentPlayer, allPawnData))
            {
                return GetNiceDiceFace(validFaces, currentPlayer, allPawnData);
            }
            else if (ShouldUseBadDice(currentPlayer, allPawnData))
            {
                return GetBadDiceFace(validFaces, currentPlayer, allPawnData);
            }
        }
        return validFaces[Random.Range(0, validFaces.Count)]; // 60% completely random
    }

    private bool ShouldUseNiceDice(PawnController player, Dictionary<PawnController, (int[], int[], int[])> allPawnData)
    {
        int totalScore = player.pawns.Sum(p => p.score);
        int activePawns = player.pawns.Count(p => !p.inBase && !p.isCollected);

        // If player is struggling, they get a Nice Dice boost
        return (totalScore < 50 || activePawns <= 2);
    }

    private bool ShouldUseBadDice(PawnController player, Dictionary<PawnController, (int[], int[], int[])> allPawnData)
    {
        int totalScore = player.pawns.Sum(p => p.score);
        int activePawns = player.pawns.Count(p => !p.inBase && !p.isCollected);

        // If the player is winning (high score & active pawns), apply Bad Dice
        return (totalScore > 150 || activePawns == 3);
    }

    private DiceFace GetNiceDiceFace(List<DiceFace> validFaces, PawnController player, Dictionary<PawnController, (int[], int[], int[])> allPawnData)
    {
        List<DiceFace> attackDice = validFaces.Where(f => f.value == 6 || f.value == 5).ToList();
        List<DiceFace> defenseDice = validFaces.Where(f => f.value == 2 || f.value == 4).ToList();
        List<DiceFace> lowAttackDice = validFaces.Where(f => f.value == 1 || f.value == 2 || f.value == 3).ToList();

        bool nearOpponent = false;
        bool closeToHome = false;

        foreach (var otherPlayer in allPawnData.Keys)
        {
            if (otherPlayer == player) continue; // Skip self

            for (int i = 0; i < player.pawns.Length; i++)
            {
                if (allPawnData[player].Item1[i] == -1) continue; // Skip pawns in base

                for (int j = 0; j < otherPlayer.pawns.Length; j++)
                {
                    if (allPawnData[otherPlayer].Item1[j] == allPawnData[player].Item1[i])
                    {
                        nearOpponent = true;
                    }
                }

                if (allPawnData[player].Item1[i] >= 50) // Assuming home stretch starts at 50
                {
                    closeToHome = true;
                }
            }
        }

        if (nearOpponent)
        {
            return lowAttackDice.Count > 0 ? lowAttackDice[Random.Range(0, lowAttackDice.Count)] : validFaces[Random.Range(0, validFaces.Count)];
        }

        if (closeToHome)
        {
            return defenseDice.Count > 0 ? defenseDice[Random.Range(0, defenseDice.Count)] : validFaces[Random.Range(0, validFaces.Count)];
        }
        if (attackDice.Count > 0 && UnityEngine.Random.value > 0.5f) // 50% chance to avoid 6 & 5
        {
            return validFaces.Where(f => f.value != 6 && f.value != 5).ToList()[Random.Range(0, validFaces.Count - 2)];
        }

        return validFaces[Random.Range(0, validFaces.Count)];
    }

    private DiceFace GetBadDiceFace(List<DiceFace> validFaces, PawnController player, Dictionary<PawnController, (int[], int[], int[])> allPawnData)
    {
        List<DiceFace> badFaces = validFaces.Where(f => f.value == 1 || f.value == 2).ToList();
        List<DiceFace> lowFaces = validFaces.Where(f => f.value == 1 || f.value == 2 || f.value == 3).ToList();
        bool opponentNearby = false;

        // Check if any opponent's pawn is within 3 tiles of the player's pawn
        foreach (var otherPlayer in GameController.Instance.activePawnControllers)
        {
            if (otherPlayer == player) continue; // Skip self

            for (int i = 0; i < player.pawns.Length; i++)
            {
                if (player.pawns[i].inBase || player.pawns[i].isCollected) continue; // Ignore pawns in base/home

                int playerPosition = player.pawns[i].currentWayID;

                for (int j = 0; j < otherPlayer.pawns.Length; j++)
                {
                    if (otherPlayer.pawns[j].inBase || otherPlayer.pawns[j].isCollected) continue; // Ignore opponent's pawns in base

                    int opponentPosition = otherPlayer.pawns[j].currentWayID;

                    if (Mathf.Abs(playerPosition - opponentPosition) <= 3) // If opponent is within 3 tiles
                    {
                        opponentNearby = true;
                        break;
                    }
                }

                if (opponentNearby) break; // Stop checking further
            }
        }

        if (opponentNearby)
        {
            if (UnityEngine.Random.value < 0.8f)
            {
                return lowFaces.Count > 0 ? lowFaces[Random.Range(0, lowFaces.Count)] : validFaces[Random.Range(0, validFaces.Count)];
            }
        }

        return badFaces.Count > 0 ? badFaces[Random.Range(0, badFaces.Count)] : validFaces[Random.Range(0, validFaces.Count)];
    }



    private async Task<int> RollDiceAsync(GameObject diceObject)
    {
        isRolling = true;

        // Ensure validFaces is initialized before rolling
        EnsureValidFaces();

        if (validFaces.Count == 0)
        {
            Debug.LogError("No valid dice faces found! Ensure validFaces is initialized.");
            return -1; // Return an invalid value to avoid crashing
        }

        DiceFace targetFace = GetDiceFace();
        Quaternion startRotation = diceObject.transform.rotation;
        Vector3 originalScale = diceObject.transform.localScale;

        // Phase 1: Hyper-spin with visual feedback
        float elapsedTime = 0f;
        float turboSpeed = rotationSpeed * 2.5f; // 250% speed boost
        float spinDuration = rotationDuration * 0.4f; // 60% shorter duration

        while (elapsedTime < spinDuration)
        {
            // Aggressive multi-axis rotation with chaos factor
            float chaos = 1 + Mathf.PingPong(elapsedTime * 10f, 0.3f);
            float xRot = Mathf.Sin(elapsedTime * 15f) * turboSpeed * chaos;
            float zRot = Mathf.Cos(elapsedTime * 12f) * turboSpeed * chaos;
            float yRot = Mathf.Sin(elapsedTime * 18f) * turboSpeed * 0.8f;

            // Quick scale pump effect
            float scaleBoost = Mathf.Clamp01(elapsedTime * 5f) * 0.2f;
            diceObject.transform.localScale = originalScale * (1 + scaleBoost);

            diceObject.transform.Rotate(new Vector3(xRot, yRot, zRot) * Time.deltaTime, Space.World);
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        // Phase 2: Snap transition with impact effects
        Quaternion targetRotation = Quaternion.Euler(targetFace.rotation);
        float snapTime = 0f;
        float snapDuration = smoothTransitionDuration * 0.3f; // 70% faster transition

        // Pre-calculate rotation path for snappiness
        Vector3 rotationAxis = (Vector3.up + Vector3.right).normalized;
        float initialBoostAngle = 45f; // Overshoot angle

        while (snapTime < snapDuration)
        {
            float t = snapTime / snapDuration;

            // Explosive ease-out with overshoot
            float snapT = Mathf.Pow(t, 0.2f); // Fast initial movement
            float overshootT = Mathf.Clamp01(t * 1.8f); // 80% overshoot

            // Quick squash on impact
            float scaleT = Mathf.Sin(t * Mathf.PI) * 0.3f;
            diceObject.transform.localScale = originalScale * (1 - scaleT);

            // Rotation with initial overshoot
            Quaternion baseRot = Quaternion.Slerp(startRotation, targetRotation, snapT);
            Quaternion overshoot = Quaternion.AngleAxis(initialBoostAngle * (1 - overshootT), rotationAxis);
            diceObject.transform.rotation = baseRot * overshoot;

            snapTime += Time.deltaTime;
            await Task.Yield();
        }

        // Final snap with particle-like effect
        diceObject.transform.rotation = targetRotation;
        diceObject.transform.localScale = originalScale;

        // Add quick white flash (assuming you have a Renderer component)
        var renderer = diceObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.white;
            await Task.Delay(50); // 50ms flash
            renderer.material.color = originalColor;
        }

        isRolling = false;
        return targetFace.value;
    }

}