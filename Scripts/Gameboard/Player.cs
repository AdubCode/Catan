using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player{

    public PlayerColors myColor { get; set; }

    public int playerVictoryPoints;
    public bool usingTutorial;
    public int myUIPos;

    public int numberOfSettlementsLeft;
    public int numberOfRoadsLeft;
    public int numberOfCitiesLeft;

    public List<Card> myDevelopmentCards;
    public int[] myResourceCards { get; private set; }
    public int[] tradeValues;

    public List<Corner> myCorners;
    public List<Edge> myEdges;

    //stuff that cards call
    public int numberOfKnightsPlayed;
    public int numberOfFreeRoads;

    public bool largestArmy;
    public bool longestRoad;

    public int myLongestRoad;
    public int myVirtualLongestRoad;
    public int roadsNeededForVLR; //roads needed to make the virtual longest road
    public List<List<Edge>> roadLengths = new List<List<Edge>>();

    //ai stuff
    public bool isAI;
    public AIArchetype myAI;
    public int mySumOfRevenues;
    public float[] myResourceRevenues;
    public float[] myFrequencyFactor;
    public bool aiSympathetic;

    public PlayerStatsUI ui;


    public MobilePlayerController myMobileController;

    public Player(PlayerColors myCol)
    {
        playerVictoryPoints = 0;
        usingTutorial = true;
        myColor = myCol;
        myDevelopmentCards = new List<Card>();
        myResourceCards = new int[] {0,0,0,0,0};
        myResourceRevenues = new float[] {0,0,0,0,0};
        myFrequencyFactor = new float[] {1,1,1,1,1};
        tradeValues = new int[] {4,4,4,4,4};
        numberOfSettlementsLeft = 5;
        numberOfCitiesLeft = 4;
        numberOfRoadsLeft = 15;
        numberOfFreeRoads = 0;
        roadLengths.Add(new List<Edge>());
        roadLengths.Add(new List<Edge>());
        aiSympathetic = true;
        myCorners = new List<Corner>();
        myEdges = new List<Edge>();
    }

    public void UpdateStatsUI(){
        if (ui != null){
            ui.UpdateUI();
        }

        UpdateMobilePlayer();
    }

    public void UpdateMobilePlayer() {
        if (isAI || myMobileController == null) return;

        myMobileController.RpcSetCurrentPlayer(GameController.gc.GetCurrentPlayer() == this);

        for(int i = 0; i < myResourceCards.Length; i++){
            myMobileController.RpcSetResource(i, myResourceCards[i]);
        }

        for(int i = 0; i < tradeValues.Length; i++){
            myMobileController.RpcSetTradeValue(i, tradeValues[i]);
        }


        myMobileController.RpcSetRollStatus(GameController.gc.currentState == GameState.setup || GameController.gc.playerRolled);
    }

    public void UpdatePlayerColors(PlayerColors[] colors) {
        if (isAI || myMobileController == null) return;
        myMobileController.RpcSetActivePlayerColors(colors);
    }

    public void AddResource(ResourceType r, int amount)
    {
        if (r == ResourceType.none || !PlayTable.Unity.PTGameManager.singleton.IS_TABLETOP) return;
        myResourceCards[(int)r] += amount;
        this.UpdateStatsUI();
    }

    public bool HasResource(ResourceType r, int amount)
    {
        if (myResourceCards[(int)r] >= amount) return true;
        else return false;
    }

    public void RemoveResource(ResourceType r, int amount)
    {
        if (r == ResourceType.none || !PlayTable.Unity.PTGameManager.singleton.IS_TABLETOP) return;
        myResourceCards[(int)r] -= amount;
        this.UpdateStatsUI();
    }

    public int GetTotalNumberOfResources() {
        int count = 0;
        for(int i = 0; i < myResourceCards.Length; i++){
            count += myResourceCards[i];
        }

        return count;
    }

    public int NumberOfResource(ResourceType r)
    {
        return myResourceCards[(int)r];
    }

    public void AddDevelopmentCard(Card c)
    {
        if(!PlayTable.Unity.PTGameManager.singleton.IS_TABLETOP) return;

        myDevelopmentCards.Add(c);
        if (!isAI) myMobileController.RpcAddDevelopmentCard((int)c.myCardType, (int)c.myVPCardType);
        this.UpdateStatsUI();
    }

    public void RemoveDevelopmentCard(Card c)
    {
        if (!PlayTable.Unity.PTGameManager.singleton.IS_TABLETOP) return;

        if (!isAI) myMobileController.RpcRemoveDevelopmentCard(myDevelopmentCards.IndexOf(c));
        myDevelopmentCards.Remove(c);
        this.UpdateStatsUI();
    }

    public void SetLargestArmy(bool b)
    {
        if (largestArmy && !b) playerVictoryPoints -= 2;
        else if (b) playerVictoryPoints += 2;

        largestArmy = b;
        this.UpdateStatsUI();
    }

    public void SetLongestRoad(bool b)
    {
        if (longestRoad && !b) playerVictoryPoints -= 2;
        else if (b) playerVictoryPoints += 2;

        longestRoad = b;
        this.UpdateStatsUI();
    }
}
