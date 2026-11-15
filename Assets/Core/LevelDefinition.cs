using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Definition")]
public class LevelDefinition : ScriptableObject
{
    [Header("Menu")]
    public string displayName;        // shown in level select
    public Sprite thumbnail;

    [Header("Scene")]
    public string sceneName;          // must match the scene file name

    [Header("Presentation")]
    [TextArea] public string startText;   // replaces mapStartText in GameManager
    //public AudioClip[] musicTracks;       // passed to MusicManager.SetPlaylist
    //public bool startWithHUD = true;      // only if some levels really start with the HUD hidden

#if UNITY_EDITOR
    [SerializeField] UnityEditor.SceneAsset sceneAsset;   // drag the scene here
    void OnValidate() { if (sceneAsset) sceneName = sceneAsset.name; }
#endif
}