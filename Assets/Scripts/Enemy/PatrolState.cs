using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : EnemyBaseState
{
    public override void EnterState(Enemy enemy)
    {
        enemy.LoadRath(enemy.wayPointObj[enemy.randomWay]);
    }

    public override void OnUpdate(Enemy enemy)
    {
        enemy.MoveToTarget();

    }
}
