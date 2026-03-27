using System.Collections;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    Coroutine attackCor;

    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM AN ATTACKING ENEMY");
        enemy.canAttack = true; // Ensure we can attack when entering this state
        //enemy.StartCoroutine(enemy.ChangeState(enemy.WanderState, 3, 5));
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {

        if (enemy.GetVectorToTarget().sqrMagnitude > enemy.enemyConfig.endAttackRange * enemy.enemyConfig.endAttackRange || !enemy.IsTargetInSight())
        {
            if (enemy.canAttack)
            {
                Debug.Log("SWITCH BACK");
                enemy.SwitchState(enemy.ChaseState);
            }
            
        }
        else if (enemy.canAttack)
        {
            enemy.canAttack = false;
            Debug.Log("Attac");
            enemy.lookDir = enemy.GetVectorToTarget();
            enemy.attackBehavior.ExecuteRangedAttack(enemy);
        }

        
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        enemy.animancer.Stop();
        enemy.StopCoroutine(enemy.ResetAttack());
        Debug.Log("True");
        enemy.canAttack = true;
    }

}
