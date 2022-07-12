using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerResourceUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler {

    public bool bank;
    public ResourceType myType;
    public int playerNumber;

    public void OnPointerDown(PointerEventData e)
    {
        if(!bank)GameController.gc.SetResourceInHand(myType, playerNumber);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        e.selectedObject = gameObject;
    }

    public void OnPointerUp(PointerEventData e)
    {
        PlayerResourceUI pRUI = e.selectedObject.GetComponent<PlayerResourceUI>();
        if (e.selectedObject != null && pRUI.bank)
        {
            GameController.gc.TradeBank(pRUI.myType);
        }
        else
        {
            if (e.selectedObject != null  && GameController.gc.playerGivngResource != pRUI.playerNumber)
            {
                GameController.gc.TradeResource(pRUI.playerNumber);
            }
        }
    }
}
