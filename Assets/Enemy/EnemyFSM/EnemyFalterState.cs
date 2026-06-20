using UnityEngine;

public class EnemyFalterState : EnemyBaseState
{
    private Coroutine switchCoroutine;
    GameObject particles;

    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM A FALTERED ENEMY");
        switchCoroutine = null;

        //if(enemy.animator != null)
        //    enemy.animator.CrossFade("Air", 1, 0, 0);
        //enemy.audioSource.enabled = false;
        //GameObject.Destroy(enemy.gameObject, 1);
        //enemy.enemyAttack.StopAttack();
        //enemy.SetNavmeshAgent(false);
        particles = Object.Instantiate(enemy.enemyConfig.falterParticlesPrefab, enemy.transform.position + enemy.transform.up * (enemy.height / 2), Quaternion.LookRotation(enemy.transform.forward, enemy.transform.up), enemy.transform);
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        if(enemy.animator != null)
            enemy.animancer.Play(enemy.enemyConfig.idleAnimation);

        if (switchCoroutine != null) return;

        //if ( && firstGroundHit)
        if(enemy.movementController.GetIsDashing() == false && enemy.movementController.GroundCheck())
        {
            switchCoroutine = enemy.StartCoroutine(enemy.SwitchState(enemy.ChaseState, enemy.enemyConfig.falterDuration, enemy.enemyConfig.falterDuration));
        }
        //enemy.movementController.Move(enemy.enemyVelocity); 
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        if (switchCoroutine != null)
        {
            enemy.StopCoroutine(switchCoroutine);
            switchCoroutine = null;
        }
        Object.Destroy(particles);
    }
}
