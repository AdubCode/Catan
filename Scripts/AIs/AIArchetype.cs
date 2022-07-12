using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Level
{
    low,
    normal,
    high
}

public enum Strategy
{
    LongestRoad,
    Revenue,
    LargestArmy,
    Depends
}

public class AIArchetype {

    public string myName;
    protected string discription;
    protected string strategyType;

    protected List<Strategy> myStrategy;
    public List<float> startupResources;
    protected Level willingnessToTrade;
    protected Level risk;

    protected Dictionary<string, List<List<int>>> decisionList = new Dictionary<string, List<List<int>>>();

    public List<int> maximumToSpend = new List<int>();
    public List<int> differenceSum;
    protected List<List<int>> currentTable;
    public List<List<int>> currentBuildProjects;
    public Strategy currentStrategy;

    public virtual void SetUpAI() { }

    public virtual Dictionary<string, List<List<int>>> SetupTables() { return new Dictionary<string, List<List<int>>>(); }

    #region Robber things
    public virtual void MoveRobber()
    {
        //Situation B
        List<Player> players = new List<Player>();
        for(int i = 0; i < GameController.gc.players.Count; i++)
        {
            Player p = GameController.gc.players[i];
            if (p != GameController.gc.GetCurrentPlayer() && p.playerVictoryPoints >= 7 && (p.HasResource(ResourceType.Brick, 1) || p.HasResource(ResourceType.Grain, 1) || p.HasResource(ResourceType.Lumber, 1) || p.HasResource(ResourceType.Ore, 1) || p.HasResource(ResourceType.Wool, 1)))
            {
                players.Add(GameController.gc.players[i]);
            }
        }

        if(players.Count != 0)
        {
            players.Sort((x, y) => (y.playerVictoryPoints.CompareTo(x.playerVictoryPoints)));
            Debug.Log("Did Situation B player: " + players[0].myColor + " winning right now");
            if(players.Count > 1 && players[0].playerVictoryPoints == players[1].playerVictoryPoints)
            {
                if (!players[0].aiSympathetic) IndividualFactor(players[0]);
                else IndividualFactor(players[1]);
            }else
            {
                IndividualFactor(players[0]);
            }
            return;
        }

        for (int i = 0; i < GameController.gc.players.Count; i++)
        {
            Player p = GameController.gc.players[i];

            if(p.HasResource(ResourceType.Brick, 1) || p.HasResource(ResourceType.Grain, 1) || p.HasResource(ResourceType.Lumber,1) || p.HasResource(ResourceType.Ore,1) || p.HasResource(ResourceType.Wool,1)) players.Add(GameController.gc.players[i]);
        }

        players.Remove(GameController.gc.GetCurrentPlayer());
        if (players.Count != 0)
        {
            players.Sort((x, y) => (y.playerVictoryPoints.CompareTo(x.playerVictoryPoints)));

            if (players.Count >= 2 && players[0].playerVictoryPoints - 2 >= players[1].playerVictoryPoints)
            {
                Debug.Log("Did Situation B2 player: " + players[0].myColor + " is up by 2 from " + players[1].myColor);
                if (players.Count >= 1)
                {
                    IndividualFactor(players[0]);
                    return;
                }else
                {
                    Debug.Log("Not enough players to do Situation B2");
                }
            }
        }


        //situation A
        //todo check what that most important resource is and check if you can get it for more than 33% if not use general factor
        GeneralFactor();
    }

