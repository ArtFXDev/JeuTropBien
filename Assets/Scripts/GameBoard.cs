﻿using UnityEngine;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour
{
	//Values
	[SerializeField]
	Transform ground = default;

	[SerializeField]
	GameTile tilePrefab = default;

	//Size of field
	Vector2Int size;

	//Array of tiles
	GameTile[] tiles;

	//End tiles
	Queue<GameTile> searchFrontier = new Queue<GameTile>();
	
	//Reference to the factory
	GameTileContentFactory contentFactory;

	//Show arrows
	bool showGrid,showPaths;

	//Grid visualisation
	[SerializeField]
	Texture2D gridTexture = default;

	//Spawn point for ennemies
	List<GameTile> spawnPoints = new List<GameTile>();

	//List of everythings thats need to be updated
	List<GameTileContent> updatingContent = new List<GameTileContent>();

	//---------------------------------------------------------
	//Functions
	public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
	{
		//Board
		this.size = size;
		this.contentFactory = contentFactory;
		ground.localScale = new Vector3(size.x, size.y, 1f);

		//Tiles
		Vector2 offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
		tiles = new GameTile[size.x * size.y];
		for (int i = 0, y = 0; y < size.y; y++){
			for (int x = 0; x < size.x; x++, i++){
				GameTile tile = tiles[i] = Instantiate(tilePrefab);
				tile.transform.SetParent(transform, false);
				tile.transform.localPosition = new Vector3(	x - offset.x, 0f, y - offset.y);

				if (x > 0){
					GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
				}
				if (y > 0){
					GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
				}

				//Set the alternative part of the tile (for having diagonal arrows)
				tile.IsAlternative = (x & 1) == 0;
				if ((y & 1) == 0)
				{
					tile.IsAlternative = !tile.IsAlternative;
				}
			}
		}


        Clear();
        
	}

	//Set tile[0] as destination for the moment
	bool FindPaths()
	{
		//first step is to clear the path of all tiles, then make one tile the destination and add it to the frontier
		foreach (GameTile tile in tiles)
		{
			if (tile.Content.Type == GameTileContentType.Destination)
			{
				tile.BecomeDestination();
				searchFrontier.Enqueue(tile);
			}
			else
			{
				tile.ClearPath();
			}
		}

		//Check if ther's at least one destination cell
		if (searchFrontier.Count == 0)
		{
			return false;
		}

		//Next step is to take the single tile out of the frontier and grow the path to its neighbors
		while (searchFrontier.Count > 0)
		{
			GameTile tile = searchFrontier.Dequeue();
			if (tile != null)
			{
				if (tile.IsAlternative)
				{
					searchFrontier.Enqueue(tile.GrowPathNorth());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathWest());
				}
				else
				{
					searchFrontier.Enqueue(tile.GrowPathWest());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathNorth());
				}
				
			}
		}

		//Check that all tiles have a path
		foreach (GameTile tile in tiles)
		{
			if (!tile.HasPath)
			{
				return false;
			}
		}

		//Put the correct rotation for all the ties if disered
		if (showPaths)
		{
			foreach (GameTile tile in tiles)
			{
				tile.ShowPath();
			}
		}
		return true;
	}

	//Will return wich tile was hit by the ray (null if none)
	public GameTile GetTile(Ray ray)
	{
		if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1)) {
			int x = (int)(hit.point.x + size.x * 0.5f);
			int y = (int)(hit.point.z + size.y * 0.5f);
			if (x >= 0 && x < size.x && y >= 0 && y < size.y)
			{
				return tiles[x + y * size.x];
			}
		}
		return null;
	}

	//Clicking an empty tiles makes it a destination, while clicking a destination removes it
	public void ToggleDestination(GameTile tile)
	{
		if (tile.Content.Type == GameTileContentType.Destination)
		{
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
			//Check if ther's at least one path
			if (!FindPaths())
			{
				tile.Content =
					contentFactory.Get(GameTileContentType.Destination);
				FindPaths();
			}
		}
		else if (tile.Content.Type == GameTileContentType.Empty)
		{
			tile.Content = contentFactory.Get(GameTileContentType.Destination);
			FindPaths();
		}
	}

	//Method for a wall
	public void ToggleWall(GameTile tile)
	{
		if (tile.Content.Type == GameTileContentType.Wall)
		{
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
			FindPaths();
		}
		else if (tile.Content.Type == GameTileContentType.Empty)
		{
			tile.Content = contentFactory.Get(GameTileContentType.Wall);
			if (!FindPaths())
			{
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FindPaths();
			}
		}
	}

	//Toggle a spawn point to GameBoard,
	public void ToggleSpawnPoint(GameTile tile)
	{
		if (tile.Content.Type == GameTileContentType.SpawnPoint)
		{
			if (spawnPoints.Count > 1)
			{
				spawnPoints.Remove(tile);
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
			}
		}
		else if (tile.Content.Type == GameTileContentType.Empty)
		{
			tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
			spawnPoints.Add(tile);
		}
	}

	//Toggle a tower
	public void ToggleTower(GameTile tile, TowerType towerType)
	{
		if (tile.Content.Type == GameTileContentType.Tower)
		{
			updatingContent.Remove(tile.Content);
            if(((Tower)tile.Content).TowerType == towerType)
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
            else
            {
                tile.Content = contentFactory.Get(towerType);
                updatingContent.Add(tile.Content);
            }
			
		}
		else if (tile.Content.Type == GameTileContentType.Empty)
		{
			tile.Content = contentFactory.Get(towerType);
			if (FindPaths()){
				updatingContent.Add(tile.Content);
			}
			else
			{
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FindPaths();
			}
		}
		else if (tile.Content.Type == GameTileContentType.Wall)
		{
			tile.Content = contentFactory.Get(towerType);
			updatingContent.Add(tile.Content);
		}
	}

    //Toggle a sand tile
    public void ToggleSand(GameTile tile)
    {
        //No need to rebuild the path, we don't change it
        if (tile.Content.Type == GameTileContentType.Sand)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Sand);
            FindPaths();
        }
    }

	//Getter and setter of showpath
	public bool ShowPaths
	{
		get => showPaths;
		set
		{
			showPaths = value;
			if (showPaths)
			{
				foreach (GameTile tile in tiles)
				{
					tile.ShowPath();
				}
			}
			else
			{
				foreach (GameTile tile in tiles)
				{
					tile.HidePath();
				}
			}
		}
	}

	//Getter and setter of show grid
	public bool ShowGrid
	{
		get => showGrid;
		set
		{
			showGrid = value;
			Material m = ground.GetComponent<MeshRenderer>().material;
			if (showGrid)
			{
				m.mainTexture = gridTexture;
				m.SetTextureScale("_MainTex", size);
			}
			else
			{
				m.mainTexture = null;
			}
		}
	}

	//Get spawn points
	public GameTile GetSpawnPoint(int index)
	{
		return spawnPoints[index];
	}

	//Amount of spawn points
	public int SpawnPointCount => spawnPoints.Count;

	public void GameUpdate()
	{
		for(int i = 0; i<updatingContent.Count; i++)
		{
			updatingContent[i].GameUpdate();
		}
	}

    //Empties all tiles, clears the spawn points and updating content, and sets the default destination and spawn point
    public void Clear()
    {
        foreach (GameTile tile in tiles)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
        }
        spawnPoints.Clear();
        updatingContent.Clear();
        //Set the center as destination
        ToggleDestination(tiles[tiles.Length / 2]);
        //Set the first tile as spawn ennemies point
        ToggleSpawnPoint(tiles[0]);
    }
}