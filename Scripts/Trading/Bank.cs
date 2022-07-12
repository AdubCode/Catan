using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bank{

    static int[] numberOfResourcesLeft;

    public static void SetupBank(int[] resourceList)
    {
        numberOfResourcesLeft = resourceList;
    }

    public static void AddToBank(ResourceType r, int num)
    {
        numberOfResourcesLeft[(int)r] += num;
    }

    public static void RemoveFromBank(ResourceType r, int num)
    {
        if (r == ResourceType.none) return;
        numberOfResourcesLeft[(int)r] -= num;
    }

    public static bool HasEnoughResources(ResourceType r, int num)
    {
        if (numberOfResourcesLeft[(int)r] > num) return true;

        return false;
    }

    public static int GiveAllResources(ResourceType r)
    {
        int ret = numberOfResourcesLeft[(int)r];
        numberOfResourcesLeft[(int)r] = 0;
        return ret;
    }
}