    void GeneralFactor()
    {
        List<Tile> checkTiles = new List<Tile>();

        for (int t = 0; t < GameController.gc.boardsTiles.Count; t++)
        {
            Tile tile = GameController.gc.boardsTiles[t];
            bool canAdd = true;
            for (int c = 0; c < tile.myCorners.Count; c++)
            {
                if (tile.myCorners[c].currentColor == GameController.gc.GetCurrentPlayer().myColor || tile.myResourceType == ResourceType.none || tile.hasRobber) canAdd = false;
            }

            if (canAdd) checkTiles.Add(tile);
        }

        for (int t = 0; t < checkTiles.Count; t++)
        {
            Tile tile = checkTiles[t];
            float gfrFactor = 1;

            int numberOfSettlements = 0;
            for (int c = 0; c < tile.myCorners.Count; c++)
            {
                if (tile.myCorners[c].hasCity) numberOfSettlements += 2;
                else if (tile.myCorners[c].hasSettlement) numberOfSettlements++;
            }

            if (GameController.gc.generalFrequencyFactor[(int)tile.myResourceType] < 10) gfrFactor = 2;
            else if (GameController.gc.generalFrequencyFactor[(int)tile.myResourceType] < 15) gfrFactor = 1.5f;

            tile.myDisturbanceFactor = tile.myProbabilityOfRevenue * gfrFactor * numberOfSettlements;
        }

        checkTiles.Sort((x, y) => (y.myDisturbanceFactor.CompareTo(x.myDisturbanceFactor)));
        List<PlayerColors> playersOnTile = new List<PlayerColors>();
        for(int i = 0; i < checkTiles[0].myCorners.Count; i++)
        {
            if ((checkTiles[0].myCorners[i].hasCity || checkTiles[0].myCorners[i].hasSettlement) && !playersOnTile.Contains(checkTiles[0].myCorners[i].currentColor)) playersOnTile.Add(checkTiles[0].myCorners[i].currentColor);
        }

        GameController.gc.StartCoroutine(MoveRobber(checkTiles[0].gameObject, playersOnTile[Random.Range(0,playersOnTile.Count)]));
    }

    void IndividualFactor(Player p)
    {
        List<Tile> checkTiles = new List<Tile>();
        for (int t = 0; t < GameController.gc.boardsTiles.Count; t++)
        {
            Tile tile = GameController.gc.boardsTiles[t];
            if (tile.myResourceType == ResourceType.none || tile.hasRobber) continue;
            float ifrFactor = 1;
            int numberOfSettlements = 0;
            for (int c = 0; c < tile.myCorners.Count; c++)
            {
                if (tile.myCorners[c].currentColor == p.myColor)
                {
                    if (tile.myCorners[c].hasCity) numberOfSettlements += 2;
                    else if (tile.myCorners[c].hasSettlement) numberOfSettlements++;
                }
            }

            if (p.myResourceRevenues[(int)tile.myResourceType] - tile.myProbabilityOfRevenue == 0) ifrFactor = 2.5f;
            else if (p.myFrequencyFactor[(int)tile.myResourceType] < 10) ifrFactor = 2;
            else if (p.myFrequencyFactor[(int)tile.myResourceType] < 15) ifrFactor = 1.5f;

            tile.myDisturbanceFactor = tile.myProbabilityOfRevenue * ifrFactor * numberOfSettlements;
            checkTiles.Add(tile);
        }

        checkTiles.Sort((x, y) => (y.myDisturbanceFactor.CompareTo(x.myDisturbanceFactor)));

        GameController.gc.StartCoroutine(MoveRobber(checkTiles[0].gameObject, p.myColor));
    }

    IEnumerator MoveRobber(GameObject tile, PlayerColors color) {
        GameController.gc.robber.MoveToTile(tile);
        yield return new WaitForSeconds(AI.TURN_WAIT_TIME);
        GameController.gc.TookResource(color);
    }

    #endregion

    #region Tradethings

    public virtual void CalculateTradeSum(Player player)
    {
        List<int> Difference = new List<int>() {0,0,0,0,0};
        maximumToSpend = new List<int>() {0,0,0,0,0};


        List<int>  BuildingProject = currentBuildProjects[0]; //FIRST AI BUILD PROJECT

        for (int i = 0; i < player.myResourceCards.Length; i++)
        {
            Difference[i] = (player.myResourceCards[i] - BuildingProject[i]) * 6;
            if ((Difference[i]/6) > 0) maximumToSpend[i] = Difference[i]/6;
            else maximumToSpend[i] = 0;
        }

        if(currentBuildProjects.Count >= 2) BuildingProject = currentBuildProjects[1]; //SECOND AI BUILD PROJECT

        for (int i = 0; i < player.myResourceCards.Length; i++)
        {
            Difference[i] += (player.myResourceCards[i] - BuildingProject[i]) * 2;
        }

        if(currentBuildProjects.Count >= 3) BuildingProject = currentBuildProjects[2]; //THIRD AI BUILD PROJECT

        for (int i = 0; i < player.myResourceCards.Length; i++)
        {
            Difference[i] += (player.myResourceCards[i] - BuildingProject[i]);
        }

        differenceSum = Difference;
        //Debug.Log("Difference Sum: " + differenceSum[0] + "," + differenceSum[1] + "," + differenceSum[2] + "," + differenceSum[3] + "," + differenceSum[4]);
    }

