using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM A CHASING ENEMY");
        //if(enemy.animator != null)
        //{
        //    enemy.animator.speed = 2;
        //    enemy.animator.CrossFade("Walk", 1f, 0, 0);
        //}
        enemy.SetNavmeshAgent(true);
        enemy.audioSource.enabled = true;
        enemy.UpdateTargetPosition();
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        //Debug.Log("Reach" + enemy.IsTargetReachable()  + "OnNavmesh" + enemy.agent.isOnNavMesh);
        //enemy.RotateInDirection(enemy.agent.desiredVelocity);
        if(!enemy.IsTargetReachable() && enemy.IsOnUsableNavMesh() && enemy.GetCurrentState() == enemy.ChaseState)
        {
            enemy.SwitchState(enemy.WanderState);
        }
        enemy.traversalBehavior.Chase(enemy);
        enemy.RotateInDirection();

        //if ((enemy.transform.position - enemy.targetTransform.position).sqrMagnitude < 2 * 2)
        //{
        //    enemy.SwitchState(enemy.AttackState);
        //}
        
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }
}
