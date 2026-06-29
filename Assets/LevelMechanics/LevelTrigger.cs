using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelTrigger : MonoBehaviour
{
    private BoxCollider boxCollider;
    private bool triggered = false;
    public Door[] doorsToUnlockOnEnd;
    public Door[] doorsToCloseOnStart;
    public AudioClip soundOnEnter = null;
    public float volume = 1;
    public bool killOnEnter = false;
    public UnityEvent onEnter;
    public UnityEvent onExit;
    [SerializeField] private string sceneToLoad = null;

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
        else if(triggered && boxCollider.ClosestPoint(PlayerHitResponder.Instance.transform.position) != PlayerHitResponder.Instance.transform.position)
        {
            triggered = false;
            OnExit();
        }
    }

    private void OnEnter()
    {
        if(sceneToLoad != null)
            SceneManager.LoadScene(sceneToLoad);
        onEnter?.Invoke();
        StartCoroutine(StartEncounter());
        PlaySoundOnEnter();
        if(killOnEnter)
            PlayerHitResponder.Instance.TakeDamage(transform, 10000);
    }

    private void OnExit()
    {
        onExit?.Invoke();
    }

    private void PlaySoundOnEnter()
    {
        if(soundOnEnter != null)
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
