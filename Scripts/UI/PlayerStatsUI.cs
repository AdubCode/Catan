using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerStatsUI : MonoBehaviour
{

    public Player player;

    public Material[] playerTabMats;

    Transform endTurnButton;
    Transform rollDiceButton;
    Vector3 rollButtonPosition;
    Vector3 rollButtonHidingPosition;
    Vector3 turnButtonPosition;
    Vector3 turnButtonHidingPosition;


    TipPrompt helpPrompt;

    DevCardContainer devCardUI;

    private float speed = 5f;

    public float hidingOffset = 0f;

    void Start()
    {
        endTurnButton = transform.Find("EndTurnButton");
        rollDiceButton = transform.Find("RollDiceButton");

        devCardUI = GetTabContent(2).Find("DevCards").GetComponent<DevCardContainer>();
        Debug.Assert(devCardUI != null, "No dev card UI found on player mat.");

        helpPrompt = GetComponentInChildren<TipPrompt>();
        Debug.Assert(helpPrompt != null, "No tip prompt found on player mat.");

        rollButtonPosition = rollDiceButton.position;
        rollButtonHidingPosition = new Vector3(rollButtonPosition.x + hidingOffset, rollButtonPosition.y, rollButtonPosition.z);

        turnButtonPosition = endTurnButton.position;
        turnButtonHidingPosition = new Vector3(turnButtonPosition.x + hidingOffset, turnButtonPosition.y, turnButtonPosition.z);

        SetupInitialState();
    }

    public void SetPlayer(Player play)
    {
        Debug.Assert(play != null, "PlayerStatsUI:SetPlayer - `play` should not be null");
        player = play;

        CreateTradingComponents();
        SetStatsColor();
        UpdateUI();
    }

    private void SetStatsColor()
    {
        string playerColor = player.myColor.ToString();

        for (int i = 1; i <= 3; i++)
        {
            Transform tab = transform.Find("Tab" + i);
            Material matMaterial = new Material(playerTabMats[i - 1]);

            string tabSelector = null;
            switch (i)
            {
                default:
                case 1:
                    tabSelector = " ";
                    break;

                case 2:
                    tabSelector = " middleTab ";
                    break;

                case 3:
                    tabSelector = " rightTab ";
                    break;
            }

            tabSelector = "mats/player mat" + tabSelector + playerColor;
            Texture colorTabTexture = Resources.Load(tabSelector) as Texture2D;
            Debug.Assert(colorTabTexture != null, "Color tab doesn't exist for \"" + tabSelector + "\"");

            matMaterial.mainTexture = colorTabTexture;
            MeshRenderer matRenderer = tab.Find("Mat").GetComponent<MeshRenderer>();
            matRenderer.material = matMaterial;
        }

        SetInventoryItemColor();
    }

    private void SetInventoryItemColor() {
        string playerColor = player.myColor.ToString();
        Material playerMat = Resources.Load(playerColor) as Material;
        Debug.Assert(playerMat != null, "A player material for " + playerColor + " was not found.");

        Transform content = GetTabContent(1);
        content.Find("Item-road").GetComponent<MeshRenderer>().material = playerMat;
        content.Find("Item-settlement").GetComponent<MeshRenderer>().material = playerMat;
        content.Find("Item-city").GetComponent<MeshRenderer>().material = playerMat;
    }

    private void CreateTradingComponents()
    {
        Transform resources = GetTabContent(3).Find("Player Resources");
        Debug.Assert(resources != null, "Resource container for " + player.myColor.ToString() + " player not found");

        EndTradeDrag etd = gameObject.AddComponent<EndTradeDrag>();
        etd.player = player;

        StartTradeDrag tradeScript;

        tradeScript = resources.Find("BrickCard").gameObject.AddComponent<StartTradeDrag>();
        tradeScript.player = player;
        tradeScript.type = ResourceType.Brick;

        tradeScript = resources.Find("LumberCard").gameObject.AddComponent<StartTradeDrag>();
        tradeScript.player = player;
        tradeScript.type = ResourceType.Lumber;

        tradeScript = resources.Find("WoolCard").gameObject.AddComponent<StartTradeDrag>();
        tradeScript.player = player;
        tradeScript.type = ResourceType.Wool;

        tradeScript = resources.Find("GrainCard").gameObject.AddComponent<StartTradeDrag>();
        tradeScript.player = player;
        tradeScript.type = ResourceType.Grain;

        tradeScript = resources.Find("OreCard").gameObject.AddComponent<StartTradeDrag>();
        tradeScript.player = player;
        tradeScript.type = ResourceType.Ore;
    }

    private void SetupInitialState() {
        endTurnButton.position = turnButtonHidingPosition;
        rollDiceButton.position = rollButtonHidingPosition;
        helpPrompt.Hide();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void UpdateUI()
    {
        UpdateRoadandArmy();
        UpdateVictoryPoints();
        UpdateRoadsLeft();
        UpdateSettlementsLeft();
        UpdateCitiesLeft();
        UpdateDevCardCount();
        UpdateResourceCardCount();

        UpdateResources();
        UpdateTradeValues();

        UpdatePromptText();
        devCardUI.UpdateCards(player);
    }

    private void UpdatePromptText() {
        GameController gc = GameController.gc;
        if (gc == null || player == null) {
            return;
        }
        Debug.Assert(helpPrompt != null, "No tip prompt found on player mat.");
        GameState state = gc.currentState;
        bool isCurrentPlayer = gc.GetCurrentPlayer() == player;

        switch(state) {
            default:
                helpPrompt.Hide();
                break;

            case GameState.clearingTooManyCards:
                int discardCount = gc.GetPlayerDiscardAmount(player);
                if(discardCount > 0) {
                    //Debug.Log("here-" + discardCount);
                    helpPrompt.Show("<nobr>You must discard " + discardCount + " cards.</nobr>", true);
                } else if(isCurrentPlayer) {
                    if (GameController.gc.TooManyCards()) {
                        helpPrompt.Show("Waiting for another player <nobr>to discard...</nobr>");
                    } else if(gc.robber.needsToMove) {
                        helpPrompt.Show("You must move the Robber to <nobr>another tile.</nobr>");
                    } else {
                        helpPrompt.Hide();
                    }

                }else {
                    helpPrompt.Hide();
                }
                break;

            case GameState.setup:
                HandleSetupPrompt();
                break;

            case GameState.gameplay:
                if (isCurrentPlayer && !gc.playerRolled) {
                    helpPrompt.Show("Your turn!");
                } else {
                    helpPrompt.Hide();
                }
                break;
        }
    }

    private void HandleSetupPrompt() {
        bool isCurrentPlayer = GameController.gc.GetCurrentPlayer() == player;
        if (!isCurrentPlayer) {
            helpPrompt.Hide();
            return;
        }

        bool hasPlacedFirstSettlement = player.numberOfSettlementsLeft == 4;
        bool hasPlacedFirstRoad = player.numberOfRoadsLeft == 14;

        bool isOnFirstRound = GameController.gc.turnNumber < GameController.gc.players.Count;

        bool hasPlacedSecondSettlement = player.numberOfSettlementsLeft == 3;
        bool hasPlacedSecondRoad = player.numberOfRoadsLeft == 13;

        if (isOnFirstRound) {
            if(!hasPlacedFirstSettlement) {
                helpPrompt.Show("Place your first settlement by dragging one to the board.");
            } else if (!hasPlacedFirstRoad) {
                helpPrompt.Show("Place your first road by dragging one to the board near your settlement.");
            } else {
                helpPrompt.Show("Your turn is over!");
            }
        } else {
            if(!hasPlacedSecondSettlement) {
                helpPrompt.Show("Place a second settlement on the board.");
            } else if (!hasPlacedSecondRoad) {
                helpPrompt.Show("Place a road near the settlement you just placed.");
            } else {
                helpPrompt.Show("Your turn is over!");
            }
        }
    }

    private Transform GetTab(int index)
    {
        Transform tab = transform.Find("Tab" + index);
        Debug.Assert(tab != null, "Tab " + index + " could not be found.");

        return tab;
    }

    private Transform GetTabContent(int index)
    {
        Transform tab = GetTab(index);
        Transform content = tab.Find("Content");
        Debug.Assert(content != null, "Tab " + index + " content could not be found.");

        return content;
    }


    #region Updating Number Tickers

    private void UpdateVictoryPoints()
    {
        GetTabContent(1).Find("#victoryPoints").GetComponent<NumberCounter>().SetNumber(player.playerVictoryPoints);
    }

    private void UpdateRoadsLeft()
    {
        GetTabContent(1).Find("#leftRoad").GetComponent<NumberCounter>().SetNumber(player.numberOfRoadsLeft);
    }

    private void UpdateSettlementsLeft()
    {
        GetTabContent(1).Find("#leftSettlement").GetComponent<NumberCounter>().SetNumber(player.numberOfSettlementsLeft);
    }

    private void UpdateCitiesLeft()
    {
        GetTabContent(1).Find("#leftCity").GetComponent<NumberCounter>().SetNumber(player.numberOfCitiesLeft);
    }

    private void UpdateDevCardCount()
    {
        Transform devCardUI = GetTab(2).Find("TabLabel").Find("numDevCards");
        Debug.Assert(devCardUI != null, "No UI found for 'num dev cards' for " + player.myColor.ToString() + " player.");

        devCardUI.GetComponent<NumberCounter>().SetNumber(player.myDevelopmentCards.Count);
    }

    private void UpdateResourceCardCount()
    {
        Transform resourceCardUi = GetTab(3).Find("TabLabel").Find("numResourceCards");
        Debug.Assert(resourceCardUi != null, "No UI found for 'num resource cards' for " + player.myColor.ToString() + " player.");

        resourceCardUi.GetComponent<NumberCounter>().SetNumber(player.GetTotalNumberOfResources());
    }


    private void UpdateResources()
    {
        Transform resourceContainer = GetTabContent(3).Find("Player Resources");

        Debug.Assert(resourceContainer != null, "Expected resource container to not be null for resources");

        resourceContainer.Find("brickCount").GetComponentInChildren<NumberCounter>().SetNumber(player.myResourceCards[(int)ResourceType.Brick]);
        resourceContainer.Find("lumberCount").GetComponentInChildren<NumberCounter>().SetNumber(player.myResourceCards[(int)ResourceType.Lumber]);
        resourceContainer.Find("woolCount").GetComponentInChildren<NumberCounter>().SetNumber(player.myResourceCards[(int)ResourceType.Wool]);
        resourceContainer.Find("grainCount").GetComponentInChildren<NumberCounter>().SetNumber(player.myResourceCards[(int)ResourceType.Grain]);
        resourceContainer.Find("oreCount").GetComponentInChildren<NumberCounter>().SetNumber(player.myResourceCards[(int)ResourceType.Ore]);
    }

    private void UpdateTradeValues(){
        Transform resourceContainer = GetTabContent(3).Find("Player Resources");
        Debug.Assert(resourceContainer != null, "Expected resource container to not be null for trade values");

        resourceContainer.Find("BrickCard").Find("BrickValue").GetComponentInChildren<TextMeshPro>().text = player.tradeValues[(int)ResourceType.Brick] + ":1";
        resourceContainer.Find("LumberCard").Find("WoodValue").GetComponentInChildren<TextMeshPro>().text = player.tradeValues[(int)ResourceType.Lumber] + ":1";
        resourceContainer.Find("WoolCard").Find("WoolValue").GetComponentInChildren<TextMeshPro>().text = player.tradeValues[(int)ResourceType.Wool] + ":1";
        resourceContainer.Find("GrainCard").Find("GrainValue").GetComponentInChildren<TextMeshPro>().text = player.tradeValues[(int)ResourceType.Grain] + ":1";
        resourceContainer.Find("OreCard").Find("OreValue").GetComponentInChildren<TextMeshPro>().text = player.tradeValues[(int)ResourceType.Ore] + ":1";
    }

    #endregion

    private void UpdateRoadandArmy()
    {
        GetTabContent(1).Find("LongestRoad").gameObject.SetActive(player.longestRoad);
        GetTabContent(1).Find("LargestArmy").gameObject.SetActive(player.largestArmy);
    }

	public void RollDice() {
		GameController.gc.RollDiceButton(true);
	}

    void Update()
    {
        UpdateButtonPlacement();
        UpdatePromptText();
    }

    /// <summary>
    /// Repositions the 'roll dice' and 'end turn' buttons.
    /// </summary>
    private void UpdateButtonPlacement()
    {
        if (player == null || GameController.gc.GetCurrentPlayer() != player)
        {
            endTurnButton.position =  Vector3.Lerp(endTurnButton.position, turnButtonHidingPosition, Time.deltaTime * speed);
            rollDiceButton.position =  Vector3.Lerp(rollDiceButton.position, rollButtonHidingPosition, Time.deltaTime * speed);
            return;
        }


        if (GameController.gc.currentState == GameState.setup || GameController.gc.playerRolled)
        {
            rollDiceButton.position = Vector3.Lerp(rollDiceButton.position, rollButtonHidingPosition, Time.deltaTime * speed);
            if (!GameController.gc.isWaitingForRoll && !GameController.gc.robber.needsToMove)
            {
                endTurnButton.position = Vector3.Lerp(endTurnButton.position, turnButtonPosition, Time.deltaTime * speed);
            }
        }
        else
        {
            endTurnButton.position = Vector3.Lerp(endTurnButton.position, turnButtonHidingPosition, Time.deltaTime * speed);
            rollDiceButton.position = Vector3.Lerp(rollDiceButton.position, rollButtonPosition, Time.deltaTime * speed);
        }
    }
}
