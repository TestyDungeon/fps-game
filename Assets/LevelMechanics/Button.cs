using UnityEngine;
using UnityEngine.Events;

public class InteractableEvent : MonoBehaviour, IInteractable
{
    [SerializeField] string prompt = "Press";
    [SerializeField] string message;           // shown after pressing; leave empty for none
    [SerializeField] bool singleUse = true;
    [SerializeField] UnityEvent onInteract;

    bool used;

    public string GetPrompt(Interactor who) => used ? null : prompt;

    public InteractResult Interact(Interactor who)
    {
        if (used) return InteractResult.Fail(null);
        if (singleUse) used = true;
        onInteract.Invoke();
        return InteractResult.Ok(message);
    }
}