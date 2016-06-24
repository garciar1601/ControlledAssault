using UnityEngine;

public enum LevelDirection
{
    North,
    East,
    South,
    West
}

public static class LevelDirections
{

    public const int Count = 4;
    
    private static IntVector2[] vectors = {
		new IntVector2(0, 1),
		new IntVector2(1, 0),
		new IntVector2(0, -1),
		new IntVector2(-1, 0)
	};

    private static LevelDirection[] opposites = {
		LevelDirection.South,
		LevelDirection.West,
		LevelDirection.North,
		LevelDirection.East
	};

    private static Quaternion[] rotations = {
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};

    public static Quaternion ToRotation(this LevelDirection direction)
    {
        return rotations[(int)direction];
    }

    public static LevelDirection GetOpposite(this LevelDirection direction)
    {
        return opposites[(int)direction];
    }

    public static LevelDirection RandomValue
    {
        get
        {
            return (LevelDirection)Random.Range(0, Count);
        }
    }

    public static IntVector2 ToIntVector2(this LevelDirection direction)
    {
        return vectors[(int)direction];
    }

}