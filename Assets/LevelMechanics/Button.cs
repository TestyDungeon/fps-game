using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    public List<Door> doors;

    public void Interact()  
    {
        SoundManager.PlaySound(SoundType.SHIELD_BLOCK, 0.2f);
        foreach(Door door in doors)
        {
            door.Open(true);
        }
    }
}
