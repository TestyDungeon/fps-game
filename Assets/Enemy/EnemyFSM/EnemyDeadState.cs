using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM A DEAD ENEMY");
        if(enemy.animator != null)
            enemy.animator.enabled = false;

        enemy.audioSource.enabled = false;
        enemy.SetNavmeshAgent(false);
        GameObject x = Object.Instantiate(enemy.enemyConfig.deathParticle, enemy.transform.position, Quaternion.identity);
        GameObject orb = Object.Instantiate(GameManager.Instance.healthOrb, enemy.transform.position, Quaternion.identity);
        orb.GetComponent<Rigidbody>().AddForce(Vector3.up * 1, ForceMode.Force);
        Object.Destroy(orb, 8);
        Object.Destroy(x, 100);
        SoundManager.PlaySound(enemy.enemyConfig.deathSFX, enemy.transform.position, 0.5f, 0.5f);

        if(enemy.rigidbodies.Length > 0)
        {
            enemy.SetRagdollColliders(true);
            enemy.SetRagdollRigidBody(true);
            enemy.ForceRagdollRigidBody(enemy.lastDamageVector);
        }
        else
        {
            Object.Destroy(enemy.gameObject);
        }

        
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }
}
