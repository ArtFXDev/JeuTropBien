using UnityEngine;

public class Enemy : GameBehavior
{
	//Values
	EnemyFactory originFactory;

	GameTile tileFrom, tileTo;
	Vector3 positionFrom, positionTo;
	float progress, progressFactor;
	float pathOffset;
	float speed;
    public float Scale { get; private set; }
	public float Health { get; set; }

    //For the rotation of the enemy
    Direction direction;
	DirectionChange directionChange;
	float directionAngleFrom, directionAngleTo;

	//Reference to its model
	[SerializeField]
	Transform model = default;
	//---------------------------------------------------------------------------
	//Functions
	public EnemyFactory OriginFactory
	{
		get => originFactory;
		set
		{
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}

	//Set the spawn position
	public void SpawnOn(GameTile tile)
	{
		tileFrom = tile;
		tileTo = tile.NextTileOnPath;
		progress = 0f;
		PrepareIntro();
	}

	//Update the enemy for making them move and check health
	public override bool GameUpdate()
	{
		if(Health <= 0f)
		{
            Recycle();
            return false;
		}

		progress += Time.deltaTime * progressFactor;
		while (progress >= 1f)
		{
            //Destination reached
			if (tileTo == null)
			{
                Game.EnemyReachedDestination(); //Tell the game that we hit the player
                Recycle();
                return false;
			}
			progress = (progress - 1f) / progressFactor;
			PrepareNextState();
			progress *= progressFactor;
		}

		//Check if ther's a direction change
		if (directionChange == DirectionChange.None)
		{
			transform.localPosition =
				Vector3.LerpUnclamped(positionFrom, positionTo, progress);
		}
		else
		{
			float angle = Mathf.LerpUnclamped(
				directionAngleFrom, directionAngleTo, progress
			);
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}
		return true;
	}

	//When entering a new state, we always have to adjust the positions, find the direction change, update the current direction, and shift the To angle to From
	void PrepareNextState()
	{
		tileFrom = tileTo;
		tileTo = tileTo.NextTileOnPath;
		positionFrom = positionTo;
		if (tileTo == null)
		{
			PrepareOutro();
			return;
		}
		positionTo = tileFrom.ExitPoint;
		directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
		direction = tileFrom.PathDirection;
		directionAngleFrom = directionAngleTo;
		switch (directionChange)
		{
			case DirectionChange.None: PrepareForward(); break;
			case DirectionChange.TurnRight: PrepareTurnRight(); break;
			case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
			default: PrepareTurnAround(); break;
		}
	}

	//Prepare for direction angle
	void PrepareForward()
	{
		transform.localRotation = direction.GetRotation();
		directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		progressFactor = speed;
	}

	void PrepareTurnRight()
	{
		directionAngleTo = directionAngleFrom + 90f;
		model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
		transform.localPosition = positionFrom + direction.GetHalfVector();
		progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
	}

	void PrepareTurnLeft()
	{
		directionAngleTo = directionAngleFrom - 90f;
		model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
		transform.localPosition = positionFrom + direction.GetHalfVector();
		progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
	}

	void PrepareTurnAround()
	{
		directionAngleTo = directionAngleFrom + (pathOffset <0f ? 180f : -180f);
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localPosition = positionFrom;
		progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
	}

	public void Initialize(float scale, float speed, float pathOffset)
	{
        Scale = scale;
		model.localScale = new Vector3(scale, scale, scale);
		this.speed = speed;
		this.pathOffset = pathOffset;

		Health = 100f * scale;
	}

	//State-preparation code
	void PrepareIntro()
	{
        //positionFrom = new Vector3(tileFrom.transform.localPosition.x, tileFrom.transform.localPosition.y - 0.5f, tileFrom.transform.localPosition.z);
        positionFrom = tileFrom.transform.localPosition;
		positionTo = tileFrom.ExitPoint;
		direction = tileFrom.PathDirection;
		directionChange = DirectionChange.None;
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localRotation = direction.GetRotation();
		progressFactor = 2 * speed;
	}

	void PrepareOutro()
	{
		positionTo = tileFrom.transform.localPosition;
		directionChange = DirectionChange.None;
		directionAngleTo = direction.GetAngle();
		model.localPosition = Vector3.zero;
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localRotation = direction.GetRotation();
		progressFactor = 2 * speed;
	}

    //For change shape of the enemy
    public void Initialize(float scale, float speed, float pathOffset, float health)
	{
		model.localScale = new Vector3(scale, scale, scale);
        this.speed = speed;
		this.pathOffset = pathOffset;
        Health = health;
	}

	public void ApplyDamage(float damage)
	{
		Debug.Assert(damage >= 0f, "Negative damage applied");
		Health -= damage;
	}

    public override void Recycle()
    {
        OriginFactory.Reclaim(this);
    }
}