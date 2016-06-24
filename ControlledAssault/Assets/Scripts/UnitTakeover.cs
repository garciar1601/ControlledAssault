using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UnitTakeover : MonoBehaviour, IPointerClickHandler
{
    private GameManager manager;
    private Unit unit;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right){
            manager.SetUserUnit(unit);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            unit.DrawPath();
        }
    }
    public void SetManager(GameManager manager)
    {
        this.manager = manager;
    }
    public void SetUnit(Unit unit)
    {
        this.unit = unit;
    }
}
