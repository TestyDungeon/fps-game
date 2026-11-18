using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] float range = 2.7f;
    static readonly int Mask = (1 << 0) | (1 << 14);   // world (Default) + interactables

    Transform origin;
    string shownPrompt;

    //public Inventory Inventory { get; private set; }

    void Start()
    {
        origin = GetComponentInChildren<Camera>().transform;
        //Inventory = GetComponentInParent<Inventory>();
    }

    void Update()
    {
        if (UI.Instance == null) return;                   // Core may load after the level (editor path)

        IInteractable target = GameFlow.IsPlaying ? FindTarget() : null;
        SetPrompt(target?.GetPrompt(this));

        if (target == null || !Input.GetKeyDown(KeyCode.E)) return;

        InteractResult result = target.Interact(this);
        SoundManager.PlaySound(SoundType.SHIELD_BLOCK, result.success ? 0.4f : 0.2f);   // swap in a real fail sound later
        if (!string.IsNullOrEmpty(result.message))
            UI.Instance.promptUI.ShowMessage(result.message);
    }

    IInteractable FindTarget()
    {
        if (!Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, range, Mask, QueryTriggerInteraction.Collide))
            return null;
        return hit.transform.GetComponentInParent<IInteractable>();
    }

    void SetPrompt(string text)
    {
        if (text == shownPrompt) return;                   // don't touch the UI every frame
        shownPrompt = text;
        UI.Instance.promptUI.SetPrompt(text);
    }
}