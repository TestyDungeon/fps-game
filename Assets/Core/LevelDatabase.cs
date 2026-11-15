using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public LevelDefinition[] levels;   // array order = order in level select
    public bool unlockAll;
    public LevelDefinition FindBySceneName(string name) =>
        System.Array.Find(levels, l => l.sceneName == name);

    public LevelDefinition Next(LevelDefinition current)
    {
        int i = System.Array.IndexOf(levels, current);
        return (i >= 0 && i + 1 < levels.Length) ? levels[i + 1] : null;
    }
}