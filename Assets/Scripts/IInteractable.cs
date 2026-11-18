public readonly struct InteractResult
{
    public readonly bool success;
    public readonly string message;    // null or empty = show nothing

    InteractResult(bool success, string message) { this.success = success; this.message = message; }

    public static InteractResult Ok(string message = null) => new InteractResult(true, message);
    public static InteractResult Fail(string message) => new InteractResult(false, message);
}

public interface IInteractable
{
    string GetPrompt(Interactor who);          // shown while looking at it; null hides the prompt
    InteractResult Interact(Interactor who);
}