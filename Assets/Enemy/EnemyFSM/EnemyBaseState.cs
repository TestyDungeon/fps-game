using UnityEngine;

public abstract class EnemyBaseState
{
    public abstract void EnterState(EnemyStateManager enemy);
    
    public abstract void FixedUpdateState(EnemyStateManager enemy);
    
    public abstract void ExitState(EnemyStateManager enemy);
    
}
