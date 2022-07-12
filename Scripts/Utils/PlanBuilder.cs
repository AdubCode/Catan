using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class PlanBuilder
{
    public static List<int> GetNone()
    {
        return new List<int>() { 0, 0, 0, 0, 0 };
    }

    public static List<int> GetRoad()
    {
        return new List<int>() { 1, 1, 0, 0, 0 };
    }

    public static List<int> GetSettlement()
    {
        return new List<int>() { 1, 1, 1, 1, 0 };
    }

    public static List<int> GetCity()
    {
        return new List<int>() { 0, 0, 0, 2, 3 };
    }

    public static List<int> GetDevelopmentCard()
    {
        return new List<int>() { 0, 0, 1, 1, 1 };
    }
}