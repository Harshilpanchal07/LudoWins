using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace BEKStudio {
    public class Pawn : MonoBehaviourPun, IPunObservable {
        public PawnController pawnController; 
        public int firstWayID;
        public bool inBase;
        public bool inColorWay;
        public bool isProtected;
        public bool isCollected;
        public int currentWayID;
        public int moveCount;
        public int score;
        public bool isKilled;
        public Vector2 startScale;
        public Vector2 startPosition;

        // by me
        public static List<Pawn> AllPawns = new List<Pawn>();



        void Start() {
            inBase = true;
            startScale = transform.localScale;
            startPosition = transform.position;
            if (!AllPawns.Contains(this))
            {
                AllPawns.Add(this);
            }
        }

        public void SetScaleToDefault() {
            transform.localScale = startScale;
        }

        public void Move(int count) {
            if (isCollected) return;
            if (!GameController.Instance.isLocal && !photonView.IsMine) return;
            if (inBase) {
                inBase = false;
                isProtected = true;
                currentWayID = firstWayID;
                LeanTween.move(gameObject, GameController.Instance.waypointParent.GetChild(firstWayID).position, 0.3f).setOnComplete(() => {
                    AudioController.Instance.PlaySFX(AudioController.Instance.pawnMoveClip);
                    GameController.Instance.CheckGameStatus();
                    TileManager.Instance.UpdatePawnPositions();
                });
                return;
            }

            StartCoroutine(MoveCoroutine(moveCount + count));
        }

        IEnumerator MoveCoroutine(int totalCount)
        {
            if (photonView.IsMine || GameController.Instance.isLocal)
            {
                bool canMove = false;
                while (moveCount != totalCount)
                {
                    if (!canMove)
                    {
                        canMove = true;
                        currentWayID = (currentWayID + 1) % GameController.Instance.waypointParent.childCount;
                        if (moveCount < 50)
                        {
                            LeanTween.move(gameObject, GameController.Instance.waypointParent.GetChild(currentWayID).position, 0.1f).setDelay(0.05f).setOnComplete(() =>
                            {
                                AudioController.Instance.PlaySFX(AudioController.Instance.pawnMoveClip);
                                moveCount++;
                                score = moveCount * 10;
                                canMove = false;
                            });
                        }
                        else
                        {
                            inColorWay = true;
                            string[] parseName = gameObject.name.Split("-");
                            Transform colorWay = GameController.Instance.colorWayParent.Find(parseName[0]);
                            LeanTween.move(gameObject, colorWay.GetChild(moveCount - 50).position, 0.1f).setDelay(0.05f).setOnComplete(() =>
                            {
                                AudioController.Instance.PlaySFX(AudioController.Instance.pawnMoveClip);
                                moveCount++;
                                canMove = false;

                                if (moveCount == 56)
                                {
                                    isCollected = true;
                                    GetComponent<CircleCollider2D>().enabled = false;
                                    AudioController.Instance.PlaySFX(AudioController.Instance.winningClip);
                                    ParticleEffectManager.Instance.TriggerEffect(transform.position);
                                }
                            });
                        }
                    }
                    yield return null;
                }


                if (currentWayID == 2 || currentWayID == 10 || currentWayID == 15 || currentWayID == 23 || currentWayID == 28 || currentWayID == 36 || currentWayID == 41 || currentWayID == 49)
                {
                    isProtected = true;
                    AudioController.Instance.PlaySFX(AudioController.Instance.SafeClip);
                }
                else
                {
                    isProtected = false;
                }
                GameController.Instance.CheckGameStatus();
            }
            TileManager.Instance.UpdatePawnPositions();
        }

        public void ReturnToBase() {
            StartCoroutine(ReturnToBaseCoroutine());
        }

        IEnumerator ReturnToBaseCoroutine() {
            bool canMove = false;
            isKilled = true;
            while (!inBase) {
                if (!canMove) {
                    canMove = true;
                    if (currentWayID > firstWayID) {
                        currentWayID = (currentWayID - 1) % GameController.Instance.waypointParent.childCount;
                        LeanTween.move(gameObject, GameController.Instance.waypointParent.GetChild(currentWayID).position, 0.05f).setDelay(0.025f).setOnComplete(() => {
                            canMove = false;
                        });
                    } else if (currentWayID < firstWayID) {
                        currentWayID = (currentWayID + 1) % GameController.Instance.waypointParent.childCount;
                        LeanTween.move(gameObject, GameController.Instance.waypointParent.GetChild(currentWayID).position, 0.05f).setDelay(0.025f).setOnComplete(() => {
                            canMove = false;
                        });
                    } else {
                        LeanTween.move(gameObject, startPosition, 0.05f).setDelay(0.025f).setOnComplete(() => {
                            canMove = false;
                            inBase = true;
                            moveCount = 0;
                            score = 0;
                            GameController.Instance.CheckForFinish();
                        });
                    }
                }

                yield return null;
            }
            isKilled = false;
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            if (this == null || isKilled) return;
            LeanTween.move(gameObject, newPosition, 0.3f).setEase(LeanTweenType.easeOutQuad);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                stream.SendNext(inBase);
                stream.SendNext(inColorWay);
                stream.SendNext(isProtected);
                stream.SendNext(isCollected);
                stream.SendNext(currentWayID);
                stream.SendNext(moveCount);
            } else {
                inBase = (bool)stream.ReceiveNext();
                inColorWay = (bool)stream.ReceiveNext();
                isProtected = (bool)stream.ReceiveNext();
                isCollected = (bool)stream.ReceiveNext();
                currentWayID = (int)stream.ReceiveNext();
                moveCount = (int)stream.ReceiveNext();
            }
        }
    }
}