    public virtual void TryToTrade(Player currentPlayer)
    {
        CalculateTradeSum(currentPlayer);

        //calculate ambivalent resource

        bool playersCanTrade = false;
        for(int p = 0; p < GameController.gc.players.Count; p++)
        {
            Player player = GameController.gc.players[p];
            if (player == currentPlayer) continue;

            List<int> tempList = new List<int>();
            for (int i = 0; i < differenceSum.Count; i++) tempList.Add(differenceSum[i]);
            tempList.Sort((x,y) => x.CompareTo(y));
            if (player.myResourceCards[differenceSum.IndexOf(differenceSum.Min())] > 0 || player.myResourceCards[differenceSum.IndexOf(tempList[1])] > 0) playersCanTrade = true;
        }

        if (!differenceSum.Any(i => i < 0) || !maximumToSpend.Any(i => i > 0) || !currentPlayer.myResourceCards.Any(i => i > 2) || !playersCanTrade) return;

        ResourceType neededResource = (ResourceType)differenceSum.IndexOf(differenceSum.Min());

        //propose a 1:1 trade

        //propose 1:1 trade with second highest resource

        //propose a 2:1 trade from people

        //do a 2:1 trade if AI has port
        for(int i = 0; i < maximumToSpend.Count; i++)
        {
            if(maximumToSpend[i] != 0 && currentPlayer.tradeValues[i] == 2 && maximumToSpend[i] >= 2)
            {
                Debug.Log("Trading 2 " + (ResourceType)i + " for " + neededResource);
                GameController.gc.SetResourceInHand((ResourceType)i, GameController.gc.players.IndexOf(currentPlayer));
                GameController.gc.TradeBank(neededResource);
                return;
            }
        }

        //do 3:1 trade if have 3:1 port
        for (int i = 0; i < maximumToSpend.Count; i++)
        {
            if (maximumToSpend[i] != 0 && currentPlayer.tradeValues[i] == 3 && maximumToSpend[i] >= 3)
            {
                Debug.Log("Trading 3 " + (ResourceType)i + " for " + neededResource);
                GameController.gc.SetResourceInHand((ResourceType)i, GameController.gc.players.IndexOf(currentPlayer));
                GameController.gc.TradeBank(neededResource);
                return;
            }
        }

        //do 4:1 trade
        for (int i = 0; i < maximumToSpend.Count; i++)
        {
            if (maximumToSpend[i] != 0 && currentPlayer.tradeValues[i] == 4 && maximumToSpend[i] >= 4)
            {
                Debug.Log("Player " + (int)currentPlayer.myColor + " Trading 4 "  + (ResourceType)i + " for " + neededResource);
                GameController.gc.SetResourceInHand((ResourceType)i, GameController.gc.players.IndexOf(currentPlayer));
                GameController.gc.TradeBank(neededResource);
                return;
            }
        }
    }

    #endregion

    #region Decision Making

