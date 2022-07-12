using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResourcePickedPhone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    public MobilePlayerController mpc;

    public bool bank;
    public bool playerTrade;
    public ResourceType myType;
    public PlayerColors myColor;

    public void OnPointerDown(PointerEventData e)
    {
        Debug.Assert(mpc != null, "MPC is undefined for phone resources.");
        if (!bank) mpc.resourceInHand = myType;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        e.selectedObject = gameObject;
    }

    public void OnPointerUp(PointerEventData e)
    {
        Debug.Assert(mpc != null, "MPC is undefined for phone resources.");

        if (e.selectedObject == null) {
            Debug.Log("No selected object.");
            return;
        }

        ResourcePickedPhone rPickedPhone = e.selectedObject.GetComponent<ResourcePickedPhone>();
        if (e.selectedObject != null && rPickedPhone.bank)
        {
            mpc.CmdTradeWithBank((int)rPickedPhone.myType, (int)mpc.resourceInHand);
        }
        else if(e.selectedObject != null && rPickedPhone.playerTrade)
        {
            mpc.CmdTradeWithPlayer((int)rPickedPhone.myColor, (int)mpc.resourceInHand);
        }
    }
}
