using UnityEngine;

public class EnemyWanderState : EnemyBaseState
{
    Vector3 dest;
    float lastTime = -1;
    float lastTimeArrived = -1;
    public override void EnterState(EnemyStateManager enemy)
    {
        enemy.SetNavmeshAgent(true);
        enemy.StartCoroutine(enemy.SwitchState(enemy.ChaseState, 1f, 2f));
        
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        bool reachedDestination = (enemy.transform.position - dest).sqrMagnitude < 2 * 2;

        if (Time.time - lastTime > 1.5f || reachedDestination)
        {
            lastTime = Time.time;
            dest = enemy.GetRandomReachablePointOnNavMesh(enemy.enemyConfig.wanderRadius);
        }

        enemy.GoToDestination(dest, enemy.enemyConfig.wanderSpeed);
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }

}
