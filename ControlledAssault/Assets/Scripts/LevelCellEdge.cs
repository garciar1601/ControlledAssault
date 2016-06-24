using UnityEngine;
using UnityEngine.UI;

public abstract class LevelCellEdge : MonoBehaviour
{
    public LevelCell cell;

    public LevelCell otherCell;

    public LevelDirection direction;

    public Image image;

    public virtual void Initialize(LevelCell cell, LevelCell otherCell, LevelDirection direction)
    {
        this.cell = cell;
        this.otherCell = otherCell;
        this.direction = direction;
        cell.SetEdge(direction, this);
        transform.parent = cell.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = direction.ToRotation();
    }
}
