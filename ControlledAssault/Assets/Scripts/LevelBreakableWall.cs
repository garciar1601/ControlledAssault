using UnityEngine;
using System.Collections;

public class LevelBreakableWall : LevelCellEdge
{
    public Transform wall;
    public Material material;
    public int health = 5;

    public override void Initialize(LevelCell cell, LevelCell otherCell, LevelDirection direction)
    {
        base.Initialize(cell, otherCell, direction);
        wall.GetComponent<Renderer>().material = material;
        this.cell = cell;
        this.otherCell = otherCell;
        this.direction = direction;
    }
}
