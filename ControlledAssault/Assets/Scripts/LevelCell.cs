using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelCell : MonoBehaviour 
{
    public IntVector2 coordinates;
    public LevelCellEdge[] edges = new LevelCellEdge[LevelDirections.Count];
    private int initializedEdgeCount;
    public LevelRoom room;
    public Image twoDImage;
    public List<Image> twoDPassages = new List<Image>();

    public void InitializeCell(LevelRoom room)
    {
        room.Add(this);
        transform.GetChild(0).GetComponent<Renderer>().material = room.mats.floorMaterial;
    }

    public bool IsFullyInitialized
    {
        get
        {
            return initializedEdgeCount == LevelDirections.Count;
        }
    }

    public LevelDirection RandomUninitializedDirection
    {
        get
        {
            int skips = Random.Range(0, LevelDirections.Count - initializedEdgeCount);
            for (int i = 0; i < LevelDirections.Count; i++)
            {
                if (edges[i] == null)
                {
                    if (skips == 0)
                    {
                        return (LevelDirection)i;
                    }
                    skips -= 1;
                }
            }
            return 0;
        }
       
    }

    public LevelCellEdge GetEdge(LevelDirection direction)
    {
        return edges[(int)direction];
    }

    public void SetEdge(LevelDirection direction, LevelCellEdge edge)
    {
        edges[(int)direction] = edge;
        initializedEdgeCount += 1;
    }
}
