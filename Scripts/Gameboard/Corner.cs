using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corner: MonoBehaviour{

    public PlayerColors currentColor;
    [HideInInspector] public bool placedSecond;

    [HideInInspector] public bool hasCity;
    [HideInInspector] public bool hasSettlement;

    [HideInInspector] public List<Tile> adjacentTiles;
    [HideInInspector] public List<Corner> adjacentCorners;
    [HideInInspector] public List<Edge> adjacentEdges;

    public int myRevenueOfSettlement;
    public float myAttractiveness;
    public int degreeOfCompetition;
    public int distanceAway;
    [HideInInspector] public bool cornerChecked;

    public Material defaultMat;

    void Awake() {
        defaultMat = GetComponent<MeshRenderer>().material;
    }

    public void SetCorner(GameController g)
    {
        hasCity = false;
        hasSettlement = false;
        currentColor = PlayerColors.None;
        //Debug.Log("Setting color to none " + currentColor);
    }

    public bool HasConstruct() {
        return hasCity || hasSettlement;
    }

    public bool IsOwnedByPlayer(Player player) {
        return currentColor == player.myColor;
    }

    public bool CanBeBuiltOn(Player player) {
        return !HasConstruct() && HasLeadingRoads(player) && !HasNeighboringConstruct();
    }

    public bool HasNeighboringConstruct() {
        return adjacentCorners.Find((Corner corn)=>{ return corn.hasSettlement || corn.hasCity; }) != null;
    }

    public bool HasLeadingRoads(Player player) {
        return adjacentEdges.Find((Edge ed)=>{
            return ed.hasRoad && ed.currentColor == player.myColor;
        }) != null;
    }

    // public void OnMouseDown()
    // {
    //     if (GameController.gc.GetCurrentPlayer().isAI) return;

    //     PlaceCorner();
    // }

    public bool PlaceCorner()
    {
        if (hasCity) return false;

        bool canPlace = true;
        for (int i = 0; i < adjacentCorners.Count; i++)
        {
            if (adjacentCorners[i].hasCity || adjacentCorners[i].hasSettlement) canPlace = false;
        }

        Material m = null;
        if (canPlace)
        {
            m = GameController.gc.CornerClicked(this);
            if (m != null)
            {
                if (hasCity)
                {
                    GameController.gc.GetCurrentPlayer().numberOfSettlementsLeft++;
                    GameController.gc.GetCurrentPlayer().numberOfCitiesLeft--;
                    GameController.gc.GetCurrentPlayer().playerVictoryPoints++;
                }
                else
                {
                    currentColor = GameController.gc.GetCurrentPlayer().myColor;
                    GetComponent<MeshRenderer>().sharedMaterial = m;
                    GameController.gc.GetCurrentPlayer().numberOfSettlementsLeft--;
                    GameController.gc.GetCurrentPlayer().myCorners.Add(this);
                    GameController.gc.GetCurrentPlayer().playerVictoryPoints++;
                }

                defaultMat = m;
            }

            GameController.gc.UpdatePlayerUIs();
        }

        return canPlace && m != null;
    }

    public bool IsOccupied()
    {
        if (hasSettlement || hasCity) return true;
        else return false;
    }

    public bool CornerIsCurrentColor()
    {
        return currentColor == GameController.gc.GetCurrentPlayer().myColor;
    }
}
