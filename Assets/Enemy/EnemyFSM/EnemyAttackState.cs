using System.Collections;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM AN ATTACKING ENEMY");
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        if(!enemy.IsPlayerInSight())
            enemy.SwitchState(enemy.ChaseState);
            
        enemy.UpdateLastPlayerPosition();
        enemy.movementController.Move(Vector3.zero);
        enemy.RotateInDirection(Vector3.ProjectOnPlane(enemy.GetVectorToLastPlayerPosition(), enemy.transform.up).normalized);
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(0);
        //enemy.attackBehavior.AnimateAttack(enemy);
        enemy.attackBehavior.ExecuteAttack(enemy, enemy.GetPlayerPosition());
        if (stateInfo.normalizedTime >= 0.95f && !enemy.animator.IsInTransition(0) && enemy.GetVectorToLastPlayerPosition().sqrMagnitude > enemy.enemyConfig.endAttackRange * enemy.enemyConfig.endAttackRange)
        {
            Debug.Log("SWITCH BACK");
            enemy.SwitchState(enemy.ChaseState);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager enemy)
    {

    }

    
}