    public virtual int[] MakeDecision()
    {
        Player currentPlayer = GameController.gc.GetCurrentPlayer();
        CalculateCurrentTable();
        /*
         * 0 - Decision number knight force
         * 1 - Decision number longest road
         * 2 - Decision number Revenue
         * */
        int[] ret = new int[] {0,0,0};
        int[] largestNum = new int[] {0,0,0};

        for(int p = 0; p < GameController.gc.players.Count; p++)
        {
            Player player = GameController.gc.players[p];
            if (player == currentPlayer) continue;

            if (player.numberOfKnightsPlayed > largestNum[0]) largestNum[0] = player.numberOfKnightsPlayed;
            if (player.myLongestRoad > largestNum[1]) largestNum[1] = player.myLongestRoad;
            if (player.mySumOfRevenues > largestNum[2]) largestNum[2] = player.mySumOfRevenues;
        }


        int knightNum = Mathf.Clamp(currentPlayer.numberOfKnightsPlayed - largestNum[0], -6, 6);
        int roadNum = Mathf.Clamp(currentPlayer.myLongestRoad - largestNum[1], -6, 6);
        int revenueNum = Mathf.Clamp(currentPlayer.mySumOfRevenues - largestNum[2], -6, 6);

        try
        {
            ret[0] = currentTable[1][currentTable[0].IndexOf(knightNum)];
            ret[1] = currentTable[2][currentTable[0].IndexOf(roadNum)];
            ret[2] = currentTable[3][currentTable[0].IndexOf(revenueNum)];
        }
        catch (System.Exception e)
        {
            Debug.LogError("Range 0 = " + knightNum + " :: Range 1 = " + roadNum + " :: Range 2 = " + revenueNum);
        }

        /*Emergency currentEmergency = EmergencyHandler.GetEmergency(currentPlayer);
        if(currentEmergency != null) {
            currentEmergency.HandleResponse(currentPlayer);
            return ret;
        }*/

        //Debug.Log("Numbers: knight force = " + ret[0] + " , longest road = " + ret[1] + " ,  Revenue = " + ret[2]);

        int[] temp = ret;
        int num = GetLargestNumber(temp);
        if (num == 0)
        {
            currentStrategy = Strategy.Revenue;
            currentBuildProjects[0] = CalculateRevenueDecision(currentPlayer);
            temp[0] = 0;
            num = GetLargestNumber(temp);
            if(num == 1)
            {
                currentBuildProjects[1] = PlanBuilder.GetRoad();
                currentBuildProjects[2] = PlanBuilder.GetDevelopmentCard();
            }
            else if(num == 2)
            {
                currentBuildProjects[1] = PlanBuilder.GetDevelopmentCard();
                currentBuildProjects[2] = PlanBuilder.GetRoad();
            }
        }
        else if (num == 1)
        {
            currentStrategy = Strategy.LongestRoad;

            currentBuildProjects[0] = PlanBuilder.GetRoad();
            temp[1] = 0;
            num = GetLargestNumber(temp);
            if (num == 0)
            {
                currentBuildProjects[1] = CalculateRevenueDecision(currentPlayer);
                currentBuildProjects[2] = PlanBuilder.GetDevelopmentCard();
            }
            else if (num == 2)
            {
                currentBuildProjects[1] = PlanBuilder.GetDevelopmentCard();
                currentBuildProjects[2] = CalculateRevenueDecision(currentPlayer);
            }
        }
        else if (num == 2)
        {
            currentStrategy = Strategy.LargestArmy;

            currentBuildProjects[0] = PlanBuilder.GetDevelopmentCard();
            temp[1] = 0;
            num = GetLargestNumber(temp);
            if (num == 0)
            {
                currentBuildProjects[1] = CalculateRevenueDecision(currentPlayer);
                currentBuildProjects[2] = PlanBuilder.GetRoad();
            }
            else if (num == 1)
            {
                currentBuildProjects[1] = PlanBuilder.GetRoad();
                currentBuildProjects[2] = CalculateRevenueDecision(currentPlayer);
            }
        }

        return ret;
    }

    int GetLargestNumber(int[] input)
    {
        int ret = -1;

        for (int i = 0; i < input.Length; i++)
        {
            if (ret == -1) ret = i;
            else if (input[i] > input[ret]) ret = i;
        }

        return ret;
    }

