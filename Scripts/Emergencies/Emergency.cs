using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Emergency: Building point has DOC 10 and is a point of settlement
/// Response: Set 'settlement' building project
/// </summary>
public abstract class Emergency
{
    /// <summary>
    /// Given a player, determines if the requirements for this Emergency scenario have been fulfilled.
    /// </summary>
    public abstract bool EvaluateConditions(Player player);

    /// <summary>
    /// Plays cards or sets building projects to remediate the Emergency situation.
    /// </summary>
    public abstract void HandleResponse(Player player);

    /// <summary>
    /// Returns the player's RoadBuilding card, if one exists in their hand.
    /// </summary>
    protected Card GetRoadbuildingCard(Player player)
    {
        return player.myDevelopmentCards.Find((Card card) => card.myCardType == CardType.RoadBuilding);
    }

    /// <summary>
    /// Determines if the given player has a RoadBuilding card in their hand.
    /// </summary>
    protected bool CheckPlayerRoadbuilding(Player player)
    {
        Card roadbuildingCard = GetRoadbuildingCard(player);
        return roadbuildingCard != null;
    }

    /// <summary>
    /// Returns the Corner of which the player's longest road points to.
    /// #TODO - This must return the corner at the "active end of the longest road."
    /// </summary>
    protected Corner GetEndOfPlayerLongestRoad(Player player)
    {
        return player.myEdges[player.myEdges.Count - 1].adjacentCorners[1];
    }

    /// <summary>
    /// Returns a virtual settlement point within the virtual longest road.
    /// #TODO
    /// </summary>
    protected Corner GetVirtualSettlementPoint(Player player)
    {
        return player.myEdges[player.myEdges.Count - 1].adjacentCorners[1];
    }

    /// <summary>
    /// Determines if the player's current corner can have a settlement placed on it.
    /// </summary>
    protected bool CheckSettlementCorner(Player player)
    {
        return player.myAI.GetSettlementCorner() != null;
    }

    /// <summary>
    /// Determines if the player's current corner can have a settlement placed on it, and if that corner is attractive.
    /// </summary>
    protected bool CheckAttractiveSettlementCorner(Player player)
    {
        Corner corner = player.myAI.GetSettlementCorner();
        return corner != null && corner.myAttractiveness >= 5f;
    }


    /// <summary>
    /// Returns the corner [distance] units away from the given player.
    /// #TODO - This must grab "the corner [distance] units away"
    /// </summary>
    protected Corner GetNearbyCorner(Player player, int distance)
    {
        // #TODO grab nearby corner
        return player.myCorners[distance - 1];
    }
}