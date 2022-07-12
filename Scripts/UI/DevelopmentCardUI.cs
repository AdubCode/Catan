using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DevelopmentCardUI : MonoBehaviour, IPointerDownHandler {

    [HideInInspector] public Card myCard;
    [HideInInspector] public bool phoneCard;
    [HideInInspector] public Player player;

    public void OnPointerDown(PointerEventData e)
    {
        if (phoneCard) player.myMobileController.CmdPlayDevelopmentCard(player.myDevelopmentCards.IndexOf(myCard));
        else myCard.PlayCard();
    }
}
