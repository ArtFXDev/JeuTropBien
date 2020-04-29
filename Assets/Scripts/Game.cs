using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
	//Values
	[SerializeField]
	Vector2Int boardSize = new Vector2Int(11, 11);

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


    //---------------------------------------------------------------
    //Functions
    void Awake()
	{
        playerHealth = startingPlayerHealth;
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

        //Wall
        switch (buttonTypeTile)
        {
            case ButtonPushType.Wall:
                Debug.Log("Hanle touch wall");
                board.ToggleWall(tile);
                break;

            case ButtonPushType.LaserTower:
                Debug.Log("Hanle touch Laser Tower");
                board.ToggleTower(tile, selectedTowerType);
                break;

            case ButtonPushType.MortarTower:
                Debug.Log("Hanle touch Mortar Tower");
                board.ToggleTower(tile, selectedTowerType);
                break;

            case ButtonPushType.Destination:
                Debug.Log("Hanle touch Destination");
                board.ToggleDestination(tile);
                break;

            case ButtonPushType.SpawnPoint:
                Debug.Log("Hanle touch Spawn Point");
                board.ToggleSpawnPoint(tile);
                break;

            case ButtonPushType.Sand:
                Debug.Log("Hanle touch Sand");
                board.ToggleSand(tile);
                break;
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
    public void GetButtonPush(string newType) 
    {
        foreach(ButtonPushType buttonType in System.Enum.GetValues(typeof(ButtonPushType)))
        {
            if(buttonType.ToString() == newType)
            {
                switch (buttonType)
                {
                    //Show info on grid
                    case ButtonPushType.ShowGrid:
                        board.ShowGrid = !board.ShowGrid;
                        Debug.Log("GetButtonPush : " + buttonType.ToString());
                        break;

                    case ButtonPushType.ShowArrows:
                        board.ShowPaths = !board.ShowPaths;
                        Debug.Log("GetButtonPush : " + buttonType.ToString());
                        break;

                    //Change the Turret type
                    case ButtonPushType.LaserTower:
                        selectedTowerType = TowerType.Laser;
                        buttonTypeTile = buttonType;
                        Debug.Log("GetButtonPush : " + buttonType.ToString());
                        break;

                    case ButtonPushType.MortarTower:
                        selectedTowerType = TowerType.Mortar;
                        buttonTypeTile = buttonType;
                        Debug.Log("GetButtonPush : " + buttonType.ToString());
                        break;


                    //Pause and restart the game
                    case ButtonPushType.Pause:
                        Time.timeScale =
                            Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
                        Debug.Log("GetButtonPush : " + buttonType.ToString());
                        break;

                    case ButtonPushType.Restart:
                        BeginNewGame();
                        Debug.Log("GetButtonPush : " + buttonType.ToString());
                        break;


                    default:
                        buttonTypeTile = buttonType;
                        Debug.Log("GetButtonPush : " + buttonType.ToString());
                        break;
                }
            }
        }
    }
}