    List<int> CalculateRevenueDecision(Player p)
    {
        List<int> ret = new List<int>();

        Corner checkCorner = GameController.gc.boardsCorners[0];

        for(int i = 0; i < p.myCorners.Count; i++)
        {
            int attractivness = 0;
            Corner corner = p.myCorners[i];
            for(int c = 0; c < corner.adjacentTiles.Count; c++)
            {
                attractivness += corner.adjacentTiles[c].myProbabilityOfRevenue;
            }

            if (attractivness > checkCorner.myAttractiveness) checkCorner = corner;
        }

        if (checkCorner != GameController.gc.boardsCorners[0]) return PlanBuilder.GetCity();

        if (checkCorner.distanceAway == 0) return PlanBuilder.GetDevelopmentCard();

        return PlanBuilder.GetRoad();
    }

    public virtual void CalculateCurrentTable() { }

    #endregion

    #region Make All This better

    public Corner GetSettlementCorner()
    {
        Player player = GameController.gc.GetCurrentPlayer();
        Corner corner = null;
        for(int i = 0; i < player.myEdges.Count; i++)
        {
            Edge edge = player.myEdges[i];
            for (int c = 0; c < edge.adjacentCorners.Count; c++)
            {
                Corner checkCorner = edge.adjacentCorners[c];

                if (corner != null && corner.myAttractiveness < checkCorner.myAttractiveness && !checkCorner.hasCity)
                {
                    if(checkCorner.hasSettlement) corner = checkCorner;
                    else
                    {
                        bool canPlace = true;
                        for (int aCorner = 0; aCorner < checkCorner.adjacentCorners.Count; aCorner++)
                        {
                            if (checkCorner.adjacentCorners[aCorner].hasCity || checkCorner.adjacentCorners[aCorner].hasSettlement) canPlace = false;
                        }

                        if (canPlace) corner = checkCorner;
                    }
                }

                if (corner == null)
                {
                    bool canPlace = true;
                    for (int aCorner = 0; aCorner < checkCorner.adjacentCorners.Count; aCorner++)
                    {
                        if (checkCorner.adjacentCorners[aCorner].hasCity || checkCorner.adjacentCorners[aCorner].hasSettlement) canPlace = false;
                    }

                    if (canPlace) corner = checkCorner;
                }
            }
        }

        return corner;
    }

    public void CalculateSettlement() {
        Corner toPlace = GetSettlementCorner();

        if (toPlace != null){
            toPlace.PlaceCorner();
        }
    }

    /// <summary>
    /// Given a Player and a Corner, breadth-first searches the board for an optimal path from the Player to the Corner,
    /// and returns the Edge from the Player which will lead them to the target Corner.
    /// </summary>
    public Edge GetEdgeTowardsCorner(Player player, Corner target) {
        Edge toPlace = null;

        // Track all of the paths moving outward from the player.
        List<List<Edge>> tracking = new List<List<Edge>>();

        // Pre-populate the tracking paths with the immediate edges surrounding the player.
        for(int cornerIdx = 0; cornerIdx < player.myCorners.Count; cornerIdx ++) {
            for(int edgeIdx = 0; edgeIdx < player.myCorners[cornerIdx].adjacentEdges.Count; edgeIdx ++) {
                tracking.Add(new List<Edge>() { player.myCorners[cornerIdx].adjacentEdges[edgeIdx] });
            }
        }

        bool found = false;
        while(!found){
            // Grab a path from the list, and remove it to prevent reprocessing.
            List<Edge> path = tracking[0];
            tracking.RemoveAt(0);

            // From the last edge of this path...
            Edge lastEdge = path[path.Count - 1];
            lastEdge.adjacentCorners.ForEach((Corner futureCorner)=>{
                // ..Check each corner to determine if it's our target.
                if (futureCorner == target) {
                    // The Edge to be placed will be the first segment of the path that is NOT a road.
                    toPlace = path.Find((Edge ed)=>{ return !ed.hasRoad; });
                    found = true;
                } else if(!futureCorner.hasSettlement && !futureCorner.hasCity){
                    // If the corner is passable, clone the current path and append that edge.
                    // That new path will be added to the tracking list to be searched over during a future iteration.
                    futureCorner.adjacentEdges.ForEach((Edge futureEdge)=>{
                        // Skip edges that exist within the path already.
                        if(!path.Contains(futureEdge)){
                            List<Edge> newPath = new List<Edge>(path);
                            newPath.Add(futureEdge);
                            tracking.Add(newPath);
                        }
                    });
                }
            });

            // In the chance we run out of paths, there is no possible route, and we should exit.
            if (tracking.Count <= 0){
                found = true;
            }
        }

        return toPlace;
    }

