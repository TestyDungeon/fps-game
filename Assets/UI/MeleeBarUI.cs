using UnityEngine.UI;
using UnityEngine;

public class MeleeBarUI : MonoBehaviour
{
    private Kick melee;
    private IAmmoHandler ammoHandler;
    [SerializeField] private Image fill;

    void Awake()
    {
        if (melee == null)
            melee = FindAnyObjectByType<Kick>();

        melee.OnAmmoChanged += OnAmmoChanged;
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
