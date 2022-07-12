using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JeanAI : AIArchetype {

    public override void SetUpAI()
    {
        myName = "Jean";
        discription = "Cheeky, Bold";
        strategyType = "Aggressive";

        myStrategy = new List<Strategy>() { Strategy.LargestArmy, Strategy.Revenue, Strategy.LongestRoad };
        startupResources = new List<float>() {0,0,.33f,.33f,.33f};

        willingnessToTrade = Level.low;
        risk = Level.high;

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
            new List<int>() {2,2,2,7,9,9,11,11,9,9,7,2,2},
            new List<int>() {1,1,4,4,6,6,6,6,6,4,4,1,1},
            new List<int>() {3,3,5,5,5,8,10,10,10,10,10,10,10},
        };

        ret.Add("A1", addList);

        addList = new List<List<int>>()
        {
            new List<int>() {6,5,4,3,2,1,0,-1,-2,-3,-4,-5,-6},
            new List<int>() {1,1,4,4,6,6,6,6,6,4,4,1,1},
            new List<int>() {3,3,5,5,5,8,10,10,10,10,10,10,10},
            new List<int>() {2,2,2,7,9,11,11,11,11,11,11,11,11},
        };

        ret.Add("B1", addList);

        return ret;
    }

    public override void CalculateCurrentTable()
    {
        Player currentPlayer = GameController.gc.GetCurrentPlayer();

        bool hasHigherOre = false; //Another player has a higher individual frequency of resource for ore and the individual frequency of the AI for ore is < 3
        bool hasHigherGrain = false; //Another player has a higher individual frequency of resource for grain and the individual frequency of the AI for grain is < 3

        for (int p = 0; p < GameController.gc.players.Count; p++)
        {
            Player player = GameController.gc.players[p];
            if (player == currentPlayer) continue;
            if (player.myFrequencyFactor[(int)ResourceType.Ore] > currentPlayer.myFrequencyFactor[(int)ResourceType.Ore] && currentPlayer.myFrequencyFactor[(int)ResourceType.Ore] < 3) hasHigherOre = true;
            if (player.myFrequencyFactor[(int)ResourceType.Grain] > currentPlayer.myFrequencyFactor[(int)ResourceType.Grain] && currentPlayer.myFrequencyFactor[(int)ResourceType.Grain] < 3) hasHigherGrain = true;

        }

        //Part1
        if (hasHigherOre && hasHigherGrain && currentPlayer.myFrequencyFactor[(int)ResourceType.Brick] > 3 && currentPlayer.myFrequencyFactor[(int)ResourceType.Lumber] > 3)
        {
            currentTable = decisionList["B1"];
        }
        else
        {
            currentTable = decisionList["A1"];
        }
    }
}
