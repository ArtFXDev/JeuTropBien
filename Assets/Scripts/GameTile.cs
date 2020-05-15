using UnityEngine;

public class GameTile : MonoBehaviour
{
    //Values
    [SerializeField]
    Transform arrow = default;

    //Different direction

    GameTile north, south, east, west, nextOnPath;

    //Amount of tiles that still have to be entered before reaching the destination
    int distance;

    //Rotation of the tiles
    static Quaternion
        northRotation = Quaternion.Euler(90f, 0f, 0f),
        eastRotation = Quaternion.Euler(90f, 90f, 0f),
        southRotation = Quaternion.Euler(90f, 180f, 0f),
        westRotation = Quaternion.Euler(90f, 270f, 0f);

    public bool IsAlternative { get; set; }

    //We will use this to change the content of the tile
    GameTileContent content = default;

    [SerializeField]
    public GameTileContentType contentTypeBegin = default;

	//Next line on the path
	public GameTile NextTileOnPath => nextOnPath;

	public Vector3 ExitPoint { get; private set; }

	//Direction for the orientation of the enemy
	public Direction PathDirection { get; private set; }

    //Boolean saying if tile is in a swap transition or not
    public bool isSwapping = false;

    //Reference to the quad for showing grid
    [SerializeField]
    private GameObject quad;


    //---------------------------------------------------------
    //Functions

    //If a tile is the eastern neighbor of a second tile, then the second tile is the western neighbor of the first tile.
    public static void MakeEastWestNeighbors(GameTile east, GameTile west)
	{
		Debug.Assert(
			west.east == null && east.west == null, "Redefined neighbors!"
		);
		west.east = east;
		east.west = west;
	}

	//Same for North and South
	public static void MakeNorthSouthNeighbors(GameTile north, GameTile south)
	{
		Debug.Assert(
			south.north == null && north.south == null, "Redefined neighbors!"
		);
		south.north = north;
		north.south = south;
	}

	//Before a path is found, there isn't a next tile yet and the distance can be considered infinite.
	public void ClearPath()
	{
		distance = int.MaxValue;
		nextOnPath = null;
	}

	//If used, that a tile has to become the destination. Such a tile has a distance of zero and there is not next tile as the path ends here.
	public void BecomeDestination()
	{
		distance = 0;
		nextOnPath = null;
		ExitPoint = transform.localPosition;
	}

	//Eventually, all tiles should have a path, so their distance is no longer equal to int.MaxValue
	public bool HasPath => distance != int.MaxValue;


	//Grow the path to one of its neighbors, defined via a parameter. The neighbor's distance becomes one longer than its own, and the neighbor's path points toward this tile.
	//public methods that instruct a tile to grow its path in a specific direction, indirectly invoking GrowPathTo
	GameTile GrowPathTo(GameTile neighbor, Direction direction)
	{
		Debug.Assert(HasPath, "No path!");
		if (neighbor == null || neighbor.HasPath)
		{
			return null;
		}
		neighbor.distance = distance + 1;
		neighbor.nextOnPath = this;

        neighbor.ExitPoint =
            neighbor.transform.localPosition + direction.GetHalfVector();

        //If our content is sand and the previous one is sand or spawner, the local exit point will be reduce by o.5f on the y axis
        if (content.Type == GameTileContentType.Sand && (neighbor.content.Type == GameTileContentType.Sand || neighbor.content.Type == GameTileContentType.SpawnPoint))
        {
            neighbor.ExitPoint = new Vector3(neighbor.ExitPoint.x, neighbor.ExitPoint.y - 2f, neighbor.ExitPoint.z);
        }

		neighbor.PathDirection = direction;
		return
			neighbor.Content.BlockPath ? null : neighbor;
	}

	public GameTile GrowPathNorth() => GrowPathTo(north, Direction.South);

	public GameTile GrowPathEast() => GrowPathTo(east, Direction.West);

	public GameTile GrowPathSouth() => GrowPathTo(south, Direction.North);

	public GameTile GrowPathWest() => GrowPathTo(west, Direction.East);

	//If the distance is zero then the tile is a destination and it has nowhere to point to, so deactivate its arrow. Otherwise, active the arrow and set its rotation
	public void ShowPath()
	{
		if (distance == 0)
		{
			arrow.gameObject.SetActive(false);
			return;
		}
		arrow.gameObject.SetActive(true);
		arrow.localRotation =
			nextOnPath == north ? northRotation :
			nextOnPath == east ? eastRotation :
			nextOnPath == south ? southRotation :
			westRotation;
	}

	//Returns the content, while its setter also recycles its previous content, 
	public GameTileContent Content
	{
		get => content;
		set
		{
			Debug.Assert(value != null, "Null assigned to content!");
			if (content != null && !isSwapping)
			{
				content.Recycle();
			}
			content = value;
			content.transform.localPosition = transform.localPosition;
            isSwapping = false;
		}
	}

	//Hide the arrows
	public void HidePath()
	{
		arrow.gameObject.SetActive(false);
	}

    //Show the grid
    public void ShowGrid(bool show)
    {
        if (show)
        {
            quad.SetActive(true);
        }
        else
        {
            quad.SetActive(false);
        }
    }
}