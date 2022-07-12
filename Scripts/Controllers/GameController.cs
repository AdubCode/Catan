using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PlayTable.Unity;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Animations;
using TMPro;

public enum GameState
{
    setup,
    gameplay,
    trading,
    gameOver,
    playingDevelopmentCard,
    clearingTooManyCards,
    pickingRandomCard,
    tutorial
}

public class GameController : NetworkBehaviour {

    public static GameController gc;

    const float HARBOR_BONUS = 2.5f;
    const float RESOURCE_IMPORTANCE = 2.5f;

    public Mesh cityMesh;
    public Text currentPlayerGUI;
    public Text rollGUI;

    public PlayerStatsUI[] playerStatGUIs;

    public Text[] bankTextGUI;
    [HideInInspector] public GameState currentState;

    [HideInInspector] public List<Player> players;
    public int currentPlayer;

    [HideInInspector] public int turnNumber;

    [HideInInspector] public bool playerRolled;

    [HideInInspector] public bool gameStarted;

    private Player nullPlayer = new Player(PlayerColors.None);

    private bool isGoingToNextTurn = false;

    //stuff for trading
    ResourceType resourceInHand;
    public int playerGivngResource;

    public bool someoneIsTrading = false;

    //stuff for development cards
    public List<Card> developmentCardList;
    public List<Material> developmentCardMaterial;
    public GameObject resourcePickerGUI;
    public GameObject handGUI;
    bool monopolyPlayed;
    int maxResourceClicks;

    //stuff for a roll of 7
    [HideInInspector] public Robber robber;
    public Transform randomCardUI;
    List<int> maxCardCount;
    [HideInInspector] public bool movingFromKnight;

    //winning stuff
    public GameObject GameOverUI;
    public RawImage winnersMat;

    //AI stuff
    public AI ai;
    [HideInInspector] public float[] generalFrequencyFactor;
    public List<Corner> boardsCorners;
    public List<Edge> boardsEdges;
    public List<Tile> boardsTiles;
    public bool animationPlaying;

    public MobilePlayerController mobileController;
    public GameObject mobileUI;


    public Dice gameDice;
    public bool isWaitingForRoll;

    private Material alphaGlowMat;


    //Emoji Stuff
    public GameObject emojiBar;


    //Stuff for Last Gameboard
    int roadsGivenOut;

    void Awake()
    {
        gc = this;

        alphaGlowMat = Resources.Load("GlowPulseAlpha") as Material;
    }


    void Update()
    {
        if (!gameStarted) return;

        if (mobileController == null)
        {
            mobileController = FindObjectOfType<MobilePlayerController>();
        }
    }

    PlayerColors[] GetUsedColors()
    {
        if (players == null) {
            return new PlayerColors[0];
        }

        PlayerColors[] usedColors = new PlayerColors[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            bool set = false;
            for (int player = 0; player < players.Count; player++)
            {
                if (set) continue;
                Player currentPlayer = players[player];
                if (currentPlayer.myUIPos == i)
                {
                    set = true;
                    usedColors[player] = currentPlayer.myColor;
                }
            }
        }

        return usedColors;
    }

    public void UpdateMobileColors() {
        if (players == null) {
            return;
        }

        PlayerColors[] colors = GetUsedColors();

        players.ForEach((Player play)=>{
            play.UpdatePlayerColors(colors);
        });
    }

    void SetPlayerCardColors()
    {
        PlayerColors[] usedColors = new PlayerColors[players.Count];

        for (int i = 0; i < playerStatGUIs.Length; i++)
        {
            PlayerStatsUI statUI = playerStatGUIs[i];
            bool set = false;
            for (int player = 0; player < players.Count; player++)
            {
                if (set) continue;
                Player currentPlayer = players[player];
                if (currentPlayer.myUIPos == i)
                {
                    statUI.Show();
                    statUI.SetPlayer(currentPlayer);
                    currentPlayer.ui = statUI;
                    set = true;

                    usedColors[player] = currentPlayer.myColor;
                }
            }
            if (!set) statUI.Hide();
        }
    }

    void UpdateCurrentPlayerText() {
        Player current = GetCurrentPlayer();
        currentPlayerGUI.text = current.myColor + " player's turn!" + "\n" + (current.isAI ? "AI" : "Human" ) +" | Victory Points: " + GetCurrentPlayer().playerVictoryPoints + (current.longestRoad ? " | Longest Road" : "");
    }

    void SpawnObjects()
    {
        if (SceneManager.GetActiveScene().name == "NewGameUI" || SceneManager.GetActiveScene().name == "MainGame")
        {
            NetworkServer.SpawnObjects();
        }
    }

    public void StartGame(List<Player> p, Robber r, List<Card> devList, List<Corner> corners, List<Tile> tiles ,int[] boardFrequencyFactor)
    {
        gameStarted = true;
        players = p;
        robber = r;
        robber.gameObject.SetActive(true);
        movingFromKnight = false;
        currentPlayer = Random.Range(0,p.Count);
        turnNumber = 0;
        developmentCardList = devList;
        Bank.SetupBank(new int[] {19,19,19,19,19});
        currentState = GameState.setup;
        maxCardCount = new List<int>() { 7, 7, 7, 7 };
        boardsCorners = corners;
        boardsTiles = tiles;

        UpdateCurrentPlayerText();

        generalFrequencyFactor = new float[] { 0, 0, 0, 0, 0 };
        for(int i = 0; i < generalFrequencyFactor.Length; i++)
        {
            generalFrequencyFactor[i] = (float)boardFrequencyFactor[i] / boardFrequencyFactor.Sum();
        }
        CalculateCornersAttractivness();

        TutorialController.ShowText(0,GetCurrentPlayer());

        SetPlayerCardColors();
        UpdateMobileColors();
        UpdatePlayerUIs();

        roadsGivenOut = 0;
        //THIS IS FOR The Last Gameboard where it skips setup
        //if (GetCurrentPlayer().isAI) ai.TakeTurn(GetCurrentPlayer());
        ai.TakeTurn(GetCurrentPlayer());
    }

    private void CollectEdges() {
        List<Edge> compiledEdgeList = new List<Edge>();
        boardsCorners.ForEach((Corner corner)=>{
            corner.adjacentEdges.ForEach((Edge edge)=>{
                if (compiledEdgeList.Contains(edge)) {
                    return;
                }
                compiledEdgeList.Add(edge);
            });
        });

        boardsEdges = compiledEdgeList;
    }

    public void UpdatePlayerUIs() {
        players.ForEach((Player play)=>{
            play.UpdateStatsUI();
        });
    }

    #region Stuff for Corners

    public Material CornerClickedDuringSetup(Corner c)
    {
        Material retMat = null;
        bool setMat = false;
        if (turnNumber < players.Count)
        {
            setMat = GetCurrentPlayer().numberOfSettlementsLeft == 5;
            TutorialController.ShowText(1, GetCurrentPlayer());
        }
        else
        {
            if (GetCurrentPlayer().numberOfSettlementsLeft == 4)
            {
                c.placedSecond = true;
                for (int i = 0; i < c.adjacentTiles.Count; i++)
                {
                    if (c.adjacentTiles[i].myResourceType == ResourceType.none) continue;
                    TutorialController.ShowText(4, GetCurrentPlayer());
                    Transform from = c.adjacentTiles[i].transform;
                    Transform to = GameObject.Find("PlayerMat" + (GetCurrentPlayer().myUIPos + 1)).transform.Find("Tab1").transform.Find("Mat").transform;
                    AnimateCard(c.adjacentTiles[i].myResourceType, from, to, 1);
                    players[currentPlayer].AddResource(c.adjacentTiles[i].myResourceType, 1);
                    Bank.RemoveFromBank(c.adjacentTiles[i].myResourceType, 1);
                }
                setMat = true;
            }
        }

        if (setMat)
        {
            retMat = GetCurrentPlayerMaterial();

            int rand = Random.Range(0, SoundEffects.soundEffects["Build Settlement"].Count);
            AudioPool.PlayLoop(SoundEffects.soundEffects["Build Settlement"][rand], "Build Settlement", 1, 1, SoundEffects.soundEffects["Build Settlement"][rand].length, false);
            c.hasSettlement = true;
            CalculatePlayerSumOfRevenues(c);
            CalculatePlayerBankDiscount(c);
        }

        return retMat;
    }

    /// <summary>
    /// This gets called when a corner is clicked checks to make sure that a settlement or city can be placed returns the mat of the player if it can be placed.
    /// </summary>
    /// <param name="c">The corner class that is being clicked</param>
    public Material CornerClicked(Corner c)
    {

        Material retMat = null;
        if (currentState == GameState.setup)
        {

            retMat = CornerClickedDuringSetup(c);
            return retMat;
        }
        else
        {
            if (!playerRolled) return null;
            Player current = GetCurrentPlayer();

            if (!c.HasLeadingRoads(current)) return retMat;

            if (c.hasSettlement)
            {
                if (current.HasResource(ResourceType.Grain, 2) && current.HasResource(ResourceType.Ore, 3) && current.numberOfCitiesLeft > 0)
                {
                    current.RemoveResource(ResourceType.Grain, 2);
                    current.RemoveResource(ResourceType.Ore, 3);

                    Bank.AddToBank(ResourceType.Grain, 2);
                    Bank.AddToBank(ResourceType.Ore, 3);

                    retMat = GetCurrentPlayerMaterial();

                    int rand = Random.Range(0, SoundEffects.soundEffects["Build City"].Count);
                    AudioPool.PlayLoop(SoundEffects.soundEffects["Build City"][rand], "Build City", 1, 1, SoundEffects.soundEffects["Build City"][rand].length, false);
                    c.hasCity = true;
                    c.hasSettlement = false;
                    c.GetComponent<MeshFilter>().mesh = cityMesh;
                    CalculatePlayerSumOfRevenues(c);
                }
            }
            else
            {
                if (current.HasResource(ResourceType.Lumber, 1) && current.HasResource(ResourceType.Brick, 1) && current.HasResource(ResourceType.Grain, 1) && current.HasResource(ResourceType.Wool, 1) && current.numberOfSettlementsLeft > 0)
                {
                    current.RemoveResource(ResourceType.Lumber, 1);
                    current.RemoveResource(ResourceType.Brick, 1);
                    current.RemoveResource(ResourceType.Grain, 1);
                    current.RemoveResource(ResourceType.Wool, 1);

                    Bank.AddToBank(ResourceType.Lumber, 1);
                    Bank.AddToBank(ResourceType.Brick, 1);
                    Bank.AddToBank(ResourceType.Grain, 1);
                    Bank.AddToBank(ResourceType.Wool, 1);

                    c.hasSettlement = true;
                    retMat = MasterController.mc.playerColorMaterials.Find((x) => x.name == current.myColor.ToString());
                    int rand = Random.Range(0, SoundEffects.soundEffects["Build Settlement"].Count);
                    AudioPool.PlayLoop(SoundEffects.soundEffects["Build Settlement"][rand], "Build Settlement", 1, 1, SoundEffects.soundEffects["Build Settlement"][rand].length, false);
                    CalculatePlayerBankDiscount(c);
                    CalculatePlayerSumOfRevenues(c);

                    List<PlayerColors> cols = new List<PlayerColors>();
                    for(int i = 0; i < c.adjacentEdges.Count; i++)
                    {
                        if (c.adjacentEdges[i].hasRoad) cols.Add(c.adjacentEdges[i].currentColor);
                    }

                    if (cols.Count != cols.Distinct().Count()) CalculateLongestRoad();

                }
            }
        }
        return retMat;
    }

    void CalculatePlayerSumOfRevenues(Corner c)
    {
        for(int i = 0; i < c.adjacentTiles.Count; i++)
        {
            GetCurrentPlayer().mySumOfRevenues += c.adjacentTiles[i].myProbabilityOfRevenue;
            if(c.adjacentTiles[i].myResourceType != ResourceType.none) GetCurrentPlayer().myResourceRevenues[(int)c.adjacentTiles[i].myResourceType] += c.adjacentTiles[i].myProbabilityOfRevenue;
        }
    }

    /// <summary>
    /// Checks the corner c if its on a port. If it is make the bank have a discount.
    /// </summary>
    /// <param name="c">The corner being checked to see if its on port</param>
    void CalculatePlayerBankDiscount(Corner c)
    {
        for (int e = 0; e < c.adjacentEdges.Count; e++)
        {
            if (c.adjacentEdges[e].isHarbor)
            {
                if (c.adjacentEdges[e].harborType == ResourceType.three)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (GetCurrentPlayer().tradeValues[i] == 4) GetCurrentPlayer().tradeValues[i] = 3;
                    }
                }
                else if (c.adjacentEdges[e].harborType == ResourceType.Brick)
                {
                    GetCurrentPlayer().tradeValues[0] = 2;
                }
                else if (c.adjacentEdges[e].harborType == ResourceType.Grain)
                {
                    GetCurrentPlayer().tradeValues[3] = 2;
                }
                else if (c.adjacentEdges[e].harborType == ResourceType.Lumber)
                {
                    GetCurrentPlayer().tradeValues[1] = 2;
                }
                else if (c.adjacentEdges[e].harborType == ResourceType.Ore)
                {
                    GetCurrentPlayer().tradeValues[4] = 2;
                }
                else if (c.adjacentEdges[e].harborType == ResourceType.Wool)
                {
                    GetCurrentPlayer().tradeValues[2] = 2;
                }
            }
        }
    }
    #endregion

    #region Edge Stuff
    /// <summary>
    /// Checks to see if you can put a road on this edge. Returns the players mat if you can.
    /// </summary>
    /// <param name="e">Edge class that is being checked</param>
    public Material EdgeClicked(Edge e)
    {
        Material retMat = null;
        if (currentState == GameState.setup) //this makes it so roads are free
        {
            if (turnNumber < players.Count) //first round of turns
            {
                if (GetCurrentPlayer().numberOfRoadsLeft == 15) //checks to see if a road has already been placed if it hasn't continue
                {

                    for (int i = 0; i < e.adjacentCorners.Count; i++) //checks each adjacent corner to see if theres a settlement there
                    {

                        if (e.adjacentCorners[i].hasSettlement && e.adjacentCorners[i].currentColor == GetCurrentPlayer().myColor) //if there is a settlement there and the color is the current player color
                        {

                            GetCurrentPlayer().roadLengths[0].Add(e);
                            TutorialController.ShowText(2, GetCurrentPlayer());

                            return GetCurrentPlayerMaterial();
                        }
                    }
                }
            }
            else //second round of turns
            {

                if (GetCurrentPlayer().numberOfRoadsLeft == 14)
                {
                    for (int i = 0; i < e.adjacentCorners.Count; i++) //checks each adjacent corner to see if theres a settlement there
                    {
                        if (e.adjacentCorners[i].hasSettlement && e.adjacentCorners[i].currentColor == GetCurrentPlayer().myColor && e.adjacentCorners[i].placedSecond) //if there is a settlement there and the color is the current player color and the settlement is placed second
                        {
                            List<Edge> edges = CalculateIfEdgeConnects(e);
                            if(edges.Count == 0) // didn't find any roads
                            {
                                GetCurrentPlayer().roadLengths[1].Add(e);
                            }
                            else //found roads
                            {
                                GetCurrentPlayer().roadLengths[0].Add(e);
                                GetCurrentPlayer().roadLengths.RemoveAt(1);
                            }
                            TutorialController.ShowText(5, GetCurrentPlayer());
                            return GetCurrentPlayerMaterial();
                        }
                    }
                }
            }
        }else if(currentState == GameState.gameplay || currentState == GameState.clearingTooManyCards)
        {
            if (!playerRolled){
                return null;
            }
            bool canPlace = false;
            for(int c = 0; c < e.adjacentCorners.Count; c++)
            {
                for(int i = 0; i < e.adjacentCorners[c].adjacentEdges.Count; i++)
                {
                    if (e.adjacentCorners[c].adjacentEdges[i].hasRoad && e.adjacentCorners[c].adjacentEdges[i].currentColor == GetCurrentPlayer().myColor) canPlace = true;
                }
            }

            if (!canPlace) {
                return retMat;
            }
            //check to see if the player has the resources
            if (GetCurrentPlayer().numberOfFreeRoads > 0)
            {
                GetCurrentPlayer().numberOfFreeRoads--;

                PutRoadInList(e);
                return GetCurrentPlayerMaterial();
            }
            else
            {
                if (GetCurrentPlayer().HasResource(ResourceType.Lumber, 1) && GetCurrentPlayer().HasResource(ResourceType.Brick, 1) && GetCurrentPlayer().numberOfRoadsLeft > 0)
                {
                    GetCurrentPlayer().RemoveResource(ResourceType.Lumber, 1);
                    GetCurrentPlayer().RemoveResource(ResourceType.Brick, 1);

                    Bank.AddToBank(ResourceType.Lumber, 1);
                    Bank.AddToBank(ResourceType.Brick, 1);

                    PutRoadInList(e);

                    return GetCurrentPlayerMaterial();
                }
            }
        }

        return retMat;
    }

    Material GetCurrentPlayerMaterial() {
        string currentPlayerColor = GetCurrentPlayer().myColor.ToString();
        Material playerMat = MasterController.mc.playerColorMaterials.Find((x) => x.name == currentPlayerColor);
        Debug.Assert(playerMat != null, "No road mat found for " + currentPlayerColor + " player.");
        return playerMat;
    }

    List<Edge> CalculateIfEdgeConnects(Edge checkEdge)
    {
        List<Edge> edgeList = new List<Edge>();

        for(int c = 0; c < checkEdge.adjacentCorners.Count; c++)
        {
            Corner corner = checkEdge.adjacentCorners[c];
            for(int e = 0; e < corner.adjacentEdges.Count; e++)
            {
                if (corner.adjacentEdges[e].hasRoad && corner.adjacentEdges[e].currentColor == GetCurrentPlayer().myColor) edgeList.Add(corner.adjacentEdges[e]);
            }
        }

        return edgeList;
    }

    void PutRoadInList(Edge e)
    {
        List<Edge> checkList = CalculateIfEdgeConnects(e);

        bool inFirstList = false;
        bool inSecondList = false;

        for(int i = 0; i < checkList.Count; i++)
        {
            if (GetCurrentPlayer().roadLengths[0].Contains(checkList[i])) inFirstList = true;
            else if (GetCurrentPlayer().roadLengths.Count >= 2 && GetCurrentPlayer().roadLengths[1].Contains(checkList[i])) inSecondList = true;
        }

        if(inFirstList && inSecondList)
        {
            //Debug.Log("Merging lists to make 1");
            for(int i = 0; i < GetCurrentPlayer().roadLengths[1].Count; i++)
            {
                GetCurrentPlayer().roadLengths[0].Add(GetCurrentPlayer().roadLengths[1][i]);
            }
            GetCurrentPlayer().roadLengths.RemoveAt(1);

        }else if (inFirstList)
        {
            //Debug.Log("Adding to the first list");
            GetCurrentPlayer().roadLengths[0].Add(e);
        }else if (inSecondList)
        {
            //Debug.Log("Adding to the Second list");
            GetCurrentPlayer().roadLengths[1].Add(e);
        }
    }
    #endregion

    /// <summary>
    /// Gets the player whos turn it is
    /// </summary>
    public Player GetCurrentPlayer()
    {
        bool useNullPlayer = players == null || currentPlayer < 0 || currentPlayer >= players.Count;
        return useNullPlayer ? nullPlayer : players[currentPlayer];
    }

    public void EndTurnButton()
    {
        if (GetCurrentPlayer().isAI) return;
        GotoNextTurn();
    }

    public void GotoNextTurn()
    {
        if (isGoingToNextTurn) return;
        isGoingToNextTurn = true;
        TogglePossibleConstructs(GetCurrentPlayer(), false);
        StartCoroutine(TransitionToNextTurn());
    }

    IEnumerator TransitionToNextTurn()
    {
        int actualPlayer = currentPlayer;
        currentPlayer = -1;
        yield return new WaitForSeconds(1f);
        currentPlayer = actualPlayer;

        EndTurn();
        isGoingToNextTurn = false;
    }

    /// <summary>
    /// Ends the current players turn and goes onto the next.
    /// </summary>
    public void EndTurn()
    {
        if (currentState == GameState.playingDevelopmentCard || currentState == GameState.trading) return;

        if(currentState == GameState.setup) //check to see if the housed and roads have been placed
        {
            bool canMoveOn = true;
            if (turnNumber < players.Count)
            {
                if (GetCurrentPlayer().numberOfSettlementsLeft != 4) canMoveOn = false;
                if (GetCurrentPlayer().numberOfRoadsLeft != 14) canMoveOn = false;
            }else
            {
                if (GetCurrentPlayer().numberOfSettlementsLeft != 3) canMoveOn = false;
                if (GetCurrentPlayer().numberOfRoadsLeft != 13) canMoveOn = false;
            }

            if (canMoveOn)
            {
                playerRolled = true;
            }
        }

        if (!CalculateCanEndTurn()) return;

        turnNumber++;

        playerRolled = false;
        isWaitingForRoll = false;

        rollGUI.text = "Roll Dice";
        if (currentState == GameState.setup)
        {
            if (turnNumber < players.Count) currentPlayer++;
            else if (turnNumber > players.Count && (turnNumber < players.Count*2)) currentPlayer--;
        }
        else
        {
            currentPlayer++;
        }

        currentPlayer = (currentPlayer + players.Count) % players.Count;
        if (currentState == GameState.setup)
        {
            if (turnNumber < players.Count) TutorialController.ShowText(0, GetCurrentPlayer());
            else TutorialController.ShowText(3, GetCurrentPlayer());
        }


        if (turnNumber > (players.Count * 2) - 1)
            currentState = GameState.gameplay;

        for(int i = 0; i < players.Count; i++)
        {
            if (players[i].playerVictoryPoints >= 10)
            {
                GameOver(players[i]);
                return;
            }
        }

        CalculateCornersAttractivness();
        CalculateCornerCompetition();

        //THIS IS FOR The Last Gameboard take away (|| currentState == GameState.setup) if you want regular game
        if (GetCurrentPlayer().isAI || currentState == GameState.setup) ai.TakeTurn(GetCurrentPlayer());

        UpdateCurrentPlayerText();
        UpdatePlayerUIs();
    }

    void CalculateCornersAttractivness()
    {
        for (int i = 0; i < GetCurrentPlayer().myFrequencyFactor.Length; i++)
        {
            float sum = GetCurrentPlayer().myResourceRevenues.Sum();
            if (sum != 0) GetCurrentPlayer().myFrequencyFactor[i] = (float)GetCurrentPlayer().myResourceRevenues[i] / sum;
            else GetCurrentPlayer().myFrequencyFactor[i] = 0;
        }
        for (int c = 0; c < boardsCorners.Count; c++)
        {
            Corner corner = boardsCorners[c];

            float generalNumber = 1;
            float individualNumber = 1;
            float resourceImportance = 0;
            for (int tile = 0; tile < corner.adjacentTiles.Count; tile++)
            {
                if (corner.adjacentTiles[tile].myResourceType != ResourceType.none)
                {
                    generalNumber *= (1 - generalFrequencyFactor[(int)corner.adjacentTiles[tile].myResourceType]);
                    individualNumber *= (1 - GetCurrentPlayer().myFrequencyFactor[(int)corner.adjacentTiles[tile].myResourceType]);
                    if (GetCurrentPlayer().isAI) resourceImportance += (RESOURCE_IMPORTANCE * GetCurrentPlayer().myAI.startupResources[(int)corner.adjacentTiles[tile].myResourceType]);
                }
            }

            bool validPort = false; // check for harbors
            for (int i = 0; i < corner.adjacentEdges.Count; i++)
            {
                if (corner.adjacentEdges[i].isHarbor)
                {
                    if (corner.adjacentEdges[i].harborType != ResourceType.three && GetCurrentPlayer().myResourceCards[(int)corner.adjacentEdges[i].harborType] != 0)
                    {
                        validPort = true;
                    }
                }
            }

            bool canPlace = true; //checks to see if the corner can be attractive
            for (int i = 0; i < corner.adjacentCorners.Count; i++)
            {
                if (corner.adjacentCorners[i].hasCity || corner.adjacentCorners[i].hasSettlement) canPlace = false;
            }
            if (canPlace && !corner.hasCity && !corner.hasSettlement)
            {
                corner.myAttractiveness = corner.myRevenueOfSettlement + individualNumber;
                if (validPort) corner.myAttractiveness += HARBOR_BONUS;
                if (currentState == GameState.setup) corner.myAttractiveness += generalNumber + resourceImportance;
            }
            else corner.myAttractiveness = 0;
        }

        boardsCorners.Sort((x, y) => (y.myAttractiveness.CompareTo(x.myAttractiveness)));
    }

    void CalculateCornerCompetition()
    {
        for (int c = 0; c < boardsCorners.Count; c++)
        {
            if((boardsCorners[c].hasCity || boardsCorners[c].hasSettlement) && boardsCorners[c].currentColor == GetCurrentPlayer().myColor)
            {
                boardsCorners[c].degreeOfCompetition = 0;
                continue;
            }

            int closestPlayer = CornerCompetition(boardsCorners[c], 1, GetCurrentPlayer());
            boardsCorners[c].distanceAway = closestPlayer;
            int closestOther = CornerCompetition(boardsCorners[c], 1);

            if (closestPlayer - 2 >= closestOther) boardsCorners[c].degreeOfCompetition = 2;
            else if (closestPlayer - 1 >= closestOther) boardsCorners[c].degreeOfCompetition = 1;
            else if (closestOther - 2 >= closestPlayer) boardsCorners[c].degreeOfCompetition = -2;
            else if (closestOther - 1 >= closestPlayer) boardsCorners[c].degreeOfCompetition = -1;
            else if (closestPlayer != 0 && closestPlayer == closestOther) boardsCorners[c].degreeOfCompetition = 10;
            else boardsCorners[c].degreeOfCompetition = 0;
        }
    }

    int CornerCompetition(Corner corner, int dist, Player player = null)
    {
        corner.cornerChecked = true;
        for (int i = 0; i < corner.adjacentCorners.Count; i++)
        {
            if (player != null)
            {
                if ((corner.adjacentCorners[i].hasCity || corner.adjacentCorners[i].hasSettlement) && corner.adjacentCorners[i].currentColor == player.myColor)
                {
                    corner.cornerChecked = false;
                    return dist;
                }
            }
            else
            {
                if ((corner.adjacentCorners[i].hasCity || corner.adjacentCorners[i].hasSettlement) && corner.adjacentCorners[i].currentColor != GetCurrentPlayer().myColor)
                {
                    corner.cornerChecked = false;
                    return dist;
                }
            }

            int temp = 0;
            if(dist < 3 && !corner.adjacentCorners[i].cornerChecked) temp = CornerCompetition(corner.adjacentCorners[i], dist + 1, player);

            if (temp != 0)
            {
                corner.cornerChecked = false;
                return temp;
            }
        }
        corner.cornerChecked = false;

        return 0;
    }

    /// <summary>
    /// Ends the game and sets the winner to player ps mat.
    /// </summary>
    /// <param name="p">Player class thats being checked</param>
    void GameOver(Player p)
    {
        currentState = GameState.gameOver;
        winnersMat.material = MasterController.mc.playerColorMaterials.Find((x) => x.name == p.myColor.ToString());
        GameOverUI.SetActive(true);
    }

    /// <summary>
    /// Checks to see if the current player can end their turn.
    /// </summary>
    bool CalculateCanEndTurn()
    {
        if (!playerRolled || robber.needsToMove || GetCurrentPlayer().numberOfFreeRoads != 0 || currentState == GameState.pickingRandomCard || animationPlaying) return false;

        return CalculateTooManyCards();
    }

    public bool CalculateTooManyCards()
    {
        if (currentState == GameState.clearingTooManyCards && TooManyCards() && !movingFromKnight) return false;
        return true;
    }

    public void RollDiceButton(bool force = false)
    {
        if(force == false && GetCurrentPlayer().isAI) return;
        if(isWaitingForRoll) return;

        // Move the game dice to be just above where the player's roll dice button is
		gameDice.transform.position = GetPlayerMat(currentPlayer).transform.Find("RollDiceButton").transform.position + (Vector3.up);

        StartCoroutine(RollDice());
    }

    /// <summary>
    /// Rolls the dice. Does things based on the roll and distributes resource based on the roll
    /// </summary>
    public IEnumerator RollDice()
    {
        if (currentState == GameState.setup) yield break;

		gameDice.transform.LookAt(Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 9f)));
        gameDice.Throw(gameDice.transform.forward, 8f);

        playerRolled = true;
        isWaitingForRoll = true;
        yield return new WaitUntil(() => gameDice.HaveSettled());
        HandleDiceValue(gameDice.GetTotalValue());

        yield return new WaitForSeconds(1.5f);
        isWaitingForRoll = false;
        gameDice.Reset();
    }

    private void HandleDiceValue(int rollValue) {
        if (currentState != GameState.gameplay || !playerRolled) return;

        if (rollValue == 7)
        {
            HandleRobberRoll();
        }
        else
        {

            boardsTiles.ForEach((Tile tile)=>{
                if (tile.name.IndexOf("Tile-" + rollValue) > -1 && !tile.hasRobber){
                    tile.gameObject.AddComponent<BlinkABit>();
                }
            });


            DistributeRollResources(rollValue);
        }
    }

    void HandleRobberRoll()
    {
        robber.needsToMove = true;
        CalculateNeededHand();
    }

    /// <summary>
    /// Utility function to animate cards from one player mat to another.
    /// </summary>
    /// <param name="playerFrom">Player giving resource (1 - 4)</param>
    /// <param name="playerTo">Player receiving resource (1 - 4)</param>
    public void AnimateCardToPlayer(ResourceType type, int playerFrom, int playerTo, int count = 1){
        int fromMat = players[playerFrom].myUIPos + 1;
        int toMat = players[playerTo].myUIPos + 1;

        Transform from = GetPlayerMat(playerFrom).transform.Find("Tab1").transform.Find("Mat").transform;
        Transform to =  GetPlayerMat(playerTo).transform.Find("Tab1").transform.Find("Mat").transform;
        AnimateCard(type, from, to, count);
    }

    public GameObject GetPlayerMat(int playerNum) {
        int pos = players[playerNum].myUIPos + 1;
        return GameObject.Find("PlayerMat" + pos);
    }

    public void AnimateCard(ResourceType type, Transform from, Transform to, int count = 1)
    {
        int i = 0;
        while(i < count) {
            StartCoroutine(StartAnimatingCard(type, from, to, i * 0.1f));
            i += 1;
        }
    }

    public void AnimateDevelopmentCard(CardType type, Transform from) {
        animationPlaying = true;

        GameObject follower = Instantiate(Resources.Load("FlyingObject") as GameObject);
        FlyingObject fo = follower.GetComponent<FlyingObject>();

        GameObject cardPrefab = Resources.Load("Cards/Dev/" + type + "Card") as GameObject;
        Debug.Assert(cardPrefab != null, "No dev card prefab found for '" + ("Cards/Dev/" + type) + "'");

        GameObject go = Instantiate(cardPrefab);
        go.transform.localScale *= 2.9f;
        go.transform.parent = follower.transform;
        go.transform.position = Vector3.zero;

        follower.transform.position = from.position;
        follower.transform.rotation = new Quaternion(0f, (360f * 4f * (Random.value > 0.5 ? -1 : 1)), 0f, 0f);

        GameObject target = new GameObject();
        Vector3 dest = Camera.main.ScreenToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        target.transform.position = new Vector3(dest.x, (Camera.main.transform.position.y / 2f), dest.z);

        fo.scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.05f);
        fo.target = target.transform;
        fo.destroyWhenDone = false;
        fo.duration = 1f;

        fo.AddCallbackOnce(()=>{
            target.transform.position = new Vector3(dest.x, -10f, dest.z);

            fo.AddCallbackOnce(()=>{
                // This fires after the replay.
                animationPlaying = false;
                Destroy(target.gameObject);
            });

            StartCoroutine(WaitThenRemoveCard(fo, 6f));
        });

        fo.Activate();
        Camera.main.gameObject.GetComponent<CameraFocus>().dofAmount = 4.56f;
    }

    IEnumerator WaitThenRemoveCard(FlyingObject cardFo, float delay) {
        yield return new WaitForSeconds(delay);
        cardFo.destroyWhenDone = true;
        cardFo.Rewind();
        Camera.main.gameObject.GetComponent<CameraFocus>().dofAmount = 9.08f;
    }

    private IEnumerator StartAnimatingCard(ResourceType type, Transform from, Transform to, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        GameObject follower = Instantiate(Resources.Load("FlyingObject") as GameObject);
        FlyingObject fo = follower.GetComponent<FlyingObject>();

        GameObject go = Instantiate(Resources.Load("Cards/" + type + "Card") as GameObject);
        go.transform.localScale *= 0.05f;
        go.transform.parent = follower.transform;
        go.transform.position = Vector3.zero;

        follower.transform.position = from.position + new Vector3(Random.value * (Random.value > 0.5 ? -0.25f : 0.25f), 0f, Random.value * (Random.value > 0.5 ? -0.25f : 0.25f));
        follower.transform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(-360f, 360f), 0f));
        fo.target = to;
        fo.Activate();
    }

    void DistributeRollResources(int type)
    {

        int[][] possibleResources = new int[players.Count][];
        for (int i = 0; i < players.Count; i++)
        {
            Player p = players[i];
            possibleResources[i] = new int[5] { 0, 0, 0, 0, 0 };
            for (int corner = 0; corner < p.myCorners.Count; corner++)
            {
                Corner c = p.myCorners[corner];
                for (int tile = 0; tile < c.adjacentTiles.Count; tile++)
                {
                    if (c.adjacentTiles[tile].myValue == type && !c.adjacentTiles[tile].hasRobber && c.adjacentTiles[tile].myResourceType != ResourceType.none)
                    {
                        Transform destination = GameObject.Find("PlayerMat" + (players[i].myUIPos + 1)).transform.Find("Tab1").transform.Find("Mat").transform;

                        if (c.hasSettlement){
                            possibleResources[i][(int)c.adjacentTiles[tile].myResourceType] += 1;
                            AnimateCard(c.adjacentTiles[tile].myResourceType, c.adjacentTiles[tile].transform, destination, 1);
                        } else if (c.hasCity) {
                            possibleResources[i][(int)c.adjacentTiles[tile].myResourceType] += 2;
                            AnimateCard(c.adjacentTiles[tile].myResourceType, c.adjacentTiles[tile].transform, destination, 2);
                        }
                    }
                }
            }
        }

        //for The Last Gameboard
        if (roadsGivenOut < players.Count)
        {
            possibleResources[currentPlayer][0] += 1;
            possibleResources[currentPlayer][1] += 1;
            roadsGivenOut++;
        }

        CalculateBankResources(possibleResources);
    }

    void CalculateBankResources(int[][] a)
    {
        for(int i = 0; i < 5; i++)
        {
            int total = 0;
            for (int player = 0; player < players.Count; player++) total += a[player][i];
            if (Bank.HasEnoughResources((ResourceType)i, total))
            {
                for (int player = 0; player < players.Count; player++)
                {

                    players[player].AddResource((ResourceType)i, a[player][i]);

                    Bank.RemoveFromBank((ResourceType)i, a[player][i]);
                }
            }else //bank does not have enough resources to hand out
            {
                List<Player> playersGettingResource = new List<Player>();
                for (int player = 0; player < players.Count; player++)
                {
                    if (a[player][i] != 0) playersGettingResource.Add(players[player]);
                }

                if(playersGettingResource.Count == 1)
                {
                    playersGettingResource[0].AddResource((ResourceType)i, Bank.GiveAllResources((ResourceType)i));
                }
            }
        }
    }

    #region stuff with Resources and trading

    /// <summary>
    /// Returns the players that the current player can take a random resource from.
    /// </summary>
    /// <param name="t">The tile that the robber was placed on</param>
    public void DisplayStealableResources(Tile t)
    {
        List<PlayerColors> tilesColors = new List<PlayerColors>();
        for (int c = 0; c < t.myCorners.Count; c++) //go through each corner and check the number of colors
        {
            PlayerColors pc = t.myCorners[c].currentColor;
            if (!tilesColors.Contains(pc) && pc != GetCurrentPlayer().myColor && (t.myCorners[c].hasCity || t.myCorners[c].hasSettlement)) tilesColors.Add(pc);
        }

        if (tilesColors.Count == 0)
        {
            //Debug.Log("Number of cards is 0");
            movingFromKnight = false;
            currentState = GameState.gameplay;
            return;
        }

        currentState = GameState.pickingRandomCard;

        randomCardUI.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        randomCardUI.gameObject.SetActive(true);

        int numberOfCards = tilesColors.Count;
        //for (int i = 0; i < numberOfCards; i++) numberOfCards += GetPlayerByColor(tilesColors[i]).GetTotalNumberOfResources();

        float startPos = ((numberOfCards - 1) * 150) * -1;
        for (int i = 0; i < tilesColors.Count; i++)
        {
            Player player = GetPlayerByColor(tilesColors[i]);

            GameObject card = (GameObject)Instantiate(Resources.Load("RandomResource"));
            card.transform.SetParent(randomCardUI.transform);
            card.transform.localPosition = new Vector3(startPos, 90f, 0);
            card.transform.localScale = new Vector3(.5f,.5f,.5f);
            TextMeshProUGUI label = card.transform.Find("Number").GetComponent<TextMeshProUGUI>();
            Debug.Assert(label != null, "No label found for player " + player.myColor);
            label.text = GetPlayerByColor(tilesColors[i]).GetTotalNumberOfResources().ToString();

            Image[] images = card.GetComponentsInChildren<Image>();
            for (int j = 0; j < images.Length; j++) images[j].color = MasterController.ToColor(tilesColors[i]);

            card.GetComponent<RandomResourceUI>().SetUp(tilesColors[i], this);

            startPos += 300;
        }

        int uiPos = players[currentPlayer].myUIPos + 1;
        randomCardUI.gameObject.transform.rotation = new Quaternion(0f, 0f, (uiPos == 2 || uiPos == 3) ? 180f : 0f, 0f);
    }

    Player GetPlayerByColor(PlayerColors c)
    {
        for(int i = 0; i < players.Count; i++)
        {
            if (players[i].myColor == c) return players[i];
        }

        Debug.LogError("GetPlayerByColor Color " + c + " Is Not A Player");
        return null;
    }

    /// <summary>
    /// Takes a random resource from a player.
    /// </summary>
    /// <param name="c">The player color that resources were took from</param>
    public void TookResource(PlayerColors c)
    {
        Player p = players.Find((x) => x.myColor == c);
        if (GetPlayerCardCount(p) != 0)
        {
            int rand = Random.Range(0, 5);
            while (p.myResourceCards[rand] == 0)
            {
                rand = Random.Range(0, 5);
            }

            p.RemoveResource((ResourceType)rand, 1);
            GetCurrentPlayer().AddResource((ResourceType)rand, 1);

            AnimateCardToPlayer(ResourceType.none, GetPlayerNumber(p), currentPlayer, 1);
        }

        foreach (Transform t in randomCardUI) {
            if (t.gameObject.name != "Label") {
                Destroy(t.gameObject);
            }
        }
        randomCardUI.gameObject.SetActive(false);
        movingFromKnight = false;
        currentState = GameState.gameplay;
    }

    /// <summary>
    /// The resource that is on the mouse/touch
    /// </summary>
    /// <param name="r">The resource type that is in the mouses hand</param>
    ///<param name="p">The player that the resource is coming from</param>
    public void SetResourceInHand(ResourceType r, int p)
    {
        if (p == -1) return; //bank is -1
        playerGivngResource = p;
        resourceInHand = r;
        someoneIsTrading = true;
    }

    public void EndResourceTrading() {
        someoneIsTrading = false;
    }

    /// <summary>
    /// Trades a resource to a player from a player
    /// </summary>
    /// <param name="playerNum">The player number to trade with</param>
    /// <param name="useAnimation">Animate a card between the players? (Used on mobile)</param>
    public bool TradeResource(int playerNum, bool useAnimation = false)
    {
        if (!players[playerGivngResource].HasResource(resourceInHand,1) || currentState == GameState.setup || !playerRolled || (players[playerNum] != GetCurrentPlayer() && players[playerGivngResource] != GetCurrentPlayer())){
            return false;
        }

        if (useAnimation) {
            AnimateCardToPlayer(resourceInHand, playerGivngResource, playerNum, 1);
        }

        players[playerGivngResource].RemoveResource(resourceInHand, 1);
        players[playerNum].AddResource(resourceInHand, 1);

        UpdatePlayerUIs();

        return true;
    }

    /// <summary>
    /// For when the players trade with the bank.
    /// </summary>
    /// <param name="r">The resource type that the bank should give back</param>
    public bool TradeBank(ResourceType r)
    {
        if (currentState == GameState.clearingTooManyCards)
        {
            ThrowCardAway();
        }else
        {
            if(players[playerGivngResource] == GetCurrentPlayer() && GetCurrentPlayer().HasResource(resourceInHand, GetCurrentPlayer().tradeValues[(int)resourceInHand]) && r != resourceInHand && Bank.HasEnoughResources(r,1))
            {
                GetCurrentPlayer().RemoveResource(resourceInHand, GetCurrentPlayer().tradeValues[(int)resourceInHand]);
                Bank.AddToBank(resourceInHand, GetCurrentPlayer().tradeValues[(int)resourceInHand]);
                GetCurrentPlayer().AddResource(r,1);
                Bank.RemoveFromBank(r,1);
                return true;
            }
        }

        return false;
    }

    public void ThrowCardAway()
    {
        if (GetPlayerCardCount(players[playerGivngResource]) != maxCardCount[playerGivngResource] && players[playerGivngResource].HasResource(resourceInHand, 1))
        {
            players[playerGivngResource].RemoveResource(resourceInHand, 1);
            Bank.AddToBank(resourceInHand, 1);
        }
    }

    #endregion

    #region Development Cards Helper Functions

    public void BuyDevelopmentCardButton()
    {
        if (GetCurrentPlayer().isAI) return;
        BuyDevelopmentCard();
    }

    /// <summary>
    /// Buys a development card
    /// </summary>
    public bool BuyDevelopmentCard()
    {
        if (!playerRolled || currentState != GameState.gameplay) return false;
        if(GetCurrentPlayer().HasResource(ResourceType.Grain, 1) && GetCurrentPlayer().HasResource(ResourceType.Wool, 1) && GetCurrentPlayer().HasResource(ResourceType.Ore, 1))
        {
            GetCurrentPlayer().RemoveResource(ResourceType.Grain,1);
            GetCurrentPlayer().RemoveResource(ResourceType.Wool, 1);
            GetCurrentPlayer().RemoveResource(ResourceType.Ore, 1);

            Bank.AddToBank(ResourceType.Grain, 1);
            Bank.AddToBank(ResourceType.Wool, 1);
            Bank.AddToBank(ResourceType.Ore, 1);

            Card c = developmentCardList[0];
            GetCurrentPlayer().AddDevelopmentCard(c);
            developmentCardList.RemoveAt(0);

            UpdatePlayerUIs();

            c.drawTurn = turnNumber;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Shows the resource clicker UI
    /// </summary>
    /// <param name="takesTwoClicks">If it can take two clicks or one click</param>
    public void ShowResourcePicker(bool takesTwoClicks)
    {
        if (GetCurrentPlayer().isAI)
            CheckMonopoly(takesTwoClicks);
        else
            StartCoroutine(ShowResourcePickerHelper(takesTwoClicks));
    }

    IEnumerator ShowResourcePickerHelper(bool takesTwoClicks)
    {
        yield return new WaitForSeconds(4);
        CheckMonopoly(takesTwoClicks);

        currentState = GameState.playingDevelopmentCard;
        resourcePickerGUI.SetActive(true);
        yield return null;
    }

    void CheckMonopoly(bool takesTwoClicks)
    {
        if (takesTwoClicks)
        {
            monopolyPlayed = false;
            maxResourceClicks = 2;
        }
        else monopolyPlayed = true;
    }

    public void ResourcePicked(ResourceType r)
    {
        if (monopolyPlayed)
        {
            for(int player = 0; player < players.Count; player++)
            {
                Player p = players[player];
                if (p != GetCurrentPlayer())
                {
                    int cardCount = p.NumberOfResource(r);
                    GetCurrentPlayer().AddResource(r, cardCount);
                    p.RemoveResource(r, cardCount);

                    AnimateCardToPlayer(r, GetPlayerNumber(p), currentPlayer, cardCount);
                }
            }
        }else
        {
            GetCurrentPlayer().AddResource(r, 1);
            Bank.RemoveFromBank(r,1);
            maxResourceClicks--;

            AnimateCardToPlayer(r, currentPlayer, currentPlayer, 1);
        }

        if(monopolyPlayed || maxResourceClicks == 0)
        {
            resourcePickerGUI.SetActive(false);
            currentState = GameState.gameplay;
        }
    }

    public void KnightPlayed()
    {
        robber.needsToMove = true;
        movingFromKnight = true;
        GetCurrentPlayer().numberOfKnightsPlayed++;

        if (GetCurrentPlayer().numberOfKnightsPlayed >= 3) CalculateLargestArmy();
    }

    void CalculateLargestArmy()
    {
        Player p = GetCurrentPlayer();
        for(int i = 0; i < players.Count; i++)
        {
            players[i].SetLargestArmy(false);
            if (players[i].numberOfKnightsPlayed > p.numberOfKnightsPlayed) p = players[i];
        }

        p.SetLargestArmy(true);
    }

    public void RoadPlayed()
    {
        if (GetCurrentPlayer().numberOfRoadsLeft < 1) return;

        if (GetCurrentPlayer().numberOfRoadsLeft == 1) GetCurrentPlayer().numberOfFreeRoads = 1;
        else GetCurrentPlayer().numberOfFreeRoads = 2;

        if (GetCurrentPlayer().numberOfRoadsLeft <= 10){
            CalculateLongestRoad();
        }
    }

    public void CalculateLongestRoad()
    {
        Player longestPlayer = null;
        for(int p = 0; p < players.Count; p++)
        {
            players[p].SetLongestRoad(false);
            int roadLength = 0;
            if(players[p].numberOfRoadsLeft <= 10)
            {
                Player player = players[p];
                for(int e = 0; e < player.myEdges.Count; e++)
                {
                    int currentRoad = GetMyNextRoad(player.myEdges[e], player);
                    if (currentRoad < 5) continue;

                    if (currentRoad > roadLength) roadLength = currentRoad;

                    if (longestPlayer == null) longestPlayer = player;
                    else if(currentRoad > longestPlayer.myLongestRoad)
                    {
                        longestPlayer = player;
                    }
                }
            }
            players[p].myLongestRoad = roadLength;
            CalculateVirtualRoad(players[p]);
        }

        if(longestPlayer != null) longestPlayer.SetLongestRoad(true);
    }

    public int GetPlayerNumber(Player player) {
        for(int p = 0; p < players.Count; p++)
        {
            if (players[p] == player) {
                return p;
            }
        }

        return -1;
    }

    public Player GetPlayerWithLongestRoad()
    {
        for(int i = 0; i < players.Count; i++)
        {
            if (players[i].longestRoad) return players[i];
        }

        return null;
    }

    int GetMyNextRoad(Edge e, Player p, Corner cornerComingFrom = null)
    {
        if (e.edgeChecked || e.currentColor != p.myColor || !e.hasRoad) return 0;
        e.edgeChecked = true;
        int length = 0;
        for(int i = 0; i < e.adjacentCorners.Count; i++)
        {
            if (cornerComingFrom != null && cornerComingFrom == e.adjacentCorners[i]) continue;
            if ((e.adjacentCorners[i].hasCity || e.adjacentCorners[i].hasSettlement) && e.currentColor != p.myColor) continue;

            for (int edge = 0; edge < e.adjacentCorners[i].adjacentEdges.Count; edge++)
            {
                int myLength = GetMyNextRoad(e.adjacentCorners[i].adjacentEdges[edge], p, e.adjacentCorners[i]);
                if (myLength > length) length = myLength;
            }
        }
        e.edgeChecked = false;
        return length + 1;
    }

    void CalculateVirtualRoad(Player player)
    {
        if (player.roadLengths.Count == 1)
        {
            player.myVirtualLongestRoad = player.myLongestRoad;
            return;
        }

        if(player.roadLengths[0].Count < 3 || player.roadLengths[1].Count < 3)
        {
            player.myVirtualLongestRoad = 0;
            return;
        }

        List<Edge> edges = new List<Edge>();
        int virtualRoad = 0;
        int roadsToMake = 0;
        for(int i = 0; i < player.roadLengths[0].Count; i++)
        {
            Edge startEdge = player.roadLengths[0][i];
            for(int j = 0; j < player.roadLengths[1].Count; j++)
            {
                int[] roads = GetNextEdge(startEdge, player.roadLengths[1][j], new int[] {0,0});
                if(virtualRoad == 0 && roadsToMake == 0 && roads[1] < 3)
                {
                    virtualRoad = roads[0];
                    roadsToMake = roads[1];
                }else
                {
                    if(roads[1] < 3)
                    {
                        if ((roads[0] - roads[1]) > (virtualRoad - roadsToMake))
                        {
                            virtualRoad = roads[0];
                            roadsToMake = roads[1];
                        }else if(((roads[0] - roads[1]) == (virtualRoad - roadsToMake)) && roads[0] > virtualRoad)
                        {
                            virtualRoad = roads[0];
                            roadsToMake = roads[1];
                        }
                    }
                }
            }
        }
        //calculate the distance between the two parts
        //check if both parts would develop the longest road

        player.myVirtualLongestRoad = virtualRoad;
        player.roadsNeededForVLR = roadsToMake;
    }

    int[] GetNextEdge(Edge startEdge, Edge endEdge, int[] listToAdd)
    {
        if (listToAdd[1] > 3) return listToAdd;
        if (startEdge == endEdge)
        {
            if(!GetCurrentPlayer().roadLengths[0].Contains(startEdge) && (GetCurrentPlayer().roadLengths.Count > 1 && !GetCurrentPlayer().roadLengths[1].Contains(startEdge)))
            {
                listToAdd[1]++;
            }
            listToAdd[0]++;
            return listToAdd;
        }
        bool useFirst = false;

        if((startEdge.adjacentCorners[0].transform.position - endEdge.transform.position).sqrMagnitude == (startEdge.adjacentCorners[1].transform.position - endEdge.transform.position).sqrMagnitude) //they are equal in distance
        {
            if (!startEdge.adjacentCorners[0].IsOccupied() && !startEdge.adjacentCorners[1].IsOccupied())
            {
                if (startEdge.adjacentCorners[0].myAttractiveness > startEdge.adjacentCorners[1].myAttractiveness) //0 is more attractive go this way
                {
                    if (startEdge.adjacentCorners[0].CornerIsCurrentColor() || startEdge.adjacentCorners[0].currentColor == PlayerColors.None)
                    {
                        useFirst = true;
                    }
                    else // 0 is more attractive but it has something in that position so use the less attractive corner
                    {
                        useFirst = false;
                    }
                }
                else //1 is more attractive go this way
                {
                    if (startEdge.adjacentCorners[1].CornerIsCurrentColor() || startEdge.adjacentCorners[1].currentColor == PlayerColors.None)
                    {
                        useFirst = false;
                    }
                    else //1 is more attractive but it has something in that position so use the less attractive corner
                    {
                        useFirst = true;
                    }
                }
            }
            else useFirst = startEdge.adjacentCorners[0].IsOccupied();
        }
        else if ((startEdge.adjacentCorners[0].transform.position - endEdge.transform.position).sqrMagnitude < (startEdge.adjacentCorners[1].transform.position - endEdge.transform.position).sqrMagnitude) //0 is closer
        {
            if(startEdge.adjacentCorners[0].CornerIsCurrentColor() || startEdge.adjacentCorners[0].currentColor == PlayerColors.None)
            {
                useFirst = true;
            }
            else // 0 is closer but it has something in that position so use the further away corner
            {
                useFirst = false;
            }
        }
        else //1 is closer so we need to use
        {
            if (startEdge.adjacentCorners[1].CornerIsCurrentColor() || startEdge.adjacentCorners[1].currentColor == PlayerColors.None)
            {
                useFirst = false;
            }
            else // 1 is closer but it has something in that position so use the further away corner
            {
                useFirst = false;
            }

        }

        Corner c = null;
        Edge retEdge = null;

        if (useFirst) c = startEdge.adjacentCorners[0];
        else c = startEdge.adjacentCorners[1];

        for (int e = 0; e < c.adjacentEdges.Count; e++)
        {
            if (retEdge == null) retEdge = c.adjacentEdges[e];
            else
            {
                if ((c.adjacentEdges[e].transform.position - endEdge.transform.position).sqrMagnitude == (retEdge.transform.position - endEdge.transform.position).sqrMagnitude)// two edges are the same distance
                {
                    Corner firstEdgeCorner = null;
                    Corner secondEdgeCorner = null;
                    for(int i = 0; i < c.adjacentEdges[e].adjacentCorners.Count; i++)
                    {
                        if (c.adjacentEdges[e].adjacentCorners[i] != c) firstEdgeCorner = c.adjacentEdges[e].adjacentCorners[i];
                        if (retEdge.adjacentCorners[i] != c) secondEdgeCorner = retEdge.adjacentCorners[i];
                    }

                    if (!firstEdgeCorner.IsOccupied() && !secondEdgeCorner.IsOccupied())
                    {
                        if (firstEdgeCorner.myAttractiveness > secondEdgeCorner.myAttractiveness) retEdge = c.adjacentEdges[e];
                    }
                    else if (firstEdgeCorner.IsOccupied()) retEdge = c.adjacentEdges[e];
                }
                else if ((c.adjacentEdges[e].transform.position - endEdge.transform.position).sqrMagnitude < (retEdge.transform.position - endEdge.transform.position).sqrMagnitude)
                {
                    retEdge = c.adjacentEdges[e];
                }
            }
        }

        if (!GetCurrentPlayer().roadLengths[0].Contains(startEdge) && (GetCurrentPlayer().roadLengths.Count > 1 && !GetCurrentPlayer().roadLengths[1].Contains(startEdge)))
        {
            listToAdd[1]++;
        }
        listToAdd[0]++;
        return GetNextEdge(retEdge, endEdge, listToAdd);
    }

    #endregion

    #region Over7Cards

    public bool TooManyCards()
    {
        if (movingFromKnight) return false;

        bool ret = false;
        for(int i = 0; i < players.Count; i++)
        {
            if (GetPlayerCardCount(players[i]) > maxCardCount[i])
            {
                // TutorialController.ShowText(6,players[i]);
                //Debug.Log("player " + i + " asdf " + maxCardCount[i] + " - " + GetPlayerCardCount(players[i]) );
                ret = true;
            }
        }

        if (!ret) TutorialController.ShowText(7,GetCurrentPlayer());
        return ret;
    }

    void CalculateNeededHand()
    {
        maxCardCount.Clear();
        currentState = GameState.clearingTooManyCards;
        for (int p = 0; p < players.Count; p++)
        {
            int cardNum = GetPlayerCardCount(players[p]);
            if (cardNum > 7)
            {
                maxCardCount.Add(Mathf.FloorToInt(cardNum / 2));
                if (players[p].isAI) ai.TooManyResources(players[p], maxCardCount[p]);
            }
            else
            {
                maxCardCount.Add(7);
            }
        }
    }

    public void TogglePossibleConstructs(Player p, bool shouldDisplay) {
        TogglePossibleRoads(p, shouldDisplay);
        TogglePossibleSettlements(p, shouldDisplay);
        TogglePossibleCities(p, shouldDisplay);
    }

    public void TogglePossibleCities(Player p, bool shouldDisplay) {
        Material glow = new Material(alphaGlowMat);
        // Color col = new Color(1f, 1f, 1f, 0.445f);
        Color col = MasterController.ToColor(p.myColor);
        col.a = 0.8f;

        glow.SetColor("_GlowColor", col);
        boardsCorners.ForEach((Corner corner)=>{
            bool cornerShouldHighlight = currentState == GameState.setup ? false : corner.hasSettlement && corner.IsOwnedByPlayer(p);
            if (shouldDisplay && cornerShouldHighlight) {
                corner.GetComponent<MeshRenderer>().material = glow;
            } else {
               corner.GetComponent<MeshRenderer>().material = corner.defaultMat;
            }
        });
    }

    public void TogglePossibleSettlements(Player p, bool shouldDisplay) {
        Material glow = new Material(alphaGlowMat);
        Color col = new Color(1f, 1f, 1f, 0.445f);
        glow.SetColor("_GlowColor", col);
        boardsCorners.ForEach((Corner corner)=>{
            bool cornerShouldHighlight = currentState == GameState.setup ? (!corner.HasConstruct() && !corner.HasNeighboringConstruct()) : corner.CanBeBuiltOn(p);
            if (shouldDisplay && cornerShouldHighlight) {
                corner.GetComponent<MeshRenderer>().material = glow;
            } else {
               corner.GetComponent<MeshRenderer>().material = corner.defaultMat;
            }
        });
    }

    public void TogglePossibleRoads(Player p, bool shouldDisplay) {
        Material glow = new Material(alphaGlowMat);
        Color col = new Color(1f, 1f, 1f, 0.445f);
        glow.SetColor("_GlowColor", col);

        List<Corner> availableCorners = boardsCorners;

        // During setup, the player can only attach roads to the most recently placed settlement.
        bool useSetupConditions = GameController.gc.currentState == GameState.setup && p.myCorners.Count > 0;
        if (useSetupConditions) {
            availableCorners = new List<Corner>() { p.myCorners[p.myCorners.Count - 1] };
        }

        availableCorners.ForEach((Corner corner)=>{
            corner.adjacentEdges.ForEach((Edge edge)=>{
                bool roadShouldHighlight = edge.adjacentCorners.Find((Corner c)=>{
                    return c.HasLeadingRoads(p) || (c.HasConstruct() && c.IsOwnedByPlayer(p));
                }) != null;

                if (shouldDisplay && (roadShouldHighlight && !edge.hasRoad)){
                    edge.GetComponent<MeshRenderer>().sharedMaterial = glow;
                } else {
                    edge.GetComponent<MeshRenderer>().sharedMaterial = edge.defaultMat;
                }
            });
        });
    }

    public int GetPlayerDiscardAmount(Player p)
    {
        int playerNum = GetPlayerNumber(p);
        int count = GetPlayerCardCount(players[playerNum]);
        return count - maxCardCount[playerNum];
    }

    public int GetPlayerCardCount(Player p)
    {
        int cardNum = 0;
        for (int c = 0; c < p.myResourceCards.Length; c++)
        {
            cardNum += p.myResourceCards[c];
        }
        return cardNum;
    }
    #endregion

    #region EmojiStuff

    public void EmojiButtonClicked()
    {
        emojiBar.SetActive(!emojiBar.activeSelf);
    }

    #endregion

}
