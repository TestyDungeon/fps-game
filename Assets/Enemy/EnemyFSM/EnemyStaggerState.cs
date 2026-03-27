using UnityEngine;

public class EnemyStaggerState : EnemyBaseState
{
    Material outline;
    
    public override void EnterState(EnemyStateManager enemy)
    {
        Debug.Log("IM A FALTERED ENEMY");
        enemy.StopAllCoroutines();
        enemy.StartCoroutine(enemy.SwitchState(enemy.ChaseState, 5, 5));
        
        outline = enemy.smrs[0].materials[1]; 
        foreach (SkinnedMeshRenderer smr in enemy.smrs)
        {
            Material[] mats = smr.materials;
            mats[1] = enemy.enemyConfig.staggerMaterial;
            smr.materials = mats;
        }
        
        //Object.Destroy(Object.Instantiate(enemy.enemyConfig.falterParticlesPrefab, enemy.transform.position + enemy.transform.up * (enemy.height / 2), Quaternion.identity, enemy.transform), 5);
    }

    public override void FixedUpdateState(EnemyStateManager enemy)
    {
        enemy.animancer.Play(enemy.enemyConfig.idleAnimation);
    }

    public override void ExitState(EnemyStateManager enemy)
    {
        foreach (SkinnedMeshRenderer smr in enemy.smrs)
        {
            Material[] mats = smr.materials;
            mats[1] = outline;
            smr.materials = mats;
        }
    }
}
