using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
	//Values
	//[SerializeField]
	Vector2Int boardSize;

    [SerializeField]
    GameObject gameBoardGround = default;

    [SerializeField]
	GameBoard board = default;

	[SerializeField]
	GameTileContentFactory tileContentFactory = default;

    [SerializeField]
    WarFactory warFactory = default;

	Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    //Collections, one for ennemies, one for non ennemies
    GameBehaviorCollection enemies = new GameBehaviorCollection();
    GameBehaviorCollection nonEnemies = new GameBehaviorCollection();

    TowerType selectedTowerType;


    //configuration field for a scenario and keep track of the its state
    [SerializeField]
    GameScenario scenario = default;

    GameScenario.State activeScenario;

    //How many enemies need to succeed to trigger defeat depends on the starting health of the player
    [SerializeField, Range(0, 100)]
    int startingPlayerHealth = 10;
    //Current health
    int playerHealth;

    //We have to keep track of game's oown instance for nonEnemies
    static Game instance;

    //Time values
    const float pausedTimeScale = 0f;

    [SerializeField, Range(1f, 10f)]
    float playSpeed = 1f;

    //Button selection
    ButtonPushType buttonTypeTile = ButtonPushType.Empty;

    //Values for swapping allies on the grid
    GameTile swapTile1 = null;
    GameTile swapTile2 = null;


    //---------------------------------------------------------------
    //Functions
    void Awake()
	{
        playerHealth = startingPlayerHealth;
        boardSize = new Vector2Int((int)gameBoardGround.transform.localScale.x, (int)gameBoardGround.transform.localScale.y);
        board.Initialize(boardSize, tileContentFactory);
		board.ShowGrid = true;
        activeScenario = scenario.Begin();
    }

    void OnValidate()
	{
		if (boardSize.x < 2)
		{
			boardSize.x = 2;
		}
		if (boardSize.y < 2)
		{
			boardSize.y = 2;
		}
	}

    //The spawn of the enemy doesn't belong anymore to update, it belong now to the scenario
	void Update()
	{
        //Checks whether  button was pressed per update
        //This button also correspond to touch screen
        if (Input.GetMouseButtonDown(0))
		{
			HandleTouch();
		}

        //Check if the player have lost
        if (playerHealth <= 0 && startingPlayerHealth > 0) //we check if the sarting player health is > 0 for debug reason, we will set starting life at 0 to try the game
        {
            Debug.Log("Defeat!");
            BeginNewGame();
        }

        //Check if player have win ( no more enemies to spawn and on the board)
        if (!activeScenario.Progress() && enemies.IsEmpty)
        {
            Debug.Log("Victory!");
            BeginNewGame();
            activeScenario.Progress();
        }

        //Update
        enemies.GameUpdate();
        Physics.SyncTransforms();
		board.GameUpdate();
        nonEnemies.GameUpdate();
	}

    void HandleTouch()
	{
		GameTile tile = board.GetTile(TouchRay);
        
        if(tile != null)
        {
            switch (buttonTypeTile)
            {
                case ButtonPushType.Wall:
                    board.ToggleWall(tile);
                    break;

                case ButtonPushType.LaserTower:
                    board.ToggleTower(tile, selectedTowerType);
                    break;

                case ButtonPushType.MortarTower:
                    board.ToggleTower(tile, selectedTowerType);
                    break;

                case ButtonPushType.Destination:
                    board.ToggleDestination(tile);
                    break;

                case ButtonPushType.SpawnPoint:
                    board.ToggleSpawnPoint(tile);
                    break;

                case ButtonPushType.Sand:
                    board.ToggleSand(tile);
                    break;

                case ButtonPushType.SwapAllies:
                    SwapAllies(tile);
                    break;
            }
        }
	}

    //Enemy functions
    //Make an enemy spawn at a random position
    public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
	{
		GameTile spawnPoint =
			instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
		Enemy enemy = factory.Get(type);
		enemy.SpawnOn(spawnPoint);
		instance.enemies.Add(enemy);
	}

    //Enemies can inform Game that they reached their destination
    public static void EnemyReachedDestination()
    {
        instance.playerHealth -= 1;
    }

    //Shell functions
    public static Shell SpawnShell()
    {
        Shell shell = instance.warFactory.Shell;
        instance.nonEnemies.Add(shell);
        return shell;
    }

    void OnEnable()
    {
        instance = this;
    }

    //Explosions function
    public static Explosion SpawnExplosion()
    {
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);
        return explosion;
    }

    //Clears the enemies, non-enemies, and board, and then begins a new scenario
    void BeginNewGame()
    {
        playerHealth = startingPlayerHealth;
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
        activeScenario = scenario.Begin();
    }

    //Button function
    public void GetButtonPush(string buttonName) 
    {
        foreach(ButtonPushType buttonType in System.Enum.GetValues(typeof(ButtonPushType)))
        {
            if(buttonType.ToString() == buttonName)
            {
                switch (buttonType)
                {
                    //Show info on grid
                    case ButtonPushType.ShowGrid:
                        board.ShowGrid = !board.ShowGrid;
                        ResetSwapAlies();
                        break;

                    case ButtonPushType.ShowArrows:
                        board.ShowPaths = !board.ShowPaths;
                        ResetSwapAlies();
                        break;

                    //Change the Turret type
                    case ButtonPushType.LaserTower:
                        selectedTowerType = TowerType.Laser;
                        buttonTypeTile = buttonType;
                        ResetSwapAlies();
                        break;

                    case ButtonPushType.MortarTower:
                        selectedTowerType = TowerType.Mortar;
                        buttonTypeTile = buttonType;
                        ResetSwapAlies();
                        break;


                    //Pause and restart the game
                    case ButtonPushType.Pause:
                        Time.timeScale =
                            Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
                        ResetSwapAlies();
                        break;

                    case ButtonPushType.Restart:
                        BeginNewGame();
                        ResetSwapAlies();
                        break;

                    //Switch case
                    case ButtonPushType.SwapAllies:
                        buttonTypeTile = buttonType;
                        break;
                                               
                    default:
                        ResetSwapAlies();
                        buttonTypeTile = buttonType;
                        break;
                }
            }
        }
    }

    void ResetSwapAlies()
    { 
        if(swapTile1 != null)
        {
            swapTile1.isSwapping = false;
            swapTile1 = null;
        }

        if(swapTile2 != null)
        {
            swapTile2.isSwapping = false;
            swapTile2 = null;
        }
    }

    void SwapAllies(GameTile tile)
    {
        //The first tile must be a turret
        if (swapTile1 == null)
        {
            if(tile.Content.Type == GameTileContentType.Tower)
            {
                swapTile1 = tile;
                Debug.Log("Tower selected");
                Debug.Log("Swap tile 1 content : " + swapTile1.Content);
                tile.isSwapping = true;
            }
        }

        //If we already have it, we check for the second tile, it must be empty
        else if (swapTile2 == null)
        {
            if (tile.Content.Type == GameTileContentType.Empty)
            {
                //We get the new stile
                swapTile2 = tile;
                tile.isSwapping = true;

                //We swap the content
                GameTileContent swapTileHold = swapTile1.Content;
                swapTile1.Content = swapTile2.Content;
                swapTile2.Content = swapTileHold;

                //Check if the new configuration will not block the ground
                if (!board.FindPaths())
                {
                    swapTile2.Content = swapTile1.Content;
                    swapTile1.Content = swapTileHold;
                    board.FindPaths();
                    Debug.Log("Failed to swap !");
                }
                else
                {
                    Debug.Log("Success to swap ! ");
                }

                ResetSwapAlies();
            }

        }
    }
}
