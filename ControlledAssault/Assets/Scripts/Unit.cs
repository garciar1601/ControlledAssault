using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour {
    public UnitState state;
    public DrawPath drawPath;
    public UnitController controller;
    public Image twoDImage;
    public TeamManager team;
    public bool swinging = false;
    public bool hit = false;
    public bool visible = false;
    public LayerMask layerMask;
    public int health;
	void Update () {
        controller.ControlUnit(this);
        if (controller is AIController)
        {
            AIController ai = (AIController)controller;
            state = ai.state;
        }
	}

    public void SetController(UnitController controller)
    {
        if (controller is AIController)
        {
            AIController ai = (AIController)controller;
            ai.layerMask = layerMask;
        }
        this.controller = controller;
    }
    public void DrawPath()
    {
        if (team.userTeam)
        {
            AIController ai = (AIController)controller;
            ai.ChangeState(UnitState.USERDRAW);
            ai.currentDrawn = Vector3.zero;
            DrawPath path = Instantiate(drawPath) as DrawPath;
            path.unit = this;
        }
    }
    public void FinishDraw(List<Vector3> drawnPath)
    {
        AIController ai = (AIController)controller;
        ai.SetDrawnList(drawnPath);
        ai.ChangeState(UnitState.USERPATH);
    }
    public void FailDraw()
    {
        AIController ai = (AIController)controller;
        ai.ChangeState(UnitState.WANDER);
    }
    public bool Swing()
    {
        bool ret = false;
        if (!swinging)
        {
            StartCoroutine(SwingAnimation());
            swinging = true;
            ret = true;
        }
        return ret;
    }
    private IEnumerator SwingAnimation()
    {
        hit = false;
        float rotation = 0.0f;
        Vector3 defaultPosition = gameObject.transform.GetChild(1).localPosition;
        Quaternion defaultRotation = gameObject.transform.GetChild(1).localRotation;
        while (rotation < 75.0f)
        {
            gameObject.transform.GetChild(1).localRotation = defaultRotation;
            gameObject.transform.GetChild(1).localPosition = defaultPosition;
            Vector3 pivot = gameObject.transform.GetChild(1).position + new Vector3(0.0f, -.25f, 0.0f);
            Vector3 axis = gameObject.transform.GetChild(1).forward;
            rotation += 400.0f * Time.deltaTime;
            gameObject.transform.GetChild(1).RotateAround(pivot, axis, rotation);
            gameObject.transform.GetChild(1).RotateAround(pivot, gameObject.transform.GetChild(1).right, -(rotation * .2f));
            yield return new WaitForSeconds(0.0f);
        }
        RaycastHit[] hitInfos = Physics.RaycastAll(gameObject.transform.position, gameObject.transform.forward, 0.5f);
        if (hitInfos.Length > 0)
        {
            foreach (RaycastHit info in hitInfos)
            {
                if (!hit)
                {
                    GameObject collided = info.collider.gameObject;
                    if (collided.tag == "Unit")
                    {
                        Unit unitHit = collided.transform.parent.gameObject.GetComponent<Unit>();
                        if (unitHit.team.Equals(team.enemyTeam))
                        {
                            hit = true;
                            unitHit.health -= 1;
                        }
                    }
                    else if (collided.tag == "Breakable")
                    {
                        hit = true;
                        LevelBreakableWall wall = collided.transform.parent.gameObject.GetComponent<LevelBreakableWall>();
                        wall.health -= 1;
                    }
                }
            }
        }
        else
        {
            hit = false;
        }
        while (rotation > 0.0f)
        {
            gameObject.transform.GetChild(1).localRotation = defaultRotation;
            gameObject.transform.GetChild(1).localPosition = defaultPosition;
            Vector3 pivot = gameObject.transform.GetChild(1).position + new Vector3(0.0f, -.25f, 0.0f);
            Vector3 axis = gameObject.transform.GetChild(1).forward;
            rotation -= 400.0f * Time.deltaTime;
            gameObject.transform.GetChild(1).RotateAround(pivot, axis, rotation);
            gameObject.transform.GetChild(1).RotateAround(pivot, gameObject.transform.GetChild(1).right, -(rotation * .2f));
            yield return new WaitForSeconds(0.0f);
        }
        gameObject.transform.GetChild(1).localRotation = defaultRotation;
        gameObject.transform.GetChild(1).localPosition = defaultPosition;
        swinging = false;
    }
}
