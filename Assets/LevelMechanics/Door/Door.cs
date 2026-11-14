using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    enum State { Closed, Opening, Open, Closing }

    [Header("Leaves")]
    [SerializeField] private DoorLeaf[] leaves;

    [Header("Motion")]
    [SerializeField] private float openTime = 0.3f;      // seconds to fully open
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float stayOpenTime = -1f;   // seconds before closing by itself; -1 = stays open. Ignored by automatic doors.

    [Header("Lock")]
    public bool locked;
    [SerializeField] private Pickup key;                 // door stays locked until this pickup is collected (destroyed)
    [SerializeField] private bool closable = false;

    [Header("Automatic")]
    [SerializeField] private bool automatic;
    [SerializeField] private float activationRange = 4f; // measured from this object's position, so put the Door root at the doorway

    private State state = State.Closed;
    private float progress;   // 0 = closed, 1 = open (linear; the ease curve is applied on top)
    private float timer;
    private bool usesKey;

    void Awake()
    {
        if (leaves == null || leaves.Length == 0)
            Debug.LogError($"{nameof(Door)} on {name} has no leaves assigned.", this);

        usesKey = key != null;
        if (usesKey)
            locked = true;
    }

    void FixedUpdate()
    {
        if (automatic)
            UpdateAutomatic();

        switch (state)
        {
            case State.Opening:
                Step(1f);
                if (progress >= 1f)
                {
                    state = State.Open;
                    timer = stayOpenTime;
                }
                break;

            case State.Open:
                if (!automatic && stayOpenTime >= 0f)
                {
                    timer -= Time.fixedDeltaTime;
                    if (timer <= 0f)
                        state = State.Closing;
                }
                break;

            case State.Closing:
                Step(0f);
                if (progress <= 0f)
                    state = State.Closed;
                break;
        }
    }

    // Press-to-use. Automatic doors ignore the use button.
    public void Interact()
    {
        if (automatic) return;

        if (state == State.Closed || state == State.Closing)
        {
            if (!Open())
            {
                // TODO: play a "locked" sound or show a message here
            }
        }
        else
        {
            if(closable)
                Close();
        }
    }

    // Returns false only if the door is locked.
    public bool Open()
    {
        if (!CanOpen()) return false;
        if (state == State.Opening || state == State.Open) return true;

        if (state == State.Closed)
            SoundManager.PlaySound(SoundType.DOOR_OPEN, transform.position, 0.2f, 0.9f);

        state = State.Opening;
        return true;
    }

    public void Close()
    {
        if (state == State.Closed || state == State.Closing) return;
        state = State.Closing;
    }

    private bool CanOpen()
    {
        // Unity treats a destroyed object as null, so a collected key pickup reads as null here.
        if (usesKey && key == null)
            locked = false;

        return !locked;
    }

    public void Unlock()
    {
        locked = false;
    }

    private void UpdateAutomatic()
    {
        if (PlayerMovement.Instance == null) return;

        Vector3 offset = PlayerMovement.Instance.transform.position - transform.position;
        bool playerNear = offset.sqrMagnitude < activationRange * activationRange;

        if (playerNear)
            Open();    // does nothing if already opening/open, or still locked
        else
            Close();   // does nothing if already closing/closed
    }

    private void Step(float target)
    {
        progress = Mathf.MoveTowards(progress, target, Time.fixedDeltaTime / Mathf.Max(openTime, 0.01f));

        float eased = ease.Evaluate(progress);
        foreach (var leaf in leaves)
            leaf.SetProgress(eased);
    }

    void OnDrawGizmosSelected()
    {
        if (!automatic) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}