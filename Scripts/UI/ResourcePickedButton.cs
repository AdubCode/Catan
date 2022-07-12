using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePickedButton : MonoBehaviour {

    public ResourceType myType;

    public void Clicked()
    {
        GameController.gc.ResourcePicked(myType);
    }
}
