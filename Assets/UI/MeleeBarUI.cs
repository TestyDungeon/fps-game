using UnityEngine.UI;
using UnityEngine;

public class MeleeBarUI : MonoBehaviour
{
    private Shield shield;
    private IAmmoHandler ammoHandler;
    [SerializeField] private Image fill;

    void Awake()
    {
        if (shield == null)
            shield = FindAnyObjectByType<Shield>();

        shield.OnAmmoChanged += OnAmmoChanged;
    }


    void OnDisable()
    {
        if (ammoHandler != null)
            ammoHandler.OnAmmoChanged -= OnAmmoChanged;
    }

    private void OnAmmoChanged(int maxAmmo, int ammo)
    {
        fill.fillAmount = (float)ammo / (float)maxAmmo;
    }
}
