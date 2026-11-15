using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenuNoUI, MainMenu, Loading, Playing, Paused, GameOver, LevelComplete }

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }
    public GameState State { get; private set; } = GameState.MainMenu;
    public static bool IsPlaying => Instance != null && Instance.State == GameState.Playing;
    public LevelDefinition CurrentLevel { get; private set; }
    [SerializeField] LevelDatabase database;
    public LevelDatabase Database => database;
    public event Action<GameState> StateChanged;
    public event Action<LevelDefinition> LevelStarted;

    [SerializeField] string menuSceneName = "MainMenu";
    [SerializeField] GameObject fallbackCamera;   // Core camera, no AudioListener, only on while Loading
    Scene menuScene;

    Scene levelScene;

    void Awake() => Instance = this;

    void Start()
    {
        
        // Editor shortcut: Play was pressed inside a level, so Core was added after it.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s == gameObject.scene) continue;
            if (s.name == menuSceneName) { menuScene = s; continue; }

            levelScene = s;
            CurrentLevel = database != null ? database.FindBySceneName(s.name) : null;
            if (CurrentLevel != null) LevelStarted?.Invoke(CurrentLevel);
            SetState(GameState.Playing);
            return;
        }
        StartCoroutine(StartMenuRoutine());               // normal start: Core only
    }

    public void LoadLevel(LevelDefinition level) => StartCoroutine(LoadRoutine(level));
    public void Restart() => LoadLevel(CurrentLevel);
    public void ReturnToMainMenu() => StartCoroutine(ReturnRoutine());

    public void TogglePause()
    {
        if (State == GameState.Playing) SetState(GameState.Paused);
        else if (State == GameState.Paused) SetState(GameState.Playing);
    }
    public void GameOver() { if (State == GameState.Playing) SetState(GameState.GameOver); }
    public void LevelEnd() { if (State == GameState.Playing) SetState(GameState.LevelComplete); }

    IEnumerator UnloadLevel()
    {
        if (levelScene.IsValid())
            yield return SceneManager.UnloadSceneAsync(levelScene);
        levelScene = default;
    }

    IEnumerator LoadRoutine(LevelDefinition level)
    {
        SetState(GameState.Loading);
        yield return HideMenuScene();
        yield return UnloadLevel();
        yield return SceneManager.LoadSceneAsync(level.sceneName, LoadSceneMode.Additive);
        levelScene = SceneManager.GetSceneByName(level.sceneName);
        SceneManager.SetActiveScene(levelScene);   // player, decals, temp audio spawn here and die with the level
        CurrentLevel = level;
        //SpawnPlayerAtStart();
        LevelStarted?.Invoke(level);               // UI shows start text, MusicManager swaps playlist
        SetState(GameState.Playing);
    }
    

    IEnumerator ReturnRoutine()
    {
        SetState(GameState.Loading);
        yield return UnloadLevel();
        CurrentLevel = null;
        yield return ShowMenuScene();
        SetState(GameState.MainMenu);
    }

    void SetState(GameState s)
    {
        if (fallbackCamera) fallbackCamera.SetActive(s == GameState.Loading);
        State = s;
        bool playing = s == GameState.Playing;
        bool frozen = s == GameState.Paused || s == GameState.GameOver || s == GameState.LevelComplete;
        Time.timeScale = frozen ? 0f : 1f;
        Cursor.lockState = playing ? CursorLockMode.Locked : CursorLockMode.Confined;
        Cursor.visible = !playing;
        StateChanged?.Invoke(s);
        Debug.Log("State: " + Instance.State);
    }

    IEnumerator ShowMenuScene()
    {
        if (menuScene.IsValid()) yield break;
        yield return SceneManager.LoadSceneAsync(menuSceneName, LoadSceneMode.Additive);
        menuScene = SceneManager.GetSceneByName(menuSceneName);
        SceneManager.SetActiveScene(menuScene);
    }

    IEnumerator HideMenuScene()
    {
        if (menuScene.IsValid())
            yield return SceneManager.UnloadSceneAsync(menuScene);
        menuScene = default;
    }

    IEnumerator StartMenuRoutine()
    {
        SetState(GameState.Loading);
        yield return ShowMenuScene();
        SetState(GameState.MainMenuNoUI);
    }

    public void ShowMainMenu()
    {
        if (State == GameState.MainMenuNoUI) StartCoroutine(RevealNextFrame());
    }

    IEnumerator RevealNextFrame()
    {
        yield return null;                  // see the note on mouse clicks below
        SetState(GameState.MainMenu);
    }
}