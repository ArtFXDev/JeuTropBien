using UnityEngine;
using UnityEngine.SceneManagement;

[SelectionBase]
public class GameTileContent : MonoBehaviour
{	
	//Values
	GameTileContentFactory originFactory;

	[SerializeField]
	GameTileContentType type = default;

	public GameTileContentType Type => type;

	public bool BlockPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

	//---------------------------------------------------------------
	//Functions
	public GameTileContentFactory OriginFactory
	{
		get => originFactory;
		set
		{
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}

	public void Recycle()
	{
		originFactory.Reclaim(this);
	}

	public virtual void GameUpdate() {}
}