using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AIController : UnitController {
    public bool attacking = false;
    private float cooldown = 0.0f;
    public int runCount;
    List<Node> nodeSystem = new List<Node>();
    List<Node> pathList = new List<Node>();
    List<Vector3> drawnList = new List<Vector3>();
    Node currentNode;
    public Vector3 currentDrawn;
    public Vector3 targetPosition;
    public Unit targetUnit;
    float beta;
    float speed = 1.5f;
    float degree = 0.0f;
    public UnitState state = UnitState.WANDER;
    public LayerMask layerMask;
    public void ChangeState(UnitState state)
    {
        this.state = state;
    }
    public UnitState GetState()
    {
        return state;
    }
    public AIController(List<Node> nodeSystem)
    {
        this.nodeSystem = nodeSystem;
    }
    private void Fight(Unit unit)
    {
        Chase(unit);
        if (!attacking && cooldown <= 0.0f)
        {
            unit.Swing();
            attacking = true;
            cooldown = Random.Range(0.0f, .25f);
        }
        else if(attacking)
        {
            if(!unit.swinging){
                attacking = false;
                if (!unit.hit)
                {

                    state = UnitState.SCAN;
                }
            }
        }
        else
        {
            cooldown -= Time.deltaTime;
        }
        if (targetUnit == null || targetUnit.health <= 0)
        {
            state = UnitState.WANDER;
        }
    }    
    private void GetFirstNode(Unit unit)
    {
        currentNode = new Node();
        currentNode.position = unit.transform.position;
        currentNode.position.y = .01f;
        pathList = new List<Node>();
        float minDistance = float.MaxValue;
        foreach (Node node in nodeSystem)
        {
            float distance = Vector3.Distance(currentNode.position, node.position);
            if (pathList.Count == 0)
            {
                pathList.Add(node);
                minDistance = distance;
            }
            else if(distance < minDistance)
            {
                pathList.RemoveAt(0);
                pathList.Add(node);
                minDistance = distance;
            }
        }
    }
    private void Wander(Unit unit)
    {
        if (currentNode == null)
        {
            GetFirstNode(unit);
        }
        else
        {
            if (pathList.Count == 0)
            {
                bool newNodeFound = false;
                Node targetNode = null;
                while (!newNodeFound)
                {
                    int id = Random.Range(0, nodeSystem.Count);
                    ++Random.seed;
                    if (id != currentNode.id)
                    {
                        targetNode = nodeSystem[id];
                        newNodeFound = true;
                    }
                }
                AStar(targetNode);
            }
            else
            {
                if (beta < 1.0f)
                {
                    RaycastHit hitInfo = new RaycastHit();
                    bool hit = Physics.Raycast(unit.gameObject.transform.position, unit.gameObject.transform.forward, out hitInfo, 0.4f);

                    if (hit && hitInfo.collider.tag == "Breakable")
                    {
                        if (!unit.swinging)
                        {
                            unit.Swing();
                        }
                    }
                    else
                    {
                        Lerp(unit);
                    }
                }
                else
                {
                    currentNode = pathList[0];
                    pathList.RemoveAt(0);
                    beta = 0.0f;
                    if (pathList.Count > 0)
                    {
                        Lerp(unit);
                    }
                }
            }
        }
    }
    private void Flee(Unit unit)
    {
        if (currentNode != null)
        {
            ResetPath();
        }
        if(Vector3.Distance(targetUnit.transform.position, unit.transform.position) <= 0.4f)
        {
            unit.transform.LookAt(targetUnit.transform.position);
            state = UnitState.FIGHT;
        }
        else
        {
            //unit.transform.LookAt(targetUnit.transform.position);
            //unit.transform.RotateAround(unit.transform.position, Vector3.up, 180);

            /*Vector3 direction = unit.transform.forward;
            Vector3 leftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * 0.2f + unit.transform.position;
            Vector3 rightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * -0.2f + unit.transform.position;
            Vector3 forwardLeftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, unit.transform.forward)) * 0.2f + unit.transform.position;
            Vector3 forwardRightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, unit.transform.forward)) * -0.2f + unit.transform.position;*/

            Vector3 direction = -(targetPosition - unit.transform.position);
            Vector3 leftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * 0.2f + unit.transform.position;
            Vector3 rightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * -0.2f + unit.transform.position;
            Vector3 forwardLeftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, unit.transform.forward)) * 0.2f + unit.transform.position;
            Vector3 forwardRightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, unit.transform.forward)) * -0.2f + unit.transform.position;
            float targetDistance = Vector3.Distance(targetPosition, unit.transform.position);
            float leftDistance;
            float rightDistance;
            float forwardLeftDistance;
            float forwardRightDistance;
            RaycastHit hit = new RaycastHit();
            int layerMask = 1 << 8;
            bool didHit = Physics.Raycast(forwardLeftPos1, unit.transform.forward, out hit, layerMask);
            forwardLeftDistance = hit.distance;
            if (didHit && ShouldntBlock(hit.collider.gameObject))
            {
                forwardLeftDistance = float.MaxValue;
            }
            didHit = Physics.Raycast(forwardRightPos1, unit.transform.forward, out hit, layerMask);
            forwardRightDistance = hit.distance;
            if (didHit && ShouldntBlock(hit.collider.gameObject))
            {
                forwardRightDistance = float.MaxValue;
            }
            didHit = Physics.Raycast(leftPos1, direction, out hit, layerMask);
            leftDistance = hit.distance;
            if (didHit && ShouldntBlock(hit.collider.gameObject))
            {
                leftDistance = float.MaxValue;
            }
            didHit = Physics.Raycast(rightPos1, direction, out hit, layerMask);
            rightDistance = hit.distance;
            if (didHit && ShouldntBlock(hit.collider.gameObject))
            {
                rightDistance = float.MaxValue;
            }
            if (forwardLeftDistance < .4f || forwardRightDistance < .4f)
            {
                bool test = TestAngle(unit, 22.5f);
                if (!test)
                {
                    test = TestAngle(unit, 45.0f);
                    if (!test)
                    {
                        test = TestAngle(unit, 67.5f);
                        if (!test)
                        {
                            state = UnitState.CHASE;
                        }
                    }
                }
            }
            else if (leftDistance > targetDistance && rightDistance > targetDistance || leftDistance > forwardLeftDistance && rightDistance > forwardRightDistance)
            {
                unit.transform.LookAt(targetPosition);
                unit.transform.RotateAround(unit.transform.position, Vector3.up, 180);
            }
            Vector3 forward = unit.transform.forward;
            if (forward.x == 1 || forward.x == -1)
            {
                forward.z = 0;
            }
            unit.transform.position += forward * speed * Time.deltaTime;
            if (targetUnit == null || targetUnit.health <= 0)
            {
                state = UnitState.WANDER;
            }
            if (Vector3.Distance(unit.transform.position, targetPosition) > 8.0f)
            {
                state = UnitState.WANDER;
            }
        }
    }
    private void Chase(Unit unit)
    {
        bool chase = true;
        float scale = 1.0f;
        if (state == UnitState.FIGHT)
        {
            scale = .25f;
            if (Vector3.Distance(unit.transform.position, targetPosition) < 0.4f)
            {
                unit.transform.LookAt(targetPosition);
                if (Physics.Raycast(unit.transform.position, targetPosition - unit.transform.position, 0.4f))
                {
                    chase = false;
                }
                else
                {
                    state = UnitState.SCAN;
                }
            }
        }
        if (currentNode != null)
        {
            ResetPath();
        }
        if (chase)
        {
            Vector3 direction = targetPosition - unit.transform.position;
            Vector3 leftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * 0.2f + unit.transform.position;
            Vector3 rightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * -0.2f + unit.transform.position;
            Vector3 forwardLeftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, unit.transform.forward)) * 0.2f + unit.transform.position;
            Vector3 forwardRightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, unit.transform.forward)) * -0.2f + unit.transform.position;
            float targetDistance = Vector3.Distance(targetPosition, unit.transform.position);
            float leftDistance;
            float rightDistance;
            float forwardLeftDistance;
            float forwardRightDistance;
            RaycastHit hit = new RaycastHit();
            int layerMask = 1 << 8;
            Physics.Raycast(forwardLeftPos1, unit.transform.forward, out hit, layerMask);
            forwardLeftDistance = hit.distance;
            if (ShouldntBlock(hit.collider.gameObject))
            {
                forwardLeftDistance = float.MaxValue;
            }
            Physics.Raycast(forwardRightPos1, unit.transform.forward, out hit, layerMask);
            forwardRightDistance = hit.distance;
            if (ShouldntBlock(hit.collider.gameObject))
            {
                forwardRightDistance = float.MaxValue;
            }
            Physics.Raycast(leftPos1, direction, out hit, layerMask);
            leftDistance = hit.distance;
            if (ShouldntBlock(hit.collider.gameObject))
            {
                leftDistance = float.MaxValue;
            }
            Physics.Raycast(rightPos1, direction, out hit, layerMask);
            rightDistance = hit.distance;
            if (ShouldntBlock(hit.collider.gameObject))
            {
                rightDistance = float.MaxValue;
            }
            if (forwardLeftDistance < .4f || forwardRightDistance < .4f)
            {
                bool test = TestAngle(unit, 22.5f);
                if (!test)
                {
                    test = TestAngle(unit, 45.0f);
                    if (!test)
                    {
                        test = TestAngle(unit, 67.5f);
                        if (!test)
                        {
                            state = UnitState.WANDER;
                        }
                    }
                }
            }
            else if (leftDistance > targetDistance && rightDistance > targetDistance || leftDistance > forwardLeftDistance && rightDistance > forwardRightDistance)
            {
                unit.transform.LookAt(targetPosition);
            }
            Vector3 forward = unit.transform.forward;
            if (forward.x == 1 || forward.x == -1)
            {
                forward.z = 0;
            }
            unit.transform.position += forward * speed * Time.deltaTime * scale;
            if (targetUnit == null || targetUnit.health <= 0)
            {
                state = UnitState.WANDER;
            }
            if (Vector3.Distance(unit.transform.position, targetPosition) < 0.4f)
            {
                unit.transform.LookAt(targetPosition);
                if (Physics.Raycast(unit.transform.position, targetPosition - unit.transform.position, 0.4f))
                {
                    state = UnitState.FIGHT;
                }
                else
                {
                    state = UnitState.SCAN;
                }
            }
            else if (Vector3.Distance(unit.transform.position, targetPosition) > 8.0f)
            {
                state = UnitState.WANDER;
            }
        }
    }
    private bool TestAngle(Unit unit, float angle)
    {
        float left;
        float right;
        RayCastTest(unit, angle, out left, out right);
        bool ret = left > .65f || right > .65f;
        if (ret)
        {
            if (left > right)
            {
                unit.transform.RotateAround(unit.transform.position, Vector3.up, angle);
            }
            else
            {
                unit.transform.RotateAround(unit.transform.position, Vector3.up, -angle);
            }
        }
        return ret;
    }
    private void RayCastTest(Unit unit, float angle, out float left, out float right)
    {
        RaycastHit hit = new RaycastHit();
        int layerMask = 1 << 8;
        Physics.Raycast(unit.transform.position, Quaternion.AngleAxis(angle, Vector3.up) * unit.transform.forward, out hit, layerMask);
        left = hit.distance;
        if (ShouldntBlock(hit.collider.gameObject))
        {
            left = float.MaxValue;
        }
        Physics.Raycast(unit.transform.position, Quaternion.AngleAxis(-angle, Vector3.up) * unit.transform.forward, out hit, layerMask);
        right = hit.distance;
        if (ShouldntBlock(hit.collider.gameObject))
        {
            right = float.MaxValue;
        }
    }
    private bool ShouldntBlock(GameObject obj)
    {
        return obj.layer == 9 || obj.layer == 10 || obj.layer == 11 || obj.layer == 12;
    }
    private void Scan(Unit unit)
    {
        unit.gameObject.transform.Rotate(Vector3.up, 720.0f * Time.deltaTime);
        degree += 720.0f * Time.deltaTime;
        if (degree >= 360.0f)
        {
            state = UnitState.WANDER;
            degree = 0.0f;
        }
    }
    public void ControlUnit(Unit unit)
    {
        switch (state)
        {
            case UnitState.WANDER:
                degree = 0.0f;
                Wander(unit);
                break;
            case UnitState.FLEE:
                degree = 0.0f;
                Flee(unit);
                break;
            case UnitState.CHASE:
                degree = 0.0f;
                Chase(unit);
                break;
            case UnitState.USERPATH:
                degree = 0.0f;
                UserPath(unit);
                break;
            case UnitState.USERDRAW:
                degree = 0.0f;
                if (currentNode != null)
                {
                    ResetPath();
                }
                break;
            case UnitState.FIGHT:
                degree = 0.0f;
                Fight(unit);
                break;
            case UnitState.SCAN:
                Scan(unit);
                break;
        }
        unit.twoDImage.transform.localPosition = new Vector3(unit.transform.position.x, unit.transform.position.z, 0);
    }
    private void Lerp(Unit unit)
    {
        Vector3 newPosition = new Vector3();
        Node targetNode = pathList[0];
        newPosition.x = ((1 - beta) * currentNode.position.x) + (beta * targetNode.position.x);
        newPosition.y = 0.4f;
	    newPosition.z = ((1 - beta) * currentNode.position.z) + (beta * targetNode.position.z);

        beta += speed/Vector3.Distance(currentNode.position, targetNode.position) * Time.deltaTime;
        unit.gameObject.transform.position = newPosition;
        Vector3 lookDirection = targetNode.position - currentNode.position;
        if (lookDirection.x != 0 || lookDirection.z != 0)
        {
            unit.gameObject.transform.rotation = Quaternion.LookRotation(targetNode.position - currentNode.position);
        }
    }
    private void AStar(Node targetNode)
    {
        List<AStarNode> closedList = new List<AStarNode>();
        List<AStarNode> openList = new List<AStarNode>();

        AStarNode start = new AStarNode();
        start.id = currentNode.id;
        start.cost = 0;
        start.TEC = start.cost + Vector3.Distance(currentNode.position, targetNode.position);
        start.parent = null;

        openList.Add(start);

        bool pathFound = false;
        while (!pathFound)
        {
            AStarNode current = null;
            foreach (AStarNode aNode in openList)
            {
                if (current == null)
                {
                    current = aNode;
                }
                else
                {
                    if (aNode.TEC < current.TEC)
                    {
                        current = aNode;
                    }
                }
            }
            openList.Remove(current);
            closedList.Add(current);
            if (current.id == targetNode.id)
            {
                pathFound = true;
            }
            if (!pathFound) 
            { 
                Node node = nodeSystem[current.id];
                foreach (int i in node.connections)
                {
                    Node neighborNode = nodeSystem[i];
                    AStarNode neighbor = new AStarNode();
                    neighbor.id = neighborNode.id;
                    neighbor.cost = current.cost + Vector3.Distance(node.position, neighborNode.position);
                    neighbor.TEC = neighbor.cost + Vector3.Distance(neighborNode.position, targetNode.position);
                    neighbor.parent = current;
                    bool addToList = true;
                    List<AStarNode> openRemoval = new List<AStarNode>();
                    foreach (AStarNode aNode in openList)
                    {
                        if (neighbor.id == aNode.id)
                        {
                            if (neighbor.cost < aNode.cost)
                            {
                                openRemoval.Add(aNode);
                            }
                            else
                            {
                                addToList = false;
                            }
                        }
                    }
                    foreach (AStarNode aNode in openRemoval)
                    {
                        openList.Remove(aNode);
                    }
                    List<AStarNode> closedRemoval = new List<AStarNode>();
                    foreach (AStarNode aNode in closedList)
                    {
                        if (neighbor.id == aNode.id)
                        {
                            if (neighbor.cost < aNode.cost)
                            {
                                closedRemoval.Add(aNode);
                            }
                            else
                            {
                                addToList = false;
                            }
                        }
                    }
                    foreach (AStarNode aNode in closedRemoval)
                    {
                        closedList.Remove(aNode);
                    }
                    if (addToList)
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }
        AStarNode endNode = closedList[closedList.Count - 1];
        bool endOfPath = false;
        List<int> reversePath = new List<int>();
        while (!endOfPath)
        {
            reversePath.Add(endNode.id);
            if (endNode.parent != null)
            {
                endNode = endNode.parent;
            }
            else
            {
                endOfPath = true;
            }
        }
        for (int i = reversePath.Count - 1; i >= 0; i--)
        {
            pathList.Add(nodeSystem[reversePath[i]]);
        }
    }
    private void ResetPath()
    {
       currentNode = null;  
       beta = 0.0f;
       pathList.Clear();      
    }
    public void SetDrawnList(List<Vector3> drawnList)
    {
        this.drawnList = drawnList;
    }
    private void UserPath(Unit unit)
    {
        if (currentDrawn.Equals(Vector3.zero))
        {
            currentDrawn = new Vector3(unit.gameObject.transform.position.x, 10.0f, unit.gameObject.transform.position.z);
            //currentDrawn = drawnList[0];
            //drawnList.RemoveAt(0);
        }
        LerpDrawn(unit, true, 0.0f);
    }
    private void LerpDrawn(Unit unit, bool increaseBeta, float betaScale)
    {
        Vector3 newPosition = new Vector3();
        Vector3 targetVector = drawnList[0];
        if (increaseBeta)
        {
            beta += speed / Vector3.Distance(currentDrawn, targetVector) * Time.deltaTime;
            betaScale = Vector3.Distance(currentDrawn, targetVector) * Time.deltaTime;
        }
        else
        {
            beta *= betaScale / Vector3.Distance(currentDrawn, targetVector) * Time.deltaTime;
            betaScale = Vector3.Distance(currentDrawn, targetVector) * Time.deltaTime;
        }
        if (beta > 1.0f)
        {
            beta -= 1.0f;
            drawnList.RemoveAt(0);
            if (drawnList.Count > 0)
            {
                currentDrawn = targetVector;
                LerpDrawn(unit, false, betaScale);
            }
            else
            {
                newPosition.x = targetVector.x;
                newPosition.y = 0.4f;
                newPosition.z = targetVector.z;
                unit.gameObject.transform.position = newPosition;
                Vector3 lookDirection = targetVector - currentDrawn;
                if (lookDirection.x != 0 || lookDirection.z != 0)
                {
                    unit.gameObject.transform.rotation = Quaternion.LookRotation(lookDirection);
                }
                state = UnitState.WANDER;
                ResetPath();
            }
        }
        else
        {
            newPosition.x = ((1 - beta) * currentDrawn.x) + (beta * targetVector.x);
            newPosition.y = 0.4f;
            newPosition.z = ((1 - beta) * currentDrawn.z) + (beta * targetVector.z);
            unit.gameObject.transform.position = newPosition;
            Vector3 lookDirection = targetVector - currentDrawn;
            if (lookDirection.x != 0 || lookDirection.z != 0)
            {
                unit.gameObject.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        
        
    }
}
