using UnityEngine;

public class EnemyWanderState : EnemyBaseState
{
    Vector3 dest;
    bool hasDest;
    float giveUpTime;
    Coroutine switchRoutine;

    public override void EnterState(EnemyStateManager enemy)
    {
        enemy.SetNavmeshAgent(true);
        hasDest = false; // clear leftovers from the previous time we were in this state

        switchRoutine = enemy.StartCoroutine(enemy.SwitchState(
            enemy.ChaseState,
            enemy.enemyConfig.wanderDuration * 1.2f,
            enemy.enemyConfig.wanderDuration * 0.8f));
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        enemy.RotateInDirection();

        Vector3 toDest = dest - enemy.transform.position;
        toDest.y = 0f; // ignore height so a raised pivot doesn't affect the check
        bool arrived = toDest.sqrMagnitude < 2f * 2f;

        if (!hasDest || arrived || Time.time > giveUpTime)
        {
            hasDest = enemy.TryGetRandomReachablePoint(
                enemy.enemyConfig.wanderDuration,
                enemy.enemyConfig.wanderRadius,
                out dest);

            // Stuck protection. Tune to roughly (wanderRadius / wanderSpeed) * 2
            giveUpTime = Time.time + 8f;
        }

        if (hasDest)
            enemy.GoToDestination(dest, enemy.enemyConfig.wanderSpeed);
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        if (switchRoutine != null)
            enemy.StopCoroutine(switchRoutine);
    }
}