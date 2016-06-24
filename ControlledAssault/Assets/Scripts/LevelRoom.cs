using UnityEngine;
using System.Collections.Generic;

public class LevelRoom : ScriptableObject
{
    public int matIndex;

    public LevelRoomMaterials mats;

    public List<LevelCell> cells = new List<LevelCell>();

    public void Add(LevelCell cell)
    {
        cell.room = this;
        cells.Add(cell);
    }
    public void Join(LevelRoom room)
    {
        for (int i = 0; i < room.cells.Count; i++)
        {
            Add(room.cells[i]);
        }
    }

}
