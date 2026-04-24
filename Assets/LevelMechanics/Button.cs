using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    public GameObject receiver;
    public IInteractable receiverInterface;
    void Awake()
    {
        receiverInterface = receiver.GetComponent<IInteractable>();
    }

    public void Interact()  
    {
        //SoundManager.PlaySound(SoundType.PICKUP_KEY, 1);
        receiverInterface.Interact();
    }
}
