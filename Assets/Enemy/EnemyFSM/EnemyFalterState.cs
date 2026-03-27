using UnityEngine;

public class EnemyFalterState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM A FALTERED ENEMY");

        //if(enemy.animator != null)
        //    enemy.animator.CrossFade("Air", 1, 0, 0);
        //enemy.audioSource.enabled = false;
        //GameObject.Destroy(enemy.gameObject, 1);
        //enemy.enemyAttack.StopAttack();
        //enemy.SetNavmeshAgent(false);
        enemy.StartCoroutine(enemy.SwitchState(enemy.ChaseState, enemy.enemyConfig.falterDuration, enemy.enemyConfig.falterDuration));
        Object.Destroy(Object.Instantiate(enemy.enemyConfig.falterParticlesPrefab, enemy.transform.position + enemy.transform.up * (enemy.height / 2), Quaternion.identity, enemy.transform), enemy.enemyConfig.falterDuration);
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        if(enemy.animator != null)
            enemy.animancer.Play(enemy.enemyConfig.idleAnimation);
        //enemy.movementController.Move(enemy.enemyVelocity); 
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }
}
