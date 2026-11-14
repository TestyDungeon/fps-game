using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    public List<Door> doors;

    public void Interact()  
    {
        
        foreach(Door door in doors)
        {
            door.Unlock();
            door.Open();
        }
    }
}
