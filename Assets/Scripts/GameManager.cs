using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private bool inMenu = false;
    private bool isGameOver = false;
    
    public GameObject grapple;
    [SerializeField] private GameObject[] itemsPos;
    [SerializeField] private string mapStartText;
    public GameObject decalParticles;
    public GameObject enemySpawnParticles;
    public GameObject bloodParticles;
    public Material whiteMaterial;
    [Header("Drops")]
    public GameObject healthOrb;
    public GameObject ammoOrb;

    [HideInInspector] public bool inUI = false;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        UI.Instance.textUI.SetText(mapStartText);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetMainMenuOpen(!inMenu);
        }
    }

   public void SetMainMenuOpen(bool open)
    {
        inMenu = open;
        UI.Instance.mainMenuUI.gameObject.SetActive(open);
        Time.timeScale = open ? 0f : 1f;

        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.Confined : CursorLockMode.Locked;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0;
        UI.Instance.gameOverUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LevelEnd()
    {
        isGameOver = true;
        Time.timeScale = 0;
        UI.Instance.levelEndUI.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
        SetMainMenuOpen(false);
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
        PlayerMovement.Instance.GetComponent<Inventory>().EnableItem(name);
    }

    public void EnableUI()
    {
        foreach(GameObject item in UI.Instance.UIs)
        {
            item.SetActive(true);  
        }
    }

    public void PlayUIButtonSound(float volume)
    {
        SoundManager.PlaySound(SoundType.UI_BUTTON, volume);
    }

    public bool GetIsGameOver()
    {
        return isGameOver;
    }

    public bool GetInMenu()
    {
        return inMenu;
    }
}
