using System.Collections;
using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    private BoxCollider boxCollider;
    private bool triggered = false;
    public Door[] doorsToUnlockOnEnd;
    public Door[] doorsToCloseOnStart;
    public AudioClip soundOnEnter;
    public bool killOnEnter = false;
    public float volume = 1;

    [System.Serializable]
    public class Encounter
    {
        public GameObject[] enemies;
    }

    public Encounter[] encounters;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
    }
    void Start()
    {
        foreach(Encounter enc in encounters)
        {
            foreach(GameObject enemy in enc.enemies)
            {
                enemy.SetActive(false);
            }
        }
    }
    void Update()
    {
        if (!triggered && boxCollider.ClosestPoint(PlayerHitResponder.Instance.transform.position) == PlayerHitResponder.Instance.transform.position)
        {
            triggered = true;
            OnEnter();
        }
    }

    private void OnEnter()
    {
        StartCoroutine(StartEncounter());
        PlaySoundOnEnter();
        if(killOnEnter)
            PlayerHitResponder.Instance.TakeDamage(transform, 10000);
    }

    private void PlaySoundOnEnter()
    {
        
        SoundManager.PlaySound(soundOnEnter, volume);
        
    }

    private IEnumerator StartEncounter()
    {
        int encInd = 0;
        foreach(Door door in doorsToCloseOnStart)
        {
            door.locked = true;
            door.Open(false);
        }
        while(encInd < encounters.Length)
        {
            foreach(GameObject enemy in encounters[encInd].enemies)
            {
                Instantiate(GameManager.Instance.enemySpawnParticles, enemy.transform);
                enemy.SetActive(true);
                EnemyStateManager state = enemy.GetComponent<EnemyStateManager>();
                state.SwitchState(state.ChaseState);
                
                SoundManager.PlaySound(SoundType.ENEMY_SPAWN, enemy.transform.position, 0.6f, 0.6f);
                yield return new WaitForSeconds(0.25f);
            }
            bool allDead = false;
            while (!allDead)
            {
                allDead = true;
                foreach(GameObject enemy in encounters[encInd].enemies)
                {
                    if(enemy == null)
                        continue;
                    EnemyStateManager esm = enemy.GetComponent<EnemyStateManager>();
                    if(esm.GetCurrentState() != esm.DeadState)
                    {
                        allDead = false;
                        break;
                    }

                }
                yield return new WaitForSeconds(0.25f);
            }
            encInd++;
        }
        foreach(Door door in doorsToUnlockOnEnd)
        {
            
            door.locked = false;
        }
    }
}
