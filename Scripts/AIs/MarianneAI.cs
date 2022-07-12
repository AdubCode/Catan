using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarianneAI : AIArchetype {

    public override void SetUpAI()
    {
        myName = "Marianne";
        discription = "Generous, Helpful";
        strategyType = "Expansion";

        myStrategy = new List<Strategy>() { Strategy.Revenue, Strategy.LongestRoad, Strategy.LargestArmy };
        startupResources = new List<float>() { .33f, .33f, 0, .33f, 0 };

        willingnessToTrade = Level.high;
        risk = Level.low;

        decisionList = SetupTables();
    }

    public override Dictionary<string, List<List<int>>> SetupTables()
    {
        Dictionary<string, List<List<int>>> ret = new Dictionary<string, List<List<int>>>();

        //makes it so that there are no build projects
        currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
        };

        List<List<int>> addList = new List<List<int>>() {
            new List<int>() {6,5,4,3,2,1,0,-1,-2,-3,-4,-5,-6},
            new List<int>() {1,1,4,4,6,6,6,6,6,4,4,1,1},
            new List<int>() {3,3,5,5,5,8,10,10,10,10,10,10,10},
            new List<int>() {2,2,2,7,9,9,11,11,9,9,7,2,2},
        };

        ret.Add("A1", addList);

        addList = new List<List<int>>()
        {
            new List<int>() {6,5,4,3,2,1,0,-1,-2,-3,-4,-5,-6},
            new List<int>() {3,3,5,5,5,8,10,10,10,10,10,10,10},
            new List<int>() {1,1,4,4,6,6,6,6,6,4,4,1,1},
            new List<int>() {2,2,2,7,9,9,11,11,9,9,7,2,2},
        };

        ret.Add("B1", addList);

        addList = new List<List<int>>()
        {
            new List<int>() {6,5,4,3,2,1,0,-1,-2,-3,-4,-5,-6},
            new List<int>() {1,1,4,4,6,6,6,6,6,4,4,1,1},
            new List<int>() {0,0,0,0,0,0,0,0,0,0,0,0,0},
            new List<int>() {3,3,5,5,5,8,10,10,10,10,10,10,10},
        };

        ret.Add("A2", addList);

        return ret;
    }

    public override void CalculateCurrentTable()
    {
        Player currentPlayer = GameController.gc.GetCurrentPlayer();

        bool hasHigherLumber = false; //Another player has a higher individual frequency of resource for wood and the individual frequency of the AI for wood is < 3
        bool hasHigherBrick = false; //Another player has a higher individual frequency of resource for brick and the individual frequency of the AI for brick is < 3

        //part 2
        bool noPlayerHasSix = true; //No player has more than 6 victory points.
        bool virtualLRIsShorterThanLR = false; //The virtual LR of the AI is shorter than the LR of at least one opponent.
        bool oneRoadShorter = false; //The LR of an opponent is no more than 1 road shorter.

        //part 3
        bool playerHasSevenMore = false; //A minimum of one player owns no less than 7 victory points.
        bool virtualRoadIsShorter = false; //The virtual LR of the AI is shorter than the one of an opponent.
        bool someoneCanWin = false; //The opponents cannot win in their round with building a road

        for (int p = 0; p < GameController.gc.players.Count; p++)
        {
            Player player = GameController.gc.players[p];
            if (player == currentPlayer) continue;
            if (player.myFrequencyFactor[(int)ResourceType.Lumber] > currentPlayer.myFrequencyFactor[(int)ResourceType.Lumber] && currentPlayer.myFrequencyFactor[(int)ResourceType.Lumber] < 3) hasHigherLumber = true;
            if (player.myFrequencyFactor[(int)ResourceType.Brick] > currentPlayer.myFrequencyFactor[(int)ResourceType.Brick] && currentPlayer.myFrequencyFactor[(int)ResourceType.Brick] < 3) hasHigherBrick = true;

            if (player.playerVictoryPoints > 6) noPlayerHasSix = false;
            if (currentPlayer.myVirtualLongestRoad < player.myLongestRoad) virtualLRIsShorterThanLR = true;
            if (player.myLongestRoad - 1 == currentPlayer.myLongestRoad) oneRoadShorter = true;

            if (player.playerVictoryPoints >= 7) playerHasSevenMore = true;
            if (currentPlayer.myVirtualLongestRoad < player.myVirtualLongestRoad) virtualRoadIsShorter = true;
            if (player.playerVictoryPoints >= 8 && !player.longestRoad && player.myLongestRoad - 1 == GameController.gc.GetPlayerWithLongestRoad().myLongestRoad) someoneCanWin = true;

        }

        //Part1
        if (hasHigherLumber && hasHigherBrick && currentPlayer.myFrequencyFactor[(int)ResourceType.Ore] > 3 && currentPlayer.myFrequencyFactor[(int)ResourceType.Grain] > 3 && currentPlayer.myFrequencyFactor[(int)ResourceType.Wool] > 1)
        {
            currentTable = decisionList["B1"];
        }
        else
        {
            currentTable = decisionList["A1"];
        }

        //Part2
        if (noPlayerHasSix && virtualLRIsShorterThanLR && currentPlayer.roadsNeededForVLR < 5 && oneRoadShorter)
        {
            currentTable = decisionList["A2"];
        }

        //Part3
        if (playerHasSevenMore && virtualRoadIsShorter && currentPlayer.roadsNeededForVLR <= 2 && oneRoadShorter && !someoneCanWin)
        {
            currentTable = decisionList["A2"];
        }
    }
}
