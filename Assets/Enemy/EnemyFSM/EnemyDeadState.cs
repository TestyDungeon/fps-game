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
        enemy.ForceRagdollRigidBody(enemy.lastDamageVector);
        enemy.audioSource.enabled = false;
        //enemy.FlyInDirection(enemy.transform.up*10);
        enemy.SetNavmeshAgent(false);
        GameObject x = Object.Instantiate(enemy.enemyConfig.deathParticle, enemy.transform.position, Quaternion.identity);
        GameObject orb = Object.Instantiate(enemy.enemyConfig.healthOrb, enemy.transform.position, Quaternion.identity);
        orb.GetComponent<Rigidbody>().AddForce(Vector3.up * 1, ForceMode.Force);
        Object.Destroy(orb, 8);
        //Time.timeScale += 0.1f;
        Object.Destroy(x, 100);
        SoundManager.PlaySound(enemy.enemyConfig.deathSFX, enemy.transform.position, 0.5f, 0.5f);
        //GameObject.Destroy(enemy.gameObject, 1);
        //enemy.enemyAttack.StopAttack();
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        //enemy.movementController.Move(enemy.enemyVelocity); 
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }
}
