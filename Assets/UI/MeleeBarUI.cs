using UnityEngine.UI;
using UnityEngine;

public class MeleeBarUI : MonoBehaviour
{
    private Kick melee;
    private IAmmoHandler ammoHandler;
    private Inventory inventory;
    [SerializeField] private Image fill;

    void Awake()
    {
        //if (melee == null)
        //    melee = FindAnyObjectByType<Kick>();
//
        //melee.OnAmmoChanged += OnAmmoChanged;

        Player.OnPlayerSpawned += HandlePlayerSpawned;

        if (Player.Instance != null)
        {
            HandlePlayerSpawned();
        }
    }


    private void HandlePlayerSpawned()
    {
        if (ammoHandler != null)
            ammoHandler.OnAmmoChanged -= OnAmmoChanged;
        ammoHandler = null;

        inventory = Player.Instance.Inventory;

        if (inventory != null)
        {
            foreach(Item item in inventory.GetAlwaysOn())
            {
                if(item is Kick)
                {
                    ammoHandler = item as Kick;
                    ammoHandler.OnAmmoChanged += OnAmmoChanged;
                }
            }
        }
    }


    void OnDestroy()
    {
        Player.OnPlayerSpawned -= HandlePlayerSpawned;

        if (ammoHandler != null)
            ammoHandler.OnAmmoChanged -= OnAmmoChanged;
    }

    private void OnAmmoChanged(int maxAmmo, int ammo)
    {
        fill.fillAmount = (float)ammo / (float)maxAmmo;
    }
}
