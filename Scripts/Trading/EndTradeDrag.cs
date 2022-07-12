using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTradeDrag : MonoBehaviour
{
    public Player player;

    void Update()
    {
        GameController gc = GameController.gc;

        if (gc.someoneIsTrading && Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = 1 << LayerMask.NameToLayer("UI");

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    if (gc.currentState != GameState.clearingTooManyCards)
                    {
                        ConfirmTrade(hit);
                        DestroyDraggedCard();
                    }
                    else
                    {
                        CancelDraggedCard();
                        EndTrading();
                    }
                }
            }
            else
            {
                if (gc.currentState == GameState.clearingTooManyCards)
                {
                    ConfirmCardRemoval();
                    DestroyDraggedCard();
                }
                else
                {
                    CancelDraggedCard();
                }
                EndTrading();
            }
        }
    }

    void DestroyDraggedCard()
    {
        // real gross
        GameObject go = GameObject.Find("DraggedCard");
        if (go == null){ return; }
        FollowMouse mouseFx = go.GetComponent<FollowMouse>();
        mouseFx.Remove();
    }

    void CancelDraggedCard()
    {
        GameObject go = GameObject.Find("DraggedCard");
        if (go == null){ return; }
        FollowMouse mouseFx = go.GetComponent<FollowMouse>();
        mouseFx.Cancel();
    }

    void ConfirmCardRemoval()
    {
        GameController.gc.ThrowCardAway();
        EndTrading();
    }

    void ConfirmTrade(RaycastHit hit)
    {
        bool didTrade = false;
        GameController gc = GameController.gc;
        int thisPlayerNum = gc.GetPlayerNumber(player);
        // Check if player is trying to trade with themselves
        if (GameController.gc.playerGivngResource == thisPlayerNum)
        {
            // If so, check if they are trying to trade to a bank item
            bool isTradingToBank = CheckIfInBank(hit.collider.gameObject);
            if (isTradingToBank)
            {
                ResourceType desiredResource = GetBankItem(hit.collider.gameObject);
                didTrade = GameController.gc.TradeBank(desiredResource);
            }
        }
        else
        {
            didTrade = gc.TradeResource(thisPlayerNum);
        }

        if (didTrade) {
            DestroyDraggedCard();            
        } else {
            CancelDraggedCard();                        
        }

        EndTrading();
    }

    bool CheckIfInBank(GameObject thing)
    {
        // This is tied a little too tightly to the PlayerMat prefab
        Transform bank = transform.Find("Tab3").Find("Content").Find("Bank");
        Debug.Assert(bank != null);

        return thing == bank || thing.transform.IsChildOf(bank);
    }

    ResourceType GetBankItem(GameObject thing)
    {
        switch (thing.name)
        {
            default:
                Debug.Assert(false, "No material for \"" + thing.name + "\"");
                return ResourceType.none;

            case "Brick":
                return ResourceType.Brick;

            case "Grain":
                return ResourceType.Grain;

            case "Lumber":
                return ResourceType.Lumber;

            case "Ore":
                return ResourceType.Ore;

            case "Wool":
                return ResourceType.Wool;
        }
    }

    void EndTrading()
    {
        GameController.gc.EndResourceTrading();
    }
}