    public Edge GetRandomEdgeForPlayer(Player player) {
        int randEdge = Random.Range(0, player.myEdges.Count);
        int randCorner = Random.Range(0, player.myEdges[randEdge].adjacentCorners.Count);

        for (int i = 0; i < player.myEdges[randEdge].adjacentCorners[randCorner].adjacentEdges.Count; i++)
        {
            if (!player.myEdges[randEdge].adjacentCorners[randCorner].adjacentEdges[i].hasRoad)
            {
                return player.myEdges[randEdge].adjacentCorners[randCorner].adjacentEdges[i];
            }
        }

        return null;
    }

    public void CalculateRoad()
    {
        Player player = GameController.gc.GetCurrentPlayer();

        //player.myResourceCards[0] += 2;
        //player.myResourceCards[1] += 2;
        Edge edgeToPlaceRoad = null;

        edgeToPlaceRoad = GetEdgeTowardsCorner(player, GameController.gc.boardsCorners[0]);

        // If for some reason we can't get an edge heading towards our target, simply select one at random.
        while (edgeToPlaceRoad == null)
        {
            edgeToPlaceRoad = GetRandomEdgeForPlayer(player);
        }

        edgeToPlaceRoad.PlaceEdge();
    }

    public void CalculateDevelopmentCards()
    {
        Player player = GameController.gc.GetCurrentPlayer();

        if (GameController.gc.BuyDevelopmentCard()) return;

        if (player.myDevelopmentCards.Count == 0) return;

        List<Card> victoryPointCards = new List<Card>();
        Card cardToPlay = null;

        for(int c = 0; c <  player.myDevelopmentCards.Count; c++)
        {
            if (player.myDevelopmentCards[c].myCardType != CardType.VictoryPoint && cardToPlay == null) cardToPlay = player.myDevelopmentCards[c];

            if (player.myDevelopmentCards[c].myCardType == CardType.VictoryPoint) victoryPointCards.Add(player.myDevelopmentCards[c]);
        }

        if(player.playerVictoryPoints + victoryPointCards.Count >= 10)
        {
            int vpCount = victoryPointCards.Count;
            for(int i = 0; i < vpCount; i++)
            {
                victoryPointCards[i].PlayCard();
            }

        }else
        {
           if (cardToPlay == null) return;
           if(cardToPlay.myCardType == CardType.Knight)
            {
                cardToPlay.PlayCard();
                //Debug.Log("Moving Robber");
                MoveRobber();
            }
            else if(cardToPlay.myCardType == CardType.RoadBuilding)
            {
                cardToPlay.PlayCard();
                int freeRoadCount = player.numberOfFreeRoads;
                for(int i = 0; i < freeRoadCount; i++)
                {
                    //Debug.Log("Placing road " + (i +1));
                    CalculateRoad();
                }
            }
            else if (cardToPlay.myCardType == CardType.Monopoly)
            {
                cardToPlay.PlayCard();

                int resourceToPick = player.myAI.differenceSum.IndexOf(player.myAI.differenceSum.Min());

                //Debug.Log("Lowest Resource = " + ((ResourceType)resourceToPick).ToString());

                GameController.gc.ResourcePicked((ResourceType)resourceToPick);
            }else if(cardToPlay.myCardType == CardType.YearOfPlenty)
            {
                cardToPlay.PlayCard();
                for(int i = 0; i < 2; i++)
                {
                    int resourceToPick = 100;
                    for (int j = 0; j < player.myAI.differenceSum.Count; j++)
                    {
                        if (player.myAI.differenceSum[j] < resourceToPick) resourceToPick = j;
                    }

                    //Debug.Log("Lowest Resource " + (i+1) + " = " + ((ResourceType)resourceToPick).ToString());

                    GameController.gc.ResourcePicked((ResourceType)resourceToPick);
                }
            }
        }


    }

    #endregion
}
