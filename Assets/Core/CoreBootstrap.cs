using UnityEngine;
using UnityEngine.SceneManagement;

public static class CoreBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureCore()
    {
        if (!SceneManager.GetSceneByName("Core").isLoaded)
            SceneManager.LoadScene("Core", LoadSceneMode.Additive);
    }
}