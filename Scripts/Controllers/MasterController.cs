using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using PlayTable.Unity;
using TMPro;

public enum PlayerColors
{
    Blue,
    Red,
    Orange,
    White,
    Brown,
    Green,
    None
}

public enum ResourceType
{
    Brick,
    Lumber,
    Wool,
    Grain,
    Ore,
    three,
    none
}

public class MasterController : NetworkBehaviour {

    public static MasterController mc;

    public GameController gc;
    public Robber robber;
    public GameObject MainMenuGUI;
    public GameObject GameGUI;

    public float radius; //size of the tiles;

    public GameObject water;
    public Port[] ports;

    public Material[] tileTypes;
    public Material[] portTypes;
    public List<Material> playerColorMaterials;
    public List<Material> playerColorMaterialsUI;

    static int[] tileValueList = new int[] { 5, 2, 6, 3, 8, 10, 9, 12, 11, 4, 8, 10, 9, 4, 5, 6, 3, 11 };
    static int[] numberOfTypes;
    static int[] numberOfRecourcePorts;

    int tileValuePointer;

    IEnumerator waitCo;

    private GameObject corners;
    private GameObject edges;

    //stuff to hold to set corners and edges
    private Corner firstCorner;
    private Corner currentCorner;
    private Corner lastCorner;
    private Edge currentEdge;
    private Edge lastEdge;

    //stuff for development cards
    static int[] numberOfDevelopmentCards;

    // Gets players hooked up to mobile
    public int playerCount;
    public int aiCount;

    //stuff for AI
    int[] frequencyFactor;
    List<Corner> boardCorners = new List<Corner>();
    List<Edge> boardEdges = new List<Edge>();
    List<Tile> boardTiles = new List<Tile>();
    public List<AIArchetype> allAIs = new List<AIArchetype>();

    public List<MobilePlayerController> playerControllers;
    public GetAiColors getAIColors;

    List<Player> players = new List<Player>();

    GameObject statsUi;

    public bool inGame;


    void Awake () {
        mc = this;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        statsUi = GameObject.Find("Stats UI");
        Debug.Log("master controller exists");
	}

    void Start()
    {
        playerControllers = new List<MobilePlayerController>(FindObjectsOfType<MobilePlayerController>());
        /*for (int i = 0; i < playerControllers.Count; i++)
        {
            if (playerControllers[i].playerColor == PlayerColors.None)
            {
                Debug.Log("Found something thats blank removing");
                playerControllers.Remove(playerControllers[i]);
            }
        }
        playerControllers.Sort((x, y) => x.myPosInt.CompareTo(y.myPosInt));*/



        if (PTGameManager.singleton.IS_TABLETOP)
        {
            GameObject g = GameObject.Find("AiColors");
            if (g != null)
            {
                getAIColors = g.GetComponent<GetAiColors>();
                aiCount = getAIColors == null ? 0 : getAIColors.aiColors.Count;
                for (int i = 0; i < 4; i++) if (getAIColors.playerColors[i] != -1) playerCount++;

                if (aiCount == 0) Destroy(g);
            }
        }

        // StartCoroutine(WaitAndSetupBoard());
        SetupBoard();
    }

    IEnumerator WaitAndSetupBoard() {
        yield return new WaitForSeconds(2.5f);
        SetupBoard();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }



    /// <summary>
    /// Press end from the menu and game
    /// </summary>
    public void PressedEnd()
    {
        if (waitCo == null) {
            waitCo = ResetQuitButton();
            GameObject.Find("QuitGame").GetComponentInChildren<Text>().text = "Are you sure?";
            StartCoroutine(waitCo);
        } else {
            StopCoroutine(waitCo);

            gc.gameStarted = false;
            RemoveEverythingAndDisconnect();

            SceneManager.LoadScene("LobbyTest");
        }

    }

    IEnumerator ResetQuitButton() {
        yield return new WaitForSeconds(5f);
        GameObject.Find("QuitGame").GetComponentInChildren<Text>().text = "Quit Game";
        waitCo = null;
    }


    void RemoveEverythingAndDisconnect() {
        NetworkServer.Reset();
        //MasterServer.UnregisterHost();

        // Using `SetParent` allows us to destroy DDOL'd GameObjects
        GameObject go = new GameObject();
        foreach (GameObject o in Object.FindObjectsOfType<GameObject>()) {
            if (o != go) {
                o.transform.SetParent(go.transform);
                Destroy(o.gameObject);
            }
        }
        Destroy(go);
    }


    #region BoardSetup
    /// <summary>
    /// Sets up the board based on radius.
    /// </summary>
    void SetupBoard()
    {
        Debug.Log("setting up board");
        tileValuePointer = 0;
        boardCorners.Clear();
        boardTiles.Clear();

        GameObject board = new GameObject("GameBoard");

        corners = new GameObject("Corners");
        corners.transform.SetParent(board.transform);

        edges = new GameObject("Edges");
        edges.transform.SetParent(board.transform);

        water.transform.localScale *= radius;

        frequencyFactor = new int[] {0,0,0,0,0};

        if ((playerCount + aiCount) < 5) //this sets up the small board
        {
            numberOfTypes = new int[] { 3, 4, 4, 4, 3, 1 };
            numberOfRecourcePorts = new int[] {1,1,1,1,1,4};
            numberOfDevelopmentCards = new int[] { 14, 2, 2, 2, 5 };


            float distance = radius * Mathf.Sqrt(3);
            float startRot = 60 * Random.Range(1, 7);

            GameObject go;

            for(int i = 0; i < 12; i++) //sets up the outside tiles
            {
                go = (GameObject)Instantiate(Resources.Load("Tile"));
                go.GetComponent<Tile>().index = i;

                if (i%2 == 0)
                {
                    go.transform.position = new Vector3(Mathf.Cos(startRot * Mathf.Deg2Rad)*(2*distance), 0, Mathf.Sin(startRot * Mathf.Deg2Rad) * (2 * distance));
                }else
                {
                    go.transform.position = new Vector3(Mathf.Cos(startRot * Mathf.Deg2Rad) * (3*radius), 0, Mathf.Sin(startRot * Mathf.Deg2Rad) * (3*radius));
                }
                SetTileProperties(go, board);
                startRot += 30;
            }

            for(int i = 0; i < 6; i++) //sets up the inside tile
            {
                go = (GameObject)Instantiate(Resources.Load("Tile"));

                go.GetComponent<Tile>().index = 12 + i;
                go.transform.position = new Vector3(Mathf.Cos(startRot * Mathf.Deg2Rad) * (distance), 0, Mathf.Sin(startRot * Mathf.Deg2Rad) * (distance));
                SetTileProperties(go, board);
                startRot += 60;
            }

            go = (GameObject)Instantiate(Resources.Load("Tile")); //sets up the very middle tile
            go.GetComponent<Tile>().index = boardTiles.Count;

            SetTileProperties(go, board);
        }

        SetupTileDropPattern();

        SetUpPorts();
        PlayerSetup();

        statsUi.SetActive(false);
        StartCoroutine(StartGame());
    }

    public void SetupTileDropPattern() {
        int rand = Random.Range(0, 2);
        int counter;

        switch(rand){
            default:
            case 0:
                // Fall in order
                break;

            case 1:
                // Fall in pairs (but not necessarily together)
                counter = 0;
                boardTiles.ForEach((Tile tile)=>{
                    tile.index = counter % 2 == 0 ? boardTiles.Count - counter : counter;
                    counter += 1;
                });
                break;

            case 2:
                // Fall randomly
                int[] queue = new int[boardTiles.Count];
                for ( int i = 0; i < queue.Length;i++ ) {
                    queue[i] = i;
                }
                int p = queue.Length - 1;
                for (int n = p - 1; n >= 0; n--)
                {
                    int r = Random.Range(0, n);
                    int t = queue[r];
                    queue[r] = queue[n];
                    queue[n] = t;
                }

                counter = 0;
                boardTiles.ForEach((Tile tile)=>{
                    tile.index = queue[counter];
                    counter += 1;
                });
                break;
        }
    }

    private void EnableTiles() {
        boardTiles.ForEach((Tile tile)=>{
            tile.EnablePhysics();
        });
    }

    IEnumerator StartGame() {
        EnableTiles();
        // #todo this is just a magic number, it should rely on waiting for the tiles to settle
        yield return new WaitForSeconds(6f);
        statsUi.SetActive(true);

        gc.StartGame(players, robber, SetupCards(), boardCorners, boardTiles ,frequencyFactor);
        inGame = true;
    }

    /// <summary>
    /// Returns a random tile type for the tiles
    /// </summary>
    TileTypes GetRandomTileType()
    {
        int rand = Random.Range(0,6);
        while(numberOfTypes[rand] == 0)
        {
            rand = Random.Range(0, 6);
        }

        numberOfTypes[rand]--;
        return (TileTypes)rand;
    }

    /// <summary>
    /// Sets up everything within the tile. This includes the corners and the edges within the tile.
    /// </summary>
    /// <param name="tile">The tile that you are adding stuff to</param>
    /// <param name="board">the board gameobject. Just to set the tile as a child</param>
    void SetTileProperties(GameObject tile, GameObject board)
    {
        tile.transform.SetParent(board.transform);
        TileTypes type = GetRandomTileType();
        int tileValue = 0;
        if (type != TileTypes.Desert)
        {
            tileValue = tileValueList[tileValuePointer];
            tileValuePointer++;
        }
        if (type == TileTypes.Desert)
        {
            robber.SetCurrentTile(tile);
        }

        tile.GetComponent<MeshRenderer>().sharedMaterial = tileTypes[(int)type];
        tile.transform.localScale = new Vector3(radius,radius,radius);
        TextMeshPro[] tileText = tile.GetComponentsInChildren<TextMeshPro>();
        for (int i = 0; i < tileText.Length; i++)
        {
            if(tileText[i].name == "Number") tileText[i].text = tileValue.ToString();
            else
            {
                int numberOfDots = Mathf.Min(tileValue - 1, 13 - tileValue);
                if (numberOfDots < 0) numberOfDots = 0;
                string d = "";
                for (int n = 0; n < numberOfDots; n++) d += ".";
                tileText[i].text = d;
                tile.GetComponent<Tile>().SetTile(radius, type, tileValue, numberOfDots);
                if(type != TileTypes.Desert) frequencyFactor[(int)type] += numberOfDots;
            }
            if (tileValue == 6 || tileValue == 8) tileText[i].color = Color.red;
        }
        tile.name = "Tile-" + tileValue;
        boardTiles.Add(tile.GetComponent<Tile>());
        GameObject c;
        GameObject e;
        float distance = (radius * Mathf.Sqrt(3) / 2);
        float startRot = 30;

        for (int i = 0; i < 12; i++)
        {
            if (i % 2 == 1)
            {
                Vector3 pos = new Vector3(tile.transform.position.x + Mathf.Cos(startRot * Mathf.Deg2Rad) * (distance), 0.025f, tile.transform.position.z + Mathf.Sin(startRot * Mathf.Deg2Rad) * (distance));
                string name = "Edge " + pos.x.ToString("F2") + ":" + pos.z.ToString("F2");

                Transform t = GameObject.Find("Edges").transform.Find(name);
                if (t != null)
                {
                    currentEdge = t.GetComponent<Edge>();
                }else
                {
                    e = (GameObject)Instantiate(Resources.Load("Edge"));
                    e.transform.position = pos;
                    e.name = name;
                    e.transform.SetParent(GameObject.Find("Edges").transform);
                    currentEdge = e.GetComponent<Edge>();
                }

                if(i == 11)
                {
                    currentEdge.SetAdjacentCorners(firstCorner, currentCorner);
                }

                lastEdge = currentEdge;
            }
            else
            {

                Vector3 pos = new Vector3(tile.transform.position.x + Mathf.Cos(startRot * Mathf.Deg2Rad) * (radius), 0.025f, tile.transform.position.z + Mathf.Sin(startRot * Mathf.Deg2Rad) * (radius));
                string name = "Corner " + pos.x.ToString("F2") + ":" + pos.z.ToString("F2");

                Transform t = GameObject.Find("Corners").transform.Find(name);
                if (t != null)
                {
                    currentCorner = t.GetComponent<Corner>();
                    SetAdjacentCorners(tile);
                }
                else {
                    c = (GameObject)Instantiate(Resources.Load("Corner"));
                    c.transform.position = pos;
                    c.transform.SetParent(GameObject.Find("Corners").transform);
                    c.transform.eulerAngles = new Vector3(0,Random.Range(0,360), 0);
                    c.name = name;

                    currentCorner = c.GetComponent<Corner>();
                    currentCorner.SetCorner(gc);
                    boardCorners.Add(currentCorner);
                    SetAdjacentCorners(tile);
                }

                tile.GetComponent<Tile>().myCorners.Add(currentCorner);
                currentCorner.myRevenueOfSettlement += tile.GetComponent<Tile>().myProbabilityOfRevenue;

                if (i == 0) firstCorner = currentCorner;
                if (i == 10)
                {
                    if(!currentCorner.adjacentCorners.Contains(firstCorner)) currentCorner.adjacentCorners.Add(firstCorner);
                    if (!firstCorner.adjacentCorners.Contains(currentCorner)) firstCorner.adjacentCorners.Add(currentCorner);
                }

                lastCorner = currentCorner;
            }
            startRot += 30;
        }
        lastCorner = null;
    }

    /// <summary>
    /// Sets the Adjacent corners to eachother. Also sets the adjacent tile to the corner
    /// </summary>
    /// <param name="tile">The adjacent tile to the current corner being set</param>
    void SetAdjacentCorners(GameObject tile)
    {
        currentCorner.adjacentTiles.Add(tile.GetComponent<Tile>());
        //Debug.Log(currentCorner.adjacentTiles.Count);
        if (lastCorner != null)
        {
            if(!currentCorner.adjacentCorners.Contains(lastCorner)) currentCorner.adjacentCorners.Add(lastCorner);
            if (!lastCorner.adjacentCorners.Contains(currentCorner)) lastCorner.adjacentCorners.Add(currentCorner);

            lastEdge.SetAdjacentCorners(lastCorner,currentCorner);
        }
    }

