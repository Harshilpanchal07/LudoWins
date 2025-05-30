using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using BEKStudio;

namespace BEKStudio {
    [System.Serializable]
    public struct BotMovements {
        public Pawn pawn;
        public int moveCount;

        public BotMovements(Pawn target, int count) {
            pawn = target;
            moveCount = count;
        }
    }

    public class PawnController : MonoBehaviour, IPunObservable {
        public Pawn[] pawns;
        public GameObject pawnParent;
        public GameObject profileParent;
        public Image profileDiceImg;
        public Image profileTimeImg;
        public Sprite[] diceSprites;
        public string pawnColor;
        public bool isBot;
        public bool canPlayAgain;
        public List<BotMovements> botMovements;
        public float time;
        public TextMeshPro usernameText;
        public Image avatarImg;

        void Start() {
            botMovements = new List<BotMovements>();
        }

        void Update() {
            if (!GameController.Instance.isLocal && !GameController.Instance.photonView.IsMine) return;

            if (GameController.Instance.currentPawnController == this) {
                if (GameController.Instance.gameState != GameController.GameState.FINISHED && GameController.Instance.gameState != GameController.GameState.MOVING && GameController.Instance.gameState != GameController.GameState.WAIT) {
                    if (time > 0) {
                        time -= 1 * Time.deltaTime;
                        profileTimeImg.fillAmount = time / 10;
                    } else {
                        canPlayAgain = false;
                        profileTimeImg.fillAmount = 0;
                        HighlightDices(false);
                        GameController.Instance.CheckGameStatus();
                    }
                } else {
                    profileTimeImg.fillAmount = 0;
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                stream.SendNext(time);
            } else {
                profileTimeImg.fillAmount = (float)stream.ReceiveNext() / 10;
            }
        }

        public void SetUserInfo(string name, int avatarID)
        {
            usernameText.text = name;
            avatarImg.sprite = GameController.Instance.avatars[avatarID];
        }

        public void SetUserInfo() {
            if (GameController.Instance.myPawnController != this) {
                usernameText.text = "Guest" + Random.Range(0, 99999);
                avatarImg.sprite = GameController.Instance.avatars[Random.Range(0, GameController.Instance.avatars.Length)];
            } else {
                usernameText.text = PlayerPrefs.GetString("username");
                avatarImg.sprite = GameController.Instance.avatars[PlayerPrefs.GetInt("avatar")];
            }
        }

        public void DisableColliders() {
            for (int i = 0; i < pawns.Length; i++) {
                pawns[i].GetComponent<CircleCollider2D>().enabled = false;
            }
        }

        public void HighlightDices(bool active) {
            for (int i = 0; i < pawns.Length; i++) {
                if (active) {
                    LeanTween.scale(pawns[i].gameObject, pawns[i].transform.localScale * 1.1f, 0.2f).setLoopPingPong();
                } else {
                    LeanTween.cancel(pawns[i].gameObject);
                    pawns[i].SetScaleToDefault();
                }
            }
        }

        public void CheckAvailableMovements(bool playAgain)
        {
            botMovements.Clear();
            canPlayAgain = playAgain;

            Pawn[] basePawns = pawns.Where(x => x.inBase).ToArray();
            int availablePawnCount = 0;

            if (basePawns.Length == 4 && GameController.Instance.currentDice != 5)
            {
                GameController.Instance.ChangePlayer();
                return;
            }

            // Collect all possible moves (for player visibility)
            for (int i = 0; i < pawns.Length; i++)
            {
                Pawn pawn = pawns[i];

                if (!pawn.isCollected)
                {
                    if (pawn.inBase)
                    {
                        if (GameController.Instance.currentDice == 5)
                        {
                            availablePawnCount++;
                            botMovements.Add(new BotMovements(pawn, GameController.Instance.currentDice + 1));
                            LeanTween.scale(pawn.gameObject, pawn.transform.localScale * 1.1f, 0.2f).setLoopPingPong();
                        }
                    }
                    else
                    {
                        int moveValue = GameController.Instance.currentDice + 1;
                        if (pawn.moveCount < 56 && pawn.moveCount + GameController.Instance.currentDice + 1 <= 56)
                        {
                            availablePawnCount++;
                            botMovements.Add(new BotMovements(pawn, GameController.Instance.currentDice + 1));
                            LeanTween.scale(pawn.gameObject, pawn.transform.localScale * 1.1f, 0.2f).setLoopPingPong();
                        }
                    }
                }
            }

            if (availablePawnCount > 0)
            {
                GameController.Instance.ChangeGameState(GameController.GameState.MOVE);

                if (isBot)
                {
                    // Bot decision-making logic
                    GameController.Instance.ChangeGameState(GameController.GameState.MOVING);
                    HighlightDices(false);

                    // Priority-based move selection
                    BotMovements selectedMove = GetPriorityMove();
                    selectedMove.pawn.Move(selectedMove.moveCount);
                    botMovements.Clear();
                }
            }
            else
            {
                GameController.Instance.ChangePlayer();
            }
        }

        private BotMovements GetPriorityMove()
        {
            int[] safetyPoints = { 2, 10, 15, 23, 28, 36, 41, 49 };
            var opponentPositions = GetOpponentPositions();

            List<BotMovements> captureMoves = new List<BotMovements>();
            List<BotMovements> safetyMoves = new List<BotMovements>();
            List<BotMovements> baseExitMoves = new List<BotMovements>();
            List<BotMovements> normalMoves = new List<BotMovements>();

            foreach (var move in botMovements)
            {
                // Skip colorway pawns for capture checks
                if (move.pawn.inColorWay)
                {
                    normalMoves.Add(move);
                    continue;
                }

                int newPosition = (move.pawn.currentWayID + move.moveCount) % GameController.Instance.waypointParent.childCount;
                bool isBaseExit = move.pawn.inBase;
                bool canCapture = CheckForOpponentAtWaypoint(newPosition);

                if (isBaseExit)
                {
                    baseExitMoves.Add(move);
                }
                else if (opponentPositions.Contains(newPosition))
                {
                    captureMoves.Add(move);
                    normalMoves.Add(move);
                }
                else if (safetyPoints.Contains(newPosition))
                {
                    safetyMoves.Add(move);
                    normalMoves.Add(move);
                }
                else
                {
                    normalMoves.Add(move);
                }
            }

            // New priority: Base Exit (if dice == 5 and pawn in base), then Capture, Safety, Normal
            if (GameController.Instance.currentDice == 5 && baseExitMoves.Count > 0)
            {
                return baseExitMoves.OrderByDescending(m => m.pawn.moveCount).First();
            }
            else if (captureMoves.Count > 0)
            {
                if (Random.value <= 0.6f)
                {
                    return normalMoves.OrderByDescending(m => m.pawn.moveCount).First();
                }
                else
                {
                    return captureMoves.OrderByDescending(m => m.pawn.moveCount).First();
                }
            }
            else if (safetyMoves.Count > 0)
            {
                return safetyMoves.OrderByDescending(m => m.pawn.moveCount).First();
            }
            else
            {
                return normalMoves.OrderByDescending(m => m.pawn.moveCount).First();
            }
        }

        private List<int> GetOpponentPositions()
        {
            var allPawnData = GameController.Instance.GetAllPlayerPawnPositions();
            List<int> positions = new List<int>();

            foreach (var data in allPawnData)
            {
                if (data.Key != this)
                {
                    positions.AddRange(data.Value.Item1.Where(pos => pos != -1));
                }
            }
            return positions;
        }

        private bool CheckForOpponentAtWaypoint(int waypointId)
        {
            foreach (PawnController player in GameController.Instance.activePawnControllers)
            {
                if (player == this) continue;

                foreach (Pawn pawn in player.pawns)
                {
                    if (!pawn.inBase &&
                        !pawn.inColorWay &&
                        pawn.currentWayID == waypointId &&
                        !pawn.isProtected)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void DisablePawn() {
            profileParent.SetActive(false);
            pawnParent.SetActive(false);
            gameObject.SetActive(false);
        }

        public void StartTimer(bool animation = false) {
            if (animation) {
                profileParent.LeanScale(Vector3.one * 1.05f, 2f).setLoopPingPong();
            }
            time = 10;
        }

        public void StopAnimation() {
            if (profileParent.LeanIsTweening()) {
                profileParent.LeanCancel();
            }
            profileParent.transform.localScale = Vector3.one;
        }

        public void Play() {
            GameController.Instance.GameDiceBtn(pawnColor);
        }
    }
}
