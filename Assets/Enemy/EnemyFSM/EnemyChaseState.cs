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
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        //enemy.RotateInDirection(enemy.agent.desiredVelocity);
        enemy.traversalBehavior.Chase(enemy);
        
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }
}
