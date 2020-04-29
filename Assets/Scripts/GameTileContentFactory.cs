using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
	//Values
	[SerializeField]
	GameTileContent destinationPrefab = default;

	[SerializeField]
	GameTileContent emptyPrefab = default;

	[SerializeField]
	GameTileContent wallPrefab = default;

	[SerializeField]
	GameTileContent spawnPointPrefab = default;

	[SerializeField]
	Tower[] towerPrefabs = default;

    [SerializeField]
    GameTileContent sandPrefab = default;


    //---------------------------------------------------------------
    //Functions
    public void Reclaim(GameTileContent content)
	{
		Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(content.gameObject);
	}

	//Give the factory a private Get method with a prefab parameter
	GameTileContent Get(GameTileContent prefab)
	{
		GameTileContent instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		return instance;
	}

	//Gets an instance of the corresponding prefab.
	public GameTileContent Get(GameTileContentType type)
	{
		switch (type)
		{
			case GameTileContentType.Destination: return Get(destinationPrefab);
			case GameTileContentType.Empty: return Get(emptyPrefab);
			case GameTileContentType.Wall: return Get(wallPrefab);
			case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
			case GameTileContentType.Sand: return Get(sandPrefab);
		}
		Debug.Assert(false, "Unsupported non tower type: " + type);
		return null;
	}

    //Get instance of corresponding tower prefab
    public Tower Get(TowerType type)
    {
        Debug.Assert((int)type < towerPrefabs.Length, "Unsupported tower type!");
        Tower prefab = towerPrefabs[(int)type];
        Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index!");
        return Get(prefab);
    }

    //Generic get method to avoid "public Tower Get(TowerType type)" error with type
    T Get<T>(T prefab) where T : GameTileContent
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }
}