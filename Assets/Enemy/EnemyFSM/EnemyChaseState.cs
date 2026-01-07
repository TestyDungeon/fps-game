using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM A CHASING ENEMY");
        if(enemy.animator != null)
        {
            enemy.animator.speed = 2;
            enemy.animator.CrossFade("Walk", 1f, 0, 0);
        }
        
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        enemy.UpdateLastPlayerPosition();

        
            if(enemy.GetVectorToLastPlayerPosition().sqrMagnitude > enemy.enemyConfig.startAttackRange * enemy.enemyConfig.startAttackRange || !enemy.IsPlayerInSight())
            {
                if (enemy.enemyConfig.gravity != 0)
                {
                    enemy.GoInDirection(Vector3.ProjectOnPlane(enemy.GetVectorToLastPlayerPosition(), enemy.transform.up).normalized * enemy.enemyConfig.chaseSpeed);
                }
                else
                {
                    enemy.FlyInDirection(enemy.GetVectorToLastPlayerPosition().normalized * enemy.enemyConfig.chaseSpeed + enemy.transform.up * 3);
                }
            }
            else
            {
                enemy.SwitchState(enemy.AttackState);
            }
            enemy.RotateInDirection(Vector3.ProjectOnPlane(enemy.GetVectorToLastPlayerPosition(), enemy.transform.up).normalized);
        
    }

    public override void OnCollisionEnter(EnemyStateManager enemy)
    {
        
    }
}
