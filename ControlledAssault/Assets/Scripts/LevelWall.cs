using UnityEngine;
using System.Collections;

public class LevelWall : LevelCellEdge {
    public Transform wall;

    public override void Initialize(LevelCell cell, LevelCell otherCell, LevelDirection direction)
    {
        base.Initialize(cell, otherCell, direction);
        wall.GetComponent<Renderer>().material = cell.room.mats.wallMaterial;
    }
}
