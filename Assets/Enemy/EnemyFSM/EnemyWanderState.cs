using UnityEngine;

public class EnemyWanderState : EnemyBaseState
{
    Vector3 dest;
    public override void EnterState(EnemyStateManager enemy)
    {
        enemy.SetNavmeshAgent(true);
        enemy.StartCoroutine(enemy.ChangeState(enemy.AttackState, 0.5f, 0.7f));
        dest = enemy.GetRandomReachablePointOnNavMesh(10);
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        
        enemy.GoToDestination(dest, enemy.enemyConfig.wanderSpeed);
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }

    
}
