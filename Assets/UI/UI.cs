using UnityEngine;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }
    public GameObject mainMenuUI;

    public GameObject gameOverUI;
    public GameObject levelEndUI;
    public Camera UICamera;
    public GameObject HUD;
    public TextUI textUI;
    public PromptUI promptUI;
    public UISway UISway { get; private set; }

    void Awake()
    {
        Instance = this;
        UISway = GetComponent<UISway>();
    }

    void Start()
    {
        var flow = GameFlow.Instance;
        flow.StateChanged += OnStateChanged;
        flow.LevelStarted += OnLevelStarted;
        OnStateChanged(flow.State);            // catch up if the state was set before we subscribed
    }

    void OnDestroy()
    {
        if (GameFlow.Instance == null) return;
        GameFlow.Instance.StateChanged -= OnStateChanged;
        GameFlow.Instance.LevelStarted -= OnLevelStarted;
    }

    void OnStateChanged(GameState s)
    {
        bool inLevel = s == GameState.Playing || s == GameState.Paused
                || s == GameState.GameOver || s == GameState.LevelComplete;

        //titleUI.SetActive(s == GameState.Title);
        mainMenuUI.gameObject.SetActive(s == GameState.Paused || s == GameState.MainMenu);
        HUD.SetActive(inLevel);
        gameOverUI.SetActive(s == GameState.GameOver);
        levelEndUI.SetActive(s == GameState.LevelComplete);
    }

    void OnLevelStarted(LevelDefinition level)
    {
        textUI.SetText(level.startText);
        // reset anything a previous level changed (see below)
    }

    //public void EnableUI() { foreach (var ui in UIs) ui.SetActive(true); }
}