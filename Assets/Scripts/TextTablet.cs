using UnityEngine;

public class TextTablet : MonoBehaviour, IInteractable
{
    [SerializeField] private string text;
    [SerializeField] private float fontSize = 26;
    public string GetPrompt(Interactor who) => "Read";

    public InteractResult Interact(Interactor who)
    {
        UI.Instance.textUI.SetSize(fontSize);
        UI.Instance.textUI.SetText(text);
        return InteractResult.Ok();
    }
}
