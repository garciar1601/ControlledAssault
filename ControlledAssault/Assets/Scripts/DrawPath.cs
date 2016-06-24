using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawPath : MonoBehaviour {
    public Unit unit;
    private LineRenderer line;
    private List<Vector3> points;
    public bool drawing;
    public LayerMask layerMask;
    // Use this for initialization
	void Start () 
    {
        line = gameObject.GetComponent<LineRenderer>();
        points = new List<Vector3>();
        Ray ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
        RaycastHit rayInfo = new RaycastHit();
        Physics.Raycast(ray, out rayInfo);
        AddPoint(rayInfo.point);
        drawing = true;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (drawing)
        {
            if (unit == null)
            {
                Destroy(gameObject);
            }
            if (Input.GetMouseButtonDown(0))
            {
                drawing = false;
                unit.FinishDraw(points);             
            }
            else
            {
                Ray ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
                RaycastHit rayInfo = new RaycastHit();
                Physics.Raycast(ray, out rayInfo);
                if (!points.Contains(rayInfo.point) && rayInfo.point.y > 10.0f) 
                {
                    if (CheckPoint(rayInfo.point))
                    {
                        AddPoint(rayInfo.point);
                    }
                    else
                    {
                        //unit.FailDraw();
                        //Destroy(gameObject);
                    }
                }
            }
        }
        else
        {
            if (unit != null && unit.controller is AIController)
            {
                AIController ai = (AIController)unit.controller;
                if (ai.GetState() != UnitState.USERPATH)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
	}

    private bool CheckPoint(Vector3 point)
    {
        Vector3 lastPoint = points[points.Count - 1];
        Vector3 forwardDirection = Vector3.Normalize(point - lastPoint);
        float distance = Vector3.Distance(point, lastPoint) + .2f;
        Vector3 leftDirection = Vector3.Normalize(Vector3.Cross(Vector3.up, forwardDirection));
        Vector3 leftPoint = lastPoint + (leftDirection * .2f);
        leftPoint = new Vector3(leftPoint.x, 0.4f, leftPoint.z);
        Vector3 rightPoint = lastPoint - (leftDirection * .2f);
        rightPoint = new Vector3(rightPoint.x, 0.4f, rightPoint.z);
        Vector3 lastRayPoint = new Vector3(lastPoint.x, 0.4f, lastPoint.z);
        Vector3 rayPoint = new Vector3(point.x, 0.4f, point.z);
        RaycastHit rayInfo = new RaycastHit();
        bool cast1 = Physics.Raycast(rayPoint, forwardDirection, out rayInfo, 0.2f, layerMask);
        bool cast2 = Physics.Raycast(leftPoint, forwardDirection, out rayInfo, distance, layerMask);
        bool cast3 = Physics.Raycast(rightPoint, forwardDirection, out rayInfo, distance, layerMask);
        bool cast4 = Physics.Raycast(lastRayPoint, Vector3.Normalize(rayPoint - lastRayPoint), out rayInfo, Vector3.Distance(lastRayPoint, rayPoint), layerMask);
        return (!cast1 && !cast2 && !cast3 && !cast4);
    }

    private void AddPoint(Vector3 point)
    {
        points.Add(point);
        line.SetVertexCount(points.Count);
        line.SetPosition(points.Count - 1, points[points.Count - 1]);
    }
}
