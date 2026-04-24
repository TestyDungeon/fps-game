using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private GameObject grapple;
    [SerializeField] private GameObject[] itemsPos;
    [SerializeField] private GameObject[] UI;
    public GameObject decalParticles;
    public GameObject enemySpawnParticles;
    public GameObject bloodParticles;
    [Header("Drops")]
    public GameObject healthOrb;
    public GameObject ammoOrb;

    [HideInInspector] public bool inUI = false;

    void Awake()
    {
        if(Instance == null)
            Instance = this;    
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !inUI)
        {
            inUI = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            upgradeUI.SetActive(true);
        }
        else if(Input.GetKeyDown(KeyCode.I) && inUI)
        {
            inUI = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            upgradeUI.SetActive(false);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        gameOverUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void EnableGrapple()
    {
        grapple.SetActive(true);
    }

    public void EnableItems()
    {
        foreach(GameObject item in itemsPos)
        {
            item.SetActive(true);  
        }
    }

    public void EnableItem(string name)
    {
        PlayerHitResponder.Instance.GetComponent<Inventory>().EnableItem(name);
    }

    public void EnableUI()
    {
        foreach(GameObject item in UI)
        {
            item.SetActive(true);  
        }
    }

    public void PlayUIButtonSound(float volume)
    {
        SoundManager.PlaySound(SoundType.UI_BUTTON, volume);
    }
}
