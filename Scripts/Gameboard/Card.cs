using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum CardType
{
    Knight,
    Monopoly,
    RoadBuilding,
    YearOfPlenty,
    VictoryPoint
}

public enum VPCardType
{
    Chapel,
    University,
    GreatHall,
    Library,
    Market,
    none
}

public class Card{
    public CardType myCardType { get; private set; }
    public VPCardType myVPCardType { get; private set; }

    public int drawTurn;

    public Card(CardType c, VPCardType v)
    {
        myCardType = c;
        myVPCardType = v;
    }

    string[] CardText = new string[] {
        "Move the robber. Steal 1 resource from the owner of a settlement or city adjacent to the robber's new hex.",
        "When you play this card, announce 1 type of resource. All other players must give you all of their resources of that type.",
        "Place 2 new roads as if you had just build them.",
        "Take any 2 resources from the bank. Add them to your hand. They can be 2 of the same resource or 2 different resources",
        "Reveal this card on your turn if, with it, you reach the number of points required for victory.",
    };

    /// <summary>
    /// Gets the name of the development card
    /// </summary>
    public string GetName()
    {
        if (IsVictoryPoint()){
            return myVPCardType.ToString();
        }
        else
        {
            return myCardType.ToString();
        }
    }

    public bool IsVictoryPoint() {
        return myCardType == CardType.VictoryPoint;
    }

    public bool WasPlayedThisTurn() {
        return drawTurn == GameController.gc.turnNumber;
    }

    /// <summary>
    /// Plays the card
    /// </summary>
    public void PlayCard()
    {

        if (WasPlayedThisTurn() || !GameController.gc.playerRolled)
        {
            Debug.Log("Not playing card - " + WasPlayedThisTurn() + " - " + !GameController.gc.playerRolled);
            return;
        }

        Transform playerMat = GameController.gc.GetPlayerMat(GameController.gc.currentPlayer).transform;
        GameController.gc.AnimateDevelopmentCard(myCardType, playerMat);

        switch (myCardType)
        {
            case CardType.Knight:
                GameController.gc.KnightPlayed();
                break;
            case CardType.Monopoly:
                GameController.gc.ShowResourcePicker(false);
                break;
            case CardType.RoadBuilding:
                GameController.gc.RoadPlayed();
                break;
            case CardType.YearOfPlenty:
                GameController.gc.ShowResourcePicker(true);
                break;
            case CardType.VictoryPoint:
                GameController.gc.GetCurrentPlayer().playerVictoryPoints++;
                break;
        }

        GameController.gc.GetCurrentPlayer().RemoveDevelopmentCard(this);
        GameController.gc.UpdatePlayerUIs();
    }
}
