using UnityEngine;

public class EnemyAirFalterState : EnemyBaseState
{
    private float startTime;
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM A FALTERED ENEMY");
        startTime = Time.time;
        //if(enemy.animator != null)
        //    enemy.animator.CrossFade("Air", 1, 0, 0);
        //enemy.audioSource.enabled = false;
        //GameObject.Destroy(enemy.gameObject, 1);
        //enemy.enemyAttack.StopAttack();
        enemy.SetNavmeshAgent(false);
        enemy.AirJuggle(10);
        Object.Destroy(Object.Instantiate(enemy.enemyConfig.falterParticlesPrefab, enemy.transform.position + enemy.transform.up * (enemy.height / 2), Quaternion.identity, enemy.transform), enemy.enemyConfig.falterDuration);
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        if(enemy.animancer != null)
            enemy.animancer.Play(enemy.enemyConfig.idleAnimation);
        //enemy.movementController.Move(enemy.enemyVelocity); 
        if((Time.time - startTime) > 0.1f && enemy.movementController.GroundCheck())
        {
            enemy.SwitchState(enemy.ChaseState);
        }
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }
}
