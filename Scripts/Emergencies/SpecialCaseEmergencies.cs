using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Emergency: AI has 5 settlements
/// Response: AI must build a city
/// </summary>
public class NoSettlementEmergency : Emergency
{
    public override bool EvaluateConditions(Player player)
    {
        return player.numberOfSettlementsLeft <= 0;
    }

    public override void HandleResponse(Player player)
    {
        player.myAI.currentBuildProjects = new List<List<int>>() {
            PlanBuilder.GetCity(),
            PlanBuilder.GetNone(),
            PlanBuilder.GetNone(),
        };
    }
}
