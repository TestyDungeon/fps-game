using UnityEngine;

public class MainScreen : MenuScreen
{
    //[SerializeField] GameObject resumeButton;
    //[SerializeField] GameObject quitToMenuButton;

    void OnEnable()
    {
        //bool paused = GameFlow.Instance != null && GameFlow.Instance.State == GameState.Paused;
        //resumeButton.SetActive(paused);
        //quitToMenuButton.SetActive(paused);
    }
}