using UnityEngine;

public class LevelSelectScreen : MenuScreen
{
    [SerializeField] Transform listRoot;
    [SerializeField] LevelButton buttonPrefab;
    [SerializeField] bool showLocked = true;

    void OnEnable()
    {
        foreach (Transform child in listRoot) Destroy(child.gameObject);
        if (GameFlow.Instance == null) return;

        var db = GameFlow.Instance.Database;
        foreach (var level in db.levels)
        {
            bool unlocked = Progress.IsUnlocked(db, level);
            if (!unlocked && !showLocked) continue;
            Instantiate(buttonPrefab, listRoot).Setup(level, unlocked);
        }
    }
}