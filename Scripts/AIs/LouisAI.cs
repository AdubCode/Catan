using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LouisAI : AIArchetype
{

    public override void SetUpAI()
    {
        myName = "Louis";
        discription = "Choleric, Short-Tempered";
        strategyType = "Depending on starting position";

        myStrategy = new List<Strategy>() { Strategy.Depends };
        startupResources = new List<float>() { 0, 0, 0, 0, 0 };

        willingnessToTrade = Level.normal;
        risk = Level.normal;

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
            new List<int>() {3,3,5,5,8,10,10,10,10,10,5,5,5},
            new List<int>() {2,2,2,7,9,11,11,11,9,9,7,2,2},
        };
        ret.Add("A1", addList);

        addList = new List<List<int>>()
        {
            new List<int>() {6,5,4,3,2,1,0,-1,-2,-3,-4,-5,-6},
            new List<int>() {3,3,5,5,8,10,10,10,10,10,5,5,5},
            new List<int>() {1,1,4,4,6,6,6,6,6,4,4,1,1},
            new List<int>() {2,2,2,7,9,11,11,11,9,9,7,2,2},
        };
        ret.Add("B1", addList);

        addList = new List<List<int>>()
        {
            new List<int>() {6,5,4,3,2,1,0,-1,-2,-3,-4,-5,-6},
            new List<int>() {1,1,4,4,4,6,6,6,4,4,4,1,1},
            new List<int>() {3,3,5,5,8,10,10,10,5,5,5,5,5},
            new List<int>() {2,2,2,7,9,11,11,11,9,9,7,2,2},
        };
        ret.Add("C1", addList);

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

        //check 1a
        bool largestFrequencyOfBrick = true; //No other player has a bigger individual frequency of resource for brick.
        bool noDoubleFrequencyLumber = true;//No other player’s individual frequency of resource for wood is more than double as high than the individual frequency of the AI for wood.

        //check 1b
        bool largestFrequencyOfLumber = true; //No other player has a bigger individual frequency of resource for wood.
        bool noDoubleFrequencyBrick = true; //No other player’s individual frequency of resource for brick is more than double as high than the individual frequency of the AI for brick.

        //check 1c
        bool aiHasThreeOne = false; //The AI owns a 3:1 harbor.

        //check 2a
        bool largestFrequencyOfOre = true; //No other player has a bigger individual frequency of resource for ore.
        bool noDoubleFrequencyGrain = true; //No other player’s individual frequency of resource for grain is more than double as high than the individual frequency of the AI for grain.

        //check 2b
        bool largestFrequencyOfGrain = true; //No other player has a bigger individual frequency of resource for wheat.
        bool noDoubleFrequencyOre = true; //No other player’s individual frequency of resource for ore is more than double as high than the individual frequency of the AI for ore.


        //check 3a
        bool noPlayerHasSix = true; //No player has more than 6 victory points.
        bool virtualLRIsShorterThanLR = false; //The virtual LR of the AI is shorter than the LR of at least one opponent.
        bool oneRoadShorter = false; //The LR of an opponent is no more than 1 road shorter.

        //check 3b
        bool playerHasSevenMore = false; //A minimum of one player owns no less than 7 victory points.
        bool virtualRoadIsShorter = false; //The virtual LR of the AI is shorter than the one of an opponent.
        bool someoneCanWin = false; //The opponents cannot win in their round with building a road

        for (int p = 0; p < GameController.gc.players.Count; p++)
        {
            Player player = GameController.gc.players[p];
            if (player == currentPlayer) continue;
            if (player.myFrequencyFactor[(int)ResourceType.Brick] > currentPlayer.myFrequencyFactor[(int)ResourceType.Brick]) largestFrequencyOfBrick = false;
            if (player.myFrequencyFactor[(int)ResourceType.Lumber] > currentPlayer.myFrequencyFactor[(int)ResourceType.Lumber] * 2) noDoubleFrequencyLumber = false;

            if (player.myFrequencyFactor[(int)ResourceType.Lumber] > currentPlayer.myFrequencyFactor[(int)ResourceType.Lumber]) largestFrequencyOfLumber = false;
            if (player.myFrequencyFactor[(int)ResourceType.Brick] > currentPlayer.myFrequencyFactor[(int)ResourceType.Brick] * 2) noDoubleFrequencyBrick = false;

            if (player.myFrequencyFactor[(int)ResourceType.Ore] > currentPlayer.myFrequencyFactor[(int)ResourceType.Ore]) largestFrequencyOfOre = false;
            if (player.myFrequencyFactor[(int)ResourceType.Grain] > currentPlayer.myFrequencyFactor[(int)ResourceType.Grain] * 2) noDoubleFrequencyGrain = false;

            if (player.myFrequencyFactor[(int)ResourceType.Grain] > currentPlayer.myFrequencyFactor[(int)ResourceType.Grain]) largestFrequencyOfGrain = false;
            if (player.myFrequencyFactor[(int)ResourceType.Ore] > currentPlayer.myFrequencyFactor[(int)ResourceType.Ore] * 2) noDoubleFrequencyOre = false;

            if (player.playerVictoryPoints > 6) noPlayerHasSix = false;
            if (currentPlayer.myVirtualLongestRoad < player.myLongestRoad) virtualLRIsShorterThanLR = true;
            if (player.myLongestRoad - 1 == currentPlayer.myLongestRoad) oneRoadShorter = true;

            if (player.playerVictoryPoints >= 7) playerHasSevenMore = true;
            if (currentPlayer.myVirtualLongestRoad < player.myVirtualLongestRoad) virtualRoadIsShorter = true;
            if (player.playerVictoryPoints >= 8 && !player.longestRoad && player.myLongestRoad - 1 == GameController.gc.GetPlayerWithLongestRoad().myLongestRoad) someoneCanWin = true;
        }

        for (int i = 0; i < currentPlayer.tradeValues.Length; i++)
        {
            if (currentPlayer.tradeValues[i] == 3) aiHasThreeOne = true;
        }

        //check 1a, 1b, 1c
        if ((largestFrequencyOfBrick && currentPlayer.myFrequencyFactor[(int)ResourceType.Lumber] > 2 && noDoubleFrequencyLumber) ||  //1a
            (largestFrequencyOfLumber && currentPlayer.myFrequencyFactor[(int)ResourceType.Brick] > 2 && noDoubleFrequencyBrick) || //1b
            (currentPlayer.myFrequencyFactor[(int)ResourceType.Lumber] > 3 && currentPlayer.myFrequencyFactor[(int)ResourceType.Brick] > 3 && noDoubleFrequencyLumber && noDoubleFrequencyBrick && aiHasThreeOne))  //1c
        {
            currentTable = decisionList["A1"];
        }
        //check 2a, 2b, 2c
        else if ((largestFrequencyOfOre && currentPlayer.myFrequencyFactor[(int)ResourceType.Grain] > 2 && currentPlayer.myFrequencyFactor[(int)ResourceType.Wool] > 2 && noDoubleFrequencyGrain) || //2a
            (largestFrequencyOfGrain && currentPlayer.myFrequencyFactor[(int)ResourceType.Ore] > 2 && currentPlayer.myFrequencyFactor[(int)ResourceType.Wool] > 2 && noDoubleFrequencyOre) || //2b
            (currentPlayer.myFrequencyFactor[(int)ResourceType.Ore] > 3 && currentPlayer.myFrequencyFactor[(int)ResourceType.Grain] > 3 && currentPlayer.myFrequencyFactor[(int)ResourceType.Wool] > 2 && noDoubleFrequencyOre && noDoubleFrequencyGrain && aiHasThreeOne)) //2c
        {
            currentTable = decisionList["B1"];
        }
        //check 3a, 3b,
        else if ((noPlayerHasSix && virtualLRIsShorterThanLR && currentPlayer.roadsNeededForVLR < 5 && oneRoadShorter) || //3a
            (playerHasSevenMore && virtualRoadIsShorter && currentPlayer.roadsNeededForVLR <= 2 && oneRoadShorter && !someoneCanWin)) //3b
        {
            currentTable = decisionList["A2"];
        }
        else
        {
            currentTable = decisionList["C1"];
        }
    }
}