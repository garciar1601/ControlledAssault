using UnityEngine;
using System.Collections.Generic;

public class TeamManager {
    public List<Unit> visibleEnemies = new List<Unit>();
    public List<Unit> units = new List<Unit>();
    public TeamManager enemyTeam;
    public bool userTeam = false;
    public void Update()
    {
        visibleEnemies.Clear();
        foreach (Unit unit in units)
        {
            List<Unit> unitsInVision = InVision(unit);
            List<Unit> allies = AlliesInRange(unit);
            foreach (Unit enemy in unitsInVision)
            {
                if (!visibleEnemies.Contains(enemy))
                {
                    visibleEnemies.Add(enemy);
                }
            }
            if (unit.controller is AIController)
            {
                AIController controller = ((AIController)unit.controller);
                if (unitsInVision.Count > 0 && controller.GetState() != UnitState.FIGHT && controller.GetState() != UnitState.USERPATH && controller.GetState() != UnitState.USERDRAW)
                {
                    if (unitsInVision.Count <= allies.Count + 2)
                    {
                        controller.ChangeState(UnitState.CHASE);
                        controller.targetPosition = unitsInVision[0].transform.position;
                        controller.targetUnit = unitsInVision[0];
                    }
                    else
                    {
                        controller.ChangeState(UnitState.FLEE);
                        controller.targetUnit = unitsInVision[0];
                        controller.runCount = unitsInVision.Count;
                    }
                }
                else if (unitsInVision.Count > 0 && controller.GetState() == UnitState.CHASE)
                {
                    if (!(unitsInVision.Count <= allies.Count + 2))
                    {
                        controller.ChangeState(UnitState.FLEE);
                        controller.targetUnit = unitsInVision[0];
                    }
                }
                else if (unitsInVision.Count > 0 && controller.GetState() == UnitState.FIGHT)
                {
                    controller.targetPosition = unitsInVision[0].transform.position;
                    controller.targetUnit = unitsInVision[0];
                }
                else if (unitsInVision.Count == 0 && !controller.attacking && controller.GetState() != UnitState.CHASE && controller.GetState() != UnitState.USERPATH && controller.GetState() != UnitState.USERDRAW && controller.GetState() != UnitState.SCAN)
                {
                    controller.ChangeState(UnitState.WANDER);
                }
                else if (controller.GetState() == UnitState.FLEE && controller.runCount <= allies.Count + 2)
                {
                    controller.ChangeState(UnitState.CHASE);
                }
            }
        }
    }
    private List<Unit> InVision(Unit unit)
    {
        List<Unit> unitsInVision = new List<Unit>();
        foreach (Unit enemy in enemyTeam.units)
        {
            if (EnemySeen(unit, enemy))
            {
                unitsInVision.Add(enemy);
            }
        }
        if (unitsInVision.Count > 0)
        {
            Unit closestUnit = unitsInVision[0];
            float closestDistance = Vector3.Distance(unit.transform.position, closestUnit.transform.position);
            foreach (Unit enemy in unitsInVision)
            {
                float distance = Vector3.Distance(unit.transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestUnit = enemy;
                    closestDistance = distance;
                }
            }
            unitsInVision.Remove(closestUnit);
            unitsInVision.Insert(0, closestUnit);
        }
        return unitsInVision;
    }
    private bool AllyInRange(Unit unit, Unit enemy)
    {
        Vector3 unitPosition = unit.gameObject.transform.position;
        Vector3 enemyPosition = enemy.gameObject.transform.position;
        Vector3 forward = unit.gameObject.transform.forward;
        if (forward.x == 1 || forward.x == -1)
        {
            forward.z = 0;
        }
        float distance = Vector3.Distance(unitPosition, enemyPosition);

        Vector3 direction = unitPosition - enemyPosition;
        Vector3 leftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * 0.2f + enemyPosition;
        Vector3 rightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * -0.2f + enemyPosition;
        RaycastHit hitLeft = new RaycastHit();
        RaycastHit hitRight = new RaycastHit();
        bool blockedLeft = Physics.Raycast(unitPosition, (leftPos1 - unitPosition).normalized, out hitLeft, distance - 0.4f);
        if (blockedLeft && ShouldntBlock(hitLeft.collider.gameObject))
        {
            blockedLeft = false;
            RaycastHit[] infos = Physics.RaycastAll(unitPosition, (leftPos1 - unitPosition).normalized, distance - 0.4f);
            foreach (RaycastHit info in infos)
            {
                if (!ShouldntBlock(info.collider.gameObject))
                {
                    blockedLeft = true;
                }
            }
        }
        bool blockedRight = Physics.Raycast(unitPosition, (rightPos1 - unitPosition).normalized, out hitRight, distance - 0.4f);
        if (blockedRight && ShouldntBlock(hitRight.collider.gameObject))
        {
            blockedRight = false;
            RaycastHit[] infos = Physics.RaycastAll(unitPosition, (rightPos1 - unitPosition).normalized, distance - 0.4f);
            foreach (RaycastHit info in infos)
            {
                if (!ShouldntBlock(info.collider.gameObject))
                {
                    blockedRight = true;
                }
            }
        }
        return distance < 8.0f && (!blockedLeft || !blockedRight);
    }
    private bool EnemySeen(Unit unit, Unit enemy)
    {
        Vector3 unitPosition = unit.gameObject.transform.position;
        Vector3 enemyPosition = enemy.gameObject.transform.position;
        Vector3 forward = unit.gameObject.transform.forward;
        if (forward.x == 1 || forward.x == -1)
        {
            forward.z = 0;
        }
        float distance = Vector3.Distance(unitPosition, enemyPosition);

        Vector3 direction = unitPosition - enemyPosition;
        Vector3 leftPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * 0.2f + enemyPosition;
        Vector3 rightPos1 = Vector3.Normalize(Vector3.Cross(Vector3.up, direction)) * -0.2f + enemyPosition;
        RaycastHit hitLeft = new RaycastHit();
        RaycastHit hitRight = new RaycastHit();
        bool blockedLeft = Physics.Raycast(unitPosition, (leftPos1 - unitPosition).normalized, out hitLeft, distance - 0.2f);
        if (blockedLeft && ShouldntBlock(hitLeft.collider.gameObject))
        {
            blockedLeft = false;
            RaycastHit[] infos = Physics.RaycastAll(unitPosition, (leftPos1 - unitPosition).normalized, distance - 0.2f);
            foreach (RaycastHit info in infos)
            {
                if (!ShouldntBlock(info.collider.gameObject))
                {
                    blockedLeft = true;
                }
            }            
        }
        bool blockedRight = Physics.Raycast(unitPosition, (rightPos1 - unitPosition).normalized, out hitRight, distance - 0.2f);
        if (blockedRight && ShouldntBlock(hitRight.collider.gameObject))
        {
            blockedRight = false;
            RaycastHit[] infos = Physics.RaycastAll(unitPosition, (rightPos1 - unitPosition).normalized, distance - 0.2f);
            foreach (RaycastHit info in infos)
            {
                if (!ShouldntBlock(info.collider.gameObject))
                {
                    blockedRight = true;
                }
            }
        }
        return distance < 8.0f && Vector3.Angle(forward.normalized, (enemyPosition - unitPosition).normalized) < 60.0f && (!blockedLeft || !blockedRight);    
    }
    private bool ShouldntBlock(GameObject obj)
    {
        return obj.layer == 8 || obj.layer == 9 || obj.layer == 10 || obj.layer == 11 || obj.layer == 12;
    }
    private List<Unit> AlliesInRange(Unit unit)
    {
        List<Unit> ret = new List<Unit>();
        foreach (Unit ally in units)
        {
            if (unit != ally && AllyInRange(unit, ally))
            {
                ret.Add(unit);
            }
        }
        return ret;
    }
}
