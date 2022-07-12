using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    public static float TURN_WAIT_TIME = 0; //1.25f;

    Player player;

    Corner attract1;
    Corner attract2;

    public bool tryingToTrade;

    public void TakeTurn(Player p)
    {
        //THIS IS FOR The Last Gamebaord set to 1.25f if you want normal game
        if (GameController.gc.currentState == GameState.setup) TURN_WAIT_TIME = 0;
        else TURN_WAIT_TIME = 1.25f;

        player = p;
        attract1 = GameController.gc.boardsCorners[0];
        attract2 = GameController.gc.boardsCorners[1];
        StartCoroutine(TakeMyTurn());
    }

    IEnumerator TakeMyTurn()
    {
        yield return new WaitForSeconds(TURN_WAIT_TIME);
        player.ui.RollDice();
        yield return new WaitUntil(() => GameController.gc.gameDice.HaveSettled());
        yield return new WaitForSeconds(1f);


        if (GameController.gc.currentState == GameState.setup)
        {
            yield return new WaitForSeconds(TURN_WAIT_TIME);
            attract1.PlaceCorner();
            yield return new WaitForSeconds(TURN_WAIT_TIME);

            if (GameController.gc.turnNumber < GameController.gc.players.Count)
            {
                CalculateClosestRoad(attract1, attract2).PlaceEdge();
            }
            else
            {
                CalculateClosestRoad(attract1, player.myCorners[0]).PlaceEdge();
            }
        }
        else
        {
            while (!GameController.gc.CalculateTooManyCards())
            {
                yield return null;
            }

            if (GameController.gc.robber.needsToMove) player.myAI.MoveRobber();

            player.myAI.MakeDecision();


            if (player.myAI.currentStrategy == Strategy.Revenue)
            {
                if (player.myAI.currentBuildProjects[0] == PlanBuilder.GetCity())
                {
                    Debug.Log("Sub catagory City");
                    if (player.HasResource(ResourceType.Grain, 2) && player.HasResource(ResourceType.Ore, 3)) player.myAI.CalculateSettlement();
                    else
                    {
                        tryingToTrade = true;
                        player.myAI.TryToTrade(player);
                        //while (tryingToTrade) yield return null;

                        yield return new WaitForSeconds(TURN_WAIT_TIME);
                        player.myAI.CalculateSettlement();
                    }

                }
                else if (player.myAI.currentBuildProjects[0] == PlanBuilder.GetSettlement())
                {
                    Debug.Log("Sub catagory Settlement");
                    if (player.HasResource(ResourceType.Lumber, 1) && player.HasResource(ResourceType.Brick, 1) && player.HasResource(ResourceType.Grain, 1) && player.HasResource(ResourceType.Wool, 1)) player.myAI.CalculateSettlement();
                    else
                    {
                        tryingToTrade = true;
                        player.myAI.TryToTrade(player);
                        //while (tryingToTrade) yield return null;

                        yield return new WaitForSeconds(TURN_WAIT_TIME);
                        player.myAI.CalculateSettlement();
                    }
                }
                else if(player.myAI.currentBuildProjects[0] == PlanBuilder.GetRoad())
                {
                    Debug.Log("Sub catagory Road");
                    if (player.HasResource(ResourceType.Lumber, 1) && player.HasResource(ResourceType.Brick, 1)) player.myAI.CalculateRoad();
                    else
                    {
                        tryingToTrade = true;
                        player.myAI.TryToTrade(player);
                        //while (tryingToTrade) yield return null;

                        yield return new WaitForSeconds(TURN_WAIT_TIME);
                        player.myAI.CalculateRoad();
                    }
                }
            }
            else if (player.myAI.currentStrategy == Strategy.LongestRoad)
            {
                if (player.HasResource(ResourceType.Lumber, 1) && player.HasResource(ResourceType.Brick, 1)) player.myAI.CalculateRoad();
                else
                {
                    tryingToTrade = true;
                    player.myAI.TryToTrade(player);

                    //while (tryingToTrade) yield return null;

                    yield return new WaitForSeconds(TURN_WAIT_TIME);
                    player.myAI.CalculateRoad();
                }
            }
            else if(player.myAI.currentStrategy == Strategy.LargestArmy)
            {
                if (player.HasResource(ResourceType.Grain, 1) && player.HasResource(ResourceType.Wool, 1) && player.HasResource(ResourceType.Ore, 1)) player.myAI.CalculateDevelopmentCards();
                else
                {
                    tryingToTrade = true;
                    player.myAI.TryToTrade(player);
                    //while (tryingToTrade) yield return null;

                    yield return new WaitForSeconds(TURN_WAIT_TIME);
                    player.myAI.CalculateDevelopmentCards();
                }
            }

            yield return new WaitForSeconds(TURN_WAIT_TIME);

        }


        yield return new WaitForSeconds(TURN_WAIT_TIME / 2);

        while (GameController.gc.animationPlaying) yield return null;

        GameController.gc.GotoNextTurn();
        //Debug.Log("Ended Turn");

        yield return null;
    }

    public void TooManyResources(Player p, int numberOfResourcesToRemove)
    {
        StartCoroutine(RemoveResource(p, numberOfResourcesToRemove));
    }

    IEnumerator RemoveResource(Player p, int numberOfResourcesToRemove)
    {
        for(int i = 0; i < numberOfResourcesToRemove + 1; i++)
        {
            p.myAI.CalculateTradeSum(p);
            int largestNum = 0;
            for (int j = 0; j < p.myResourceCards.Length; j++)
            {
                if (p.myAI.differenceSum[j] > p.myAI.differenceSum[largestNum]) largestNum = j;
            }

            int resource = Random.Range(0,p.myResourceCards.Length);

            while(p.myResourceCards[resource] == 0)
            {
                resource = Random.Range(0, p.myResourceCards.Length);
                yield return null;
            }

            p.RemoveResource((ResourceType)resource, 1);
            Bank.AddToBank((ResourceType)resource, 1);

            yield return new WaitForSeconds(TURN_WAIT_TIME / 2);
        }
        yield return null;
    }

    Edge CalculateClosestRoad(Corner c, Corner far)
    {
        int longest = 0;
        for(int i = 0; i < c.adjacentEdges.Count; i++)
        {
            float dist1 = (c.adjacentEdges[i].transform.position - far.transform.position).sqrMagnitude;
            float dist2 = (c.adjacentEdges[longest].transform.position - far.transform.position).sqrMagnitude;
            if (dist1 < dist2) longest = i;
        }

        return c.adjacentEdges[longest];
    }
}
