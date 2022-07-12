using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Emergency: Building point has DOC 10 and is a point of settlement
/// Response: Set 'settlement' building project
/// </summary>
public class BuildingEmergency1 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 1).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);

        return pointDOC == 10 && hasPointOfSettlement;
    }

    public override void HandleResponse(Player player)
    {
        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetSettlement(),
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
        };
    }
}

/// <summary>
/// Emergency: Crossing point 1 unit away has DOC -1,0,1 and is a settlement point
/// Response: If AI has road building { Play road building, set project as settlement }
///     else { set project as road and settlement }
/// </summary>
public class BuildingEmergency2 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 1).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);

        return (-1 <= pointDOC && pointDOC <= 1) && hasPointOfSettlement;
    }

    public override void HandleResponse(Player player)
    {
        Card roadbuildingCard = GetRoadbuildingCard(player);
        if (roadbuildingCard != null)
        {
            roadbuildingCard.PlayCard();
            player.myAI.currentBuildProjects = new List<List<int>>() {
                PlanBuilder.GetSettlement(),
                PlanBuilder.GetNone(),
                PlanBuilder.GetNone(),
            };
        }
        else
        {
            player.myAI.currentBuildProjects = new List<List<int>>() {
                PlanBuilder.GetRoad(),
                PlanBuilder.GetSettlement(),
                PlanBuilder.GetNone(),                
            };
        }
    }
}

/// <summary>
/// Emergency: Crossing point 1 unit away has DOC -2,-1 and is NOT a settlement point
/// Response: If AI has road building { Play road building }
///     else { set project as two roads }
/// </summary>
public class BuildingEmergency3 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 1).degreeOfCompetition;
        bool hasPointOfSettlement = player.myAI.GetSettlementCorner() != null;

        return (pointDOC == -2 || pointDOC == -1) && !hasPointOfSettlement;
    }

    public override void HandleResponse(Player player)
    {
        Card roadbuildingCard = GetRoadbuildingCard(player);
        if (roadbuildingCard != null)
        {
            roadbuildingCard.PlayCard();
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
/// Emergency: Crossing point 2 units away has DOC -2 and is a settlement point and AI has roadbuilding
/// Response: AI plays road building then sets project as settlement
/// </summary>
public class BuildingEmergency5 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 2).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);
        bool hasRoadbuilding = CheckPlayerRoadbuilding(player);

        return (pointDOC == -2) && hasPointOfSettlement && hasRoadbuilding;
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
/// Emergency: Crossing point 2 units away has DOC -2,-1,0 and is NOT a settlement point and AI has roadbuilding
/// Response: AI plays roadbuilding then sets build project as road towards target
/// </summary>
public class BuildingEmergency6 : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        int pointDOC = GetNearbyCorner(player, 2).degreeOfCompetition;
        bool hasPointOfSettlement = CheckSettlementCorner(player);
        bool hasRoadbuilding = CheckPlayerRoadbuilding(player);

        return (pointDOC == -2) && !hasPointOfSettlement && hasRoadbuilding;
    }

    public override void HandleResponse(Player player)
    {
        Card roadbuildingCard = GetRoadbuildingCard(player);        
        
        roadbuildingCard.PlayCard();

        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetRoad(),
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
        };
    }
}



