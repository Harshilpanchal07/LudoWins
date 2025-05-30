using System.Collections.Generic;
using BEKStudio;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void UpdatePawnPositions()
    {
        Pawn.AllPawns.RemoveAll(p => p == null);

        Dictionary<int, List<Pawn>> groups = new Dictionary<int, List<Pawn>>();
        int groupNumber = 1;

        foreach (Pawn pawn in Pawn.AllPawns)
        {
            if (pawn == null || pawn.inBase || pawn.isKilled)
                continue;

            Transform tileTransform;
            if (pawn.inColorWay)
            {
                string[] parseName = pawn.name.Split('-');
                Transform colorWay = GameController.Instance.colorWayParent.Find(parseName[0]);
                tileTransform = colorWay.GetChild(pawn.moveCount - 51);
            }
            else
            {
                tileTransform = GameController.Instance.waypointParent.GetChild(pawn.currentWayID);
            }

            int key = tileTransform.GetInstanceID();

            if (!groups.ContainsKey(key))
                groups[key] = new List<Pawn>();

            groups[key].Add(pawn);
        }

        foreach (var kvp in groups)
        {
            var group = kvp.Value;

            if (group.Count > 0)
            {
                string logMessage = $"Group {groupNumber++}: ";
                foreach (Pawn p in group)
                {
                    logMessage += $"{p.name} (";
                    if (p.inColorWay)
                        logMessage += $"ColorWay Pos: {p.transform.position}";
                    else
                        logMessage += $"WayID: {p.currentWayID}";
                    logMessage += "), ";
                }
                Debug.Log(logMessage);
            }

            if (group.Count > 1)
            {
                Debug.Log("Arranged Pawn");
                ArrangePawns(group);
            }
            else
            {
                Debug.Log("Reset Pawn");
                ResetPawn(group[0]);
            }
        }

        Debug.Log($"Pawn Updated - Total groups formed: {groupNumber - 1}");
    }

    private void ArrangePawns(List<Pawn> pawns)
    {
        int count = pawns.Count;
        float spacing = 0.1f;
        float scale = 1f;

        if (count == 2) scale = 0.85f;
        else if (count == 3) scale = 0.75f;
        else if (count >= 4) scale = 0.65f;

        //Vector3 center = pawns[0].transform.position;
        // Determine the tile's fixed position
        Vector3 center;
        if (pawns[0].inColorWay)
        {
            // Use the current position for colorway pawns
            //center = pawns[0].transform.position;
            string[] parseName = pawns[0].name.Split('-');
            Transform colorWay = GameController.Instance.colorWayParent.Find(parseName[0]);
            center = colorWay.GetChild(pawns[0].moveCount - 51).position;
        }
        else
        {
            // Use the waypoint position for regular pawns
            int wayID = pawns[0].currentWayID;
            center = GameController.Instance.waypointParent.GetChild(wayID).position;
        }
        float startOffset = -(count - 1) * spacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float xOffset = startOffset + (i * spacing);
            Vector3 newPos = center + new Vector3(xOffset, 0, 0);

            pawns[i].UpdatePosition(newPos);
            pawns[i].transform.localScale = pawns[i].startScale * scale;
        }
    }

    private void ResetPawn(Pawn pawn)
    {
        Vector3 targetPos;

        if (pawn.inColorWay)
        {
            //targetPos = pawn.transform.position;
            string[] parseName = pawn.name.Split('-');
            Transform colorWay = GameController.Instance.colorWayParent.Find(parseName[0]);
            targetPos = colorWay.GetChild(pawn.moveCount - 51).position;
        }
        else
        {
            targetPos = GameController.Instance.waypointParent.GetChild(pawn.currentWayID).position;
        }

        pawn.UpdatePosition(targetPos);
        pawn.transform.localScale = pawn.startScale;
    }
}








