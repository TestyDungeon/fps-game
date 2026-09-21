using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }

    public MainMenuUI mainMenuUI;
    public GameObject gameOverUI;
    public GameObject levelEndUI;

    public Camera UICamera;
    
    public GameObject[] UIs;
    public TextUI textUI;

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
}
