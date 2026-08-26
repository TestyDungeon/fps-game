using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager enemy)
    {
        enemy.animator.Play("Death");
        Debug.Log("IM A DEAD ENEMY");
        enemy.StartCoroutine(enemy.ehr.SetHurtBoxToDead());
        SoundManager.PlaySound(enemy.enemyConfig.deathSFX, enemy.transform.position, 0.6f, 0.5f);
        //if(enemy.animator != null)
        //    enemy.animator.enabled = false;
        enemy.gameObject.layer = LayerMask.NameToLayer("Corpse");
        enemy.audioSource.enabled = false;
        enemy.SetNavmeshAgent(false);
        GameObject orb = Object.Instantiate(GameManager.Instance.healthOrb, enemy.transform.position, Quaternion.identity);
        orb.GetComponent<Rigidbody>().AddForce(Vector3.up * 1, ForceMode.Force);
        Object.Destroy(orb, 8);
        //GameObject x = Object.Instantiate(enemy.enemyConfig.deathParticle, enemy.transform.position, Quaternion.LookRotation(enemy.transform.forward, enemy.transform.up));
        //Object.Destroy(x, 100);
        //enemy.movementController.addVelocity(enemy.lastDamageVector * 0.3f);
        enemy.movementController.SetEnemyLayerMaskToDead();

        //if(enemy.rigidbodies.Length > 0)
        //{
        //    enemy.SetRagdollColliders(true);
        //    enemy.SetRagdollRigidBody(true);
        //    enemy.ForceRagdollRigidBody(enemy.lastDamageVector);
        //}
        //else
        //{
            
            //Object.Destroy(enemy.gameObject);
        //}

        
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        
    }
}
