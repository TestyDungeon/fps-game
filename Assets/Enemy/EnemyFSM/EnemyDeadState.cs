using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM A DEAD ENEMY");
        if(enemy.animator != null)
            enemy.animator.enabled = false;
        enemy.SetRagdollColliders(true);
        enemy.SetRagdollRigidBody(true);
        enemy.audioSource.enabled = false;
        //enemy.enemyAttack.StopAttack();
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        //enemy.movementController.Move(enemy.enemyVelocity); 
    }

    public override void OnCollisionEnter(EnemyStateManager enemy)
    {
        
    }
}
