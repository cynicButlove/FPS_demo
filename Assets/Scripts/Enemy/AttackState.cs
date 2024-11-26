using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : EnemyBaseState
{
    public override void EnterState(Enemy enemy)
    {
        Debug.Log("½øÈë¹¥»÷×´Ì¬");
    }

    public override void OnUpdate(Enemy enemy)
    {
        enemy.MoveToTarget(enemy.player.position);
    }

}
