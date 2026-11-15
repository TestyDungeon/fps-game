using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject menuRoot;     // CanvasMainMenu
    [SerializeField] MenuScreen mainScreen;

    MenuScreen[] allScreens;
    readonly Stack<MenuScreen> stack = new Stack<MenuScreen>();

    void Awake() => allScreens = menuRoot.GetComponentsInChildren<MenuScreen>(true);

    void Start()
    {
        GameFlow.Instance.StateChanged += OnStateChanged;
        OnStateChanged(GameFlow.Instance.State);   // catch up
    }

    void OnDestroy()
    {
        if (GameFlow.Instance != null) GameFlow.Instance.StateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameState s)
    {
        if (s == GameState.Paused || s == GameState.MainMenu) ResetToMain();
    }

    void ResetToMain()
    {
        foreach (var screen in allScreens) screen.gameObject.SetActive(false);
        stack.Clear();
        Push(mainScreen);
    }

    public void Push(MenuScreen screen)
    {
        if (stack.Count > 0) stack.Peek().gameObject.SetActive(false);
        stack.Push(screen);
        screen.gameObject.SetActive(true);
    }

    public void Pop()
    {
        if (stack.Count <= 1) return;
        stack.Pop().gameObject.SetActive(false);
        stack.Peek().gameObject.SetActive(true);
    }

    void Update()
    {
        var flow = GameFlow.Instance;
        if (flow.State == GameState.MainMenuNoUI && Input.anyKeyDown) { flow.ShowMainMenu(); return; }

        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        switch (flow.State)
        {
            case GameState.Playing:
                flow.TogglePause();
                break;
            case GameState.Paused:
            case GameState.MainMenu:
                if (stack.Count > 1) Pop();
                else if (flow.State == GameState.Paused) flow.TogglePause();   // resume
                break;
        }
    }
}