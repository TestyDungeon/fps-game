using TMPro;
using UnityEngine;

public class AmmoIndicatorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Inventory inventory;
    private IAmmoHandler ammoHandler;

    void Awake()
    {
        Player.OnPlayerSpawned += HandlePlayerSpawned;

        if (Player.Instance != null)
        {
            HandlePlayerSpawned();
        }
    }

    void OnDestroy()
    {
        Player.OnPlayerSpawned -= HandlePlayerSpawned;

        if (inventory != null)
            inventory.OnSlotChanged -= OnSlotChanged;

        if (ammoHandler != null)
            ammoHandler.OnAmmoChanged -= OnAmmoChanged;
    }

    private void HandlePlayerSpawned()
    {
        if (inventory != null)
            inventory.OnSlotChanged -= OnSlotChanged;

        if (ammoHandler != null)
            ammoHandler.OnAmmoChanged -= OnAmmoChanged;
        ammoHandler = null;
        text.SetText("");

        inventory = Player.Instance.Inventory;

        if (inventory != null)
        {
            inventory.OnSlotChanged += OnSlotChanged;
            OnSlotChanged(inventory.GetCurrent());
        }
    }

    private void OnSlotChanged(Item item)
    {
        if (ammoHandler != null)
            ammoHandler.OnAmmoChanged -= OnAmmoChanged;

        if (item is IAmmoHandler handler)
        {
            ammoHandler = handler;
            ammoHandler.OnAmmoChanged += OnAmmoChanged;
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
        text.SetText(ammo.ToString());
    }
}