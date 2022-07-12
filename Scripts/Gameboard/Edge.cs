using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour {

    [HideInInspector] public PlayerColors currentColor;

    [HideInInspector] public List<Corner> adjacentCorners;
    [HideInInspector] public bool hasRoad;
    [HideInInspector] public bool isHarbor;
    [HideInInspector] public ResourceType harborType;
    [HideInInspector] public bool edgeChecked;

    public Material defaultMat;

    void Awake() {
        defaultMat = GetComponent<MeshRenderer>().material;
    }

    public void SetAdjacentCorners(Corner c1, Corner c2)
    {
        if(adjacentCorners == null) adjacentCorners = new List<Corner>();
        adjacentCorners.Add(c1);
        adjacentCorners.Add(c2);

        currentColor = PlayerColors.None;

        Vector3 line = new Vector3(c1.transform.position.x - c2.transform.position.x, 0, c1.transform.position.z - c2.transform.position.z);
        float rot = Mathf.Atan2(line.x,line.z);
        transform.eulerAngles = new Vector3(0,Mathf.Rad2Deg* rot + 90,0);

        if (!c1.adjacentEdges.Contains(this)) c1.adjacentEdges.Add(this);
        if (!c2.adjacentEdges.Contains(this)) c2.adjacentEdges.Add(this);
    }

    public bool PlaceEdge()
    {
        if (hasRoad) return false;

        Material m = GameController.gc.EdgeClicked(this);
        if (m != null)
        {
            hasRoad = true;
            int rand = Random.Range(0, SoundEffects.soundEffects["Build Road"].Count);
            AudioPool.PlayLoop(SoundEffects.soundEffects["Build Road"][rand], "Build Road", 1, 1, SoundEffects.soundEffects["Build Road"][rand].length, false);
            currentColor = GameController.gc.GetCurrentPlayer().myColor;
            GetComponent<MeshRenderer>().sharedMaterial = m;
            GameController.gc.GetCurrentPlayer().numberOfRoadsLeft--;
            GameController.gc.GetCurrentPlayer().myEdges.Add(this);
            if (GameController.gc.GetCurrentPlayer().numberOfRoadsLeft <= 10) GameController.gc.CalculateLongestRoad();

            GameController.gc.UpdatePlayerUIs();
            defaultMat = m;
        }

        return m != null;
    }
}
