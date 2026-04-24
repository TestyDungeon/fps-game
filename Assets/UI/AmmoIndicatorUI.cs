using TMPro;
using UnityEngine;

public class AmmoIndicatorUI : MonoBehaviour
{
    private Inventory inventory;
    private IAmmoHandler ammoHandler;
    [SerializeField] private TextMeshProUGUI text;

    void Awake()
    {
        if (inventory == null)
            inventory = FindAnyObjectByType<Inventory>();
    }

    //void Start()
    //{
    //    if(ammoHandler != null)
    //    {
    //        //ammoHandler = inventory.GetCurrent() as IAmmoHandler;
    //        ammoHandler.OnAmmoChanged += OnAmmoChanged;
    //        OnAmmoChanged(ammoHandler.GetAmmo());
    //    }
    //}

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnSlotChanged += OnSlotChanged;
        }
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnSlotChanged -= OnSlotChanged;

        if (ammoHandler != null)
            ammoHandler.OnAmmoChanged -= OnAmmoChanged;
    }

    private void OnSlotChanged(Item item)
    {
        if (ammoHandler != null)
            ammoHandler.OnAmmoChanged -= OnAmmoChanged;

        if (item is IAmmoHandler handler)
        {
            ammoHandler = handler;
            ammoHandler.OnAmmoChanged += OnAmmoChanged;
            //Debug.Log("Slot Changed : " + ammoHandler.GetAmmo());
            text.SetText(ammoHandler.GetAmmo().ToString());
        }
        else
        {
            ammoHandler = null;
            text.SetText("");
        }
        
    }

    private void OnAmmoChanged(int maxAmmo, int ammo)
    {
        //Debug.Log("Ammo: " + ammo);
        text.SetText(ammo.ToString());
    }
}
