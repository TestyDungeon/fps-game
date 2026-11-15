using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] Image thumbnail;   // optional
    [SerializeField] UnityEngine.UI.Button button;

    public void Setup(LevelDefinition level, bool unlocked)
    {
        label.text = level.displayName;
        if (thumbnail) thumbnail.sprite = level.thumbnail;
        button.interactable = unlocked;
        button.onClick.AddListener(() => GameFlow.Instance.LoadLevel(level));
    }
}