using System.Collections;
using System.Dynamic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    Vector3 dir;
    public override void EnterState(EnemyStateManager enemy)
    {
        enemy.InvokeRepeating("GetRandomDirection", 0, 2);
        
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        enemy.movementController.Move(Vector3.zero);


        //enemy.GoInDirection(enemy.idleDir * enemy.enemyConfig.idleSpeed);
        if(enemy.animator != null)
            enemy.animator.Play("Idle1");
        //Debug.Log("IM AN IDLE ENEMY");
        if (enemy.IsPlayerInSight())
        {
            enemy.SwitchState(enemy.ChaseState);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager enemy)
    {

    }

    
}
