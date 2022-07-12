using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Emergency: Active end of the longest road has DOC 10 and is a point of settlement
/// Response: AI sets building project as settlement and road toward target
/// </summary>
public class LongestRoadEmergency1 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        Corner lastPoint = GetEndOfPlayerLongestRoad(player);
        int pointDOC = lastPoint.degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);

        return pointDOC == 10 && hasPointOfSettlement;
    }

    public override void HandleResponse(Player player)
    {
        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetSettlement(),
            PlanBuilder.GetRoad(),
            PlanBuilder.GetNone(),
        };
    }
}

/// <summary>
/// Emergency: Active end of the longest road has DOC 1 or 2 and is an unattractive point of settlement
/// Response: AI sets building project as road towards opp's building point
/// </summary>
public class LongestRoadEmergency1a : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        Corner lastPoint = GetEndOfPlayerLongestRoad(player);
        int pointDOC = lastPoint.degreeOfCompetition;

        bool hasPointOfSettlement = CheckSettlementCorner(player);
        bool isAttractivePoint = CheckAttractiveSettlementCorner(player);

        return (pointDOC == 1 || pointDOC == 2) && hasPointOfSettlement && !isAttractivePoint;
    }

    public override void HandleResponse(Player player)
    {
        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetRoad(),
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
        };
    }
}


/// <summary>
/// Emergency: Corner one unit away has DOC -1 and is a settlement point
/// Response: AI sets building project as road and settlement
/// </summary>
public class LongestRoadEmergency2 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 1).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);

        return (pointDOC == -1) && hasPointOfSettlement;
    }

    public override void HandleResponse(Player player)
    {
        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetRoad(),
            PlanBuilder.GetSettlement(),
            PlanBuilder.GetNone(),
        };
    }
}

/// <summary>
/// Emergency: Corner one unit away has DOC -1 and is an UNATTRACTIVE settlement point
/// Response: If AI has road building { Play road building, one road toward corner, one road toward opponent }
///     else { set project as one road toward corner, one road toward opponent }
/// </summary>
public class LongestRoadEmergency2a : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 1).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);
        bool hasAttractivePoint = CheckAttractiveSettlementCorner(player);

        return (pointDOC == -1) && hasPointOfSettlement && !hasAttractivePoint;
    }

    public override void HandleResponse(Player player)
    {
        if (CheckPlayerRoadbuilding(player))
        {
            Card roadbuildingCard = GetRoadbuildingCard(player);
            roadbuildingCard.PlayCard(); // #TODO: roadbuilding in two different directions
        }
        else
        {
            player.myAI.currentBuildProjects = new List<List<int>>() {
                PlanBuilder.GetRoad(),
                PlanBuilder.GetRoad(),
                PlanBuilder.GetNone(),
            };
        }


    }
}


/// <summary>
/// Emergency: Corner one unit away has DOC -1 and is an UNATTRACTIVE settlement point
/// Response: If AI has road building { Play road building, one road toward corner, one road toward opponent }
///     else { set project as one road toward corner, one road toward opponent }
/// </summary>
public class LongestRoadEmergency3 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 1).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);
        bool hasAttractivePoint = CheckAttractiveSettlementCorner(player);

        return pointDOC == -1 && hasPointOfSettlement && !hasAttractivePoint;
    }

    public override void HandleResponse(Player player)
    {
        if (CheckPlayerRoadbuilding(player))
        {
            Card roadbuildingCard = GetRoadbuildingCard(player);
            roadbuildingCard.PlayCard(); // #TODO: roadbuilding in two different directions
        }
        else
        {
            player.myAI.currentBuildProjects = new List<List<int>>() {
                PlanBuilder.GetRoad(),
                PlanBuilder.GetRoad(),
                PlanBuilder.GetNone(),
            };
        }
    }
}


/// <summary>
/// Emergency: Corner two units away has DOC -2 and is a settlement point and has roadbuilding
/// Response: AI plays road building then sets building project as settlement
/// </summary>
public class LongestRoadEmergency5 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 2).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);
        bool hasRoadbuilding = CheckPlayerRoadbuilding(player);

        return pointDOC == -2 && hasPointOfSettlement && hasRoadbuilding;
    }

    public override void HandleResponse(Player player)
    {
        
        Card roadbuildingCard = GetRoadbuildingCard(player);
        roadbuildingCard.PlayCard();

        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetSettlement(),
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
        };
    }
}

/// <summary>
/// Emergency: Corner two units away has DOC -1 or 0, and is a settlement point, and has roadbuilding
/// Response: AI plays road building then sets building project as road towards opponents building point
/// </summary>
public class LongestRoadEmergency5a : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 2).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);
        bool hasRoadbuilding = CheckPlayerRoadbuilding(player);

        return (pointDOC == -1 || pointDOC == 0) && hasPointOfSettlement && hasRoadbuilding;
    }

    public override void HandleResponse(Player player)
    {
        
        Card roadbuildingCard = GetRoadbuildingCard(player);
        roadbuildingCard.PlayCard();

        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetRoad(), // #TODO build towards opponent
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
        };
    }
}


/// <summary>
/// Emergency: Settlement point within virtual longest road has a DOC of 10, 1, 2
/// Response: AI must build a settlement on the "crossing point"
/// </summary>
public class DisconnectRoadEmergency : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetVirtualSettlementPoint(player).degreeOfCompetition;

        return false && (pointDOC == 1 || pointDOC == 2 || pointDOC == 10);
    }

    public override void HandleResponse(Player player)
    {   
        // #TODO Build settlement on virtual settlement point
        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetSettlement(),
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
        };
    }
}


/// <summary>
/// Emergency: Opp has longest road and needs 2 points to win, and AI's VRL is longer than Opp's
/// Response: AI must build roads to connect virtual longest road
/// </summary>
public class ConnectRoadEmergency : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        Player opponent = GameController.gc.players.Find((Player opp)=>{
            return opp != player && opp.longestRoad && opp.playerVictoryPoints >= 8 && player.myVirtualLongestRoad > opp.myLongestRoad;
        });

        return opponent != null;
    }

    public override void HandleResponse(Player player)
    {
        Card roadbuildingCard = GetRoadbuildingCard(player);
        if (roadbuildingCard != null) {
            roadbuildingCard.PlayCard();
        } else {
            // #TODO Build towards target.
            player.myAI.currentBuildProjects = new List<List<int>>() {
                PlanBuilder.GetRoad(),
                PlanBuilder.GetRoad(),
                PlanBuilder.GetNone(),
            };
        }
    }
}

