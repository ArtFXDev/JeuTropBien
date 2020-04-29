using UnityEngine;

//Differant face to rotate
public enum Direction
{
	North, East, South, West
}

//For interpolation, in wich direction i'm going to rotate
public enum DirectionChange
{
	None, TurnRight, TurnLeft, TurnAround
}

public static class DirectionExtensions
{
	//Values
	static Quaternion[] rotations = {
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};

	//The enemy itself has to move to the rotation point. Again this is by half a unit
	static Vector3[] halfVectors = {
		Vector3.forward * 0.5f,
		Vector3.right * 0.5f,
		Vector3.back * 0.5f,
		Vector3.left * 0.5f
	};

	//--------------------------------------------------------------------
	//Functions
	public static Quaternion GetRotation(this Direction direction)
	{
		return rotations[(int)direction];
	}

	//Returns the direction change from the current direction to the next
	public static DirectionChange GetDirectionChangeTo(
		this Direction current, Direction next
	)
	{
		if (current == next)
		{
			return DirectionChange.None;
		}
		else if (current + 1 == next || current - 3 == next)
		{
			return DirectionChange.TurnRight;
		}
		else if (current - 1 == next || current + 3 == next)
		{
			return DirectionChange.TurnLeft;
		}
		return DirectionChange.TurnAround;
	}

	//Gets the angle of a direction, in degrees
	public static float GetAngle(this Direction direction)
	{
		return (float)direction * 90f;
	}

	//The exact offset depends on the direction
	public static Vector3 GetHalfVector(this Direction direction)
	{
		return halfVectors[(int)direction];
	}
}