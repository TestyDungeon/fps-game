using UnityEngine;

public class TextTablet : MonoBehaviour, IInteractable
{
    [SerializeField] private string text;
    [SerializeField] private float fontSize = 26;
    public void Interact()
    {
        UI.Instance.textUI.SetSize(fontSize);
        UI.Instance.textUI.SetText(text);
    }
}
