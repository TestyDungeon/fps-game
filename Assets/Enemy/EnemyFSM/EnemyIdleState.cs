using System.Collections;
using System.Dynamic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    Vector3 dir;
    public override void EnterState(EnemyStateManager enemy)
    {
        enemy.audioSource.enabled = false;
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {


        //enemy.GoInDirection(enemy.idleDir * enemy.enemyConfig.idleSpeed);
        enemy.animancer.Play(enemy.enemyConfig.idleAnimation);
        //Debug.Log("IM AN IDLE ENEMY");
        if (enemy.IsTargetInSight())
        {
            enemy.SwitchState(enemy.ChaseState);
        }
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }

    
}