    /// <summary>
    /// Does all the port set up. ports are all set through the inspector. Sets up the type and the edge that it references.
    /// </summary>
    void SetUpPorts()
    {
        int rand = 0;
        GameObject edges = GameObject.Find("Edges");

        for(int i = 0; i < ports.Length; i++)
        {
            rand = Random.Range(0,6);
            while (numberOfRecourcePorts[rand] == 0)
            {
                rand = Random.Range(0, 6);
            }

            numberOfRecourcePorts[rand]--;

            ports[i].GetComponentInChildren<MeshRenderer>().material = portTypes[rand];
            ports[i].myType = (ResourceType)rand;

            Vector3 v = ports[i].transform.position * radius;

            Transform t = edges.transform.Find("Edge " + v.x.ToString("F2") + ":" + v.z.ToString("F2"));
            if (t != null)
            {
                ports[i].myEdge = t.GetComponent<Edge>();
                t.GetComponent<Edge>().isHarbor = true;
                t.GetComponent<Edge>().harborType = ports[i].myType;
            }
        }
    }
    #endregion

    /// <summary>
    /// Sets up the development cards for the game
    /// </summary>
    List<Card> SetupCards()
    {
        List<Card> cList = new List<Card>();
        for(int i = 0; i < 25; i++)
        {
            int rand = Random.Range(0, 5);

            while (numberOfDevelopmentCards[rand] == 0)
            {
                rand = Random.Range(0, 5);
            }
            numberOfDevelopmentCards[rand]--;
            VPCardType v = VPCardType.none;
            if (rand == 4) v = (VPCardType)numberOfDevelopmentCards[rand];
            Card c = new Card((CardType)rand, v);
            cList.Add(c);
        }

        cList.Sort((x, y) => (Random.Range(-1, 2)));
        return cList;
    }

    /// <summary>
    /// Sets up all the players.
    /// </summary>
    void PlayerSetup()
    {
        SetupAI();
        players = new List<Player>();
        int playersAdded = 0;
        int aiAdded = 0;
        for (int i = 0; i < 4; i++)
        {
            PlayerColors col = PlayerColors.None;
            Player p = null;
            if (getAIColors.playerColors[i] != -1 && playersAdded < playerCount)
            {
                col = (PlayerColors)getAIColors.playerColors[i];
                p = LinkMPC(playersAdded, (int)col);
                p.myUIPos = i;
                playersAdded++;
            }
            else if (aiAdded < aiCount)
            {
                col = (PlayerColors)getAIColors.aiColors[aiAdded];
                p = new Player(col);
                p.myUIPos = i;
                p.isAI = true;
                int r = Random.Range(0, allAIs.Count);
                p.myAI = allAIs[r];
                p.myAI.SetUpAI();
                allAIs.RemoveAt(r);
                aiAdded++;
            }

            if(p != null) players.Add(p);
        }
    }

    void SetupAI()
    {
        allAIs.Clear();
        allAIs.Add(new CandamirAI());
        allAIs.Add(new JeanAI());
        allAIs.Add(new MarianneAI());
        allAIs.Add(new SeanAI());
        allAIs.Add(new VincentAI());
        allAIs.Add(new WilliamAI());
        allAIs.Add(new HildegardAI());
        allAIs.Add(new LouisAI());
    }

    public static Color ToColor(PlayerColors pc)
    {
        switch (pc)
        {
            case (PlayerColors.Blue) : return Color.blue;
            case (PlayerColors.Brown) : return new Color(0.5f, 0.35f, 0f);
            case (PlayerColors.Green) : return Color.green;
            case (PlayerColors.Orange) : return new Color(1f, 0.5f, 0f);
            case (PlayerColors.Red) : return Color.red;
            case (PlayerColors.White) : return Color.white;
            default : return new Color(1f, 1f, 1f, 0.25f);
        }
    }

    Player LinkMPC(int mpcIndex, int playerColor)
    {
        Player newP = new Player((PlayerColors)playerColor);
        if (mpcIndex < playerControllers.Count)
        {
            playerControllers[mpcIndex].player = newP;
            newP.myMobileController = playerControllers[mpcIndex];

            Debug.Log("Sending things to the client");
            RpcController.rpcc.RpcUpdateMyPlayer(playerColor);
        }

        return newP;
    }

}
