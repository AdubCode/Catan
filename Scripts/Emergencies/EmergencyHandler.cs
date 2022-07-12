using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for determining the state of emergency (if any) for the given player.
/// </summary>
public static class EmergencyHandler
{
    static List<Emergency> types = new List<Emergency>() { };
    static bool didInit = false;

    static void InitTypes()
    {
        if (EmergencyHandler.didInit) return;

        EmergencyHandler.types.Add(new BuildingEmergency1());
        EmergencyHandler.types.Add(new BuildingEmergency2());
        EmergencyHandler.types.Add(new BuildingEmergency3());
        EmergencyHandler.types.Add(new BuildingEmergency5());
        EmergencyHandler.types.Add(new BuildingEmergency6());


        EmergencyHandler.types.Add(new LongestRoadEmergency1());
        EmergencyHandler.types.Add(new LongestRoadEmergency1a());
        EmergencyHandler.types.Add(new LongestRoadEmergency2());
        EmergencyHandler.types.Add(new LongestRoadEmergency2a());
        EmergencyHandler.types.Add(new LongestRoadEmergency3());
        EmergencyHandler.types.Add(new LongestRoadEmergency5());
        EmergencyHandler.types.Add(new LongestRoadEmergency5a());

        EmergencyHandler.types.Add(new DisconnectRoadEmergency());
        EmergencyHandler.types.Add(new ConnectRoadEmergency());

        EmergencyHandler.types.Add(new NoSettlementEmergency());
    }

    public static Emergency GetEmergency(Player player)
    {
        if (!EmergencyHandler.didInit)
        {
            EmergencyHandler.InitTypes();
        }

        return EmergencyHandler.types.Find((Emergency em) =>
        {
            return em.EvaluateConditions(player);
        });
    }
}
