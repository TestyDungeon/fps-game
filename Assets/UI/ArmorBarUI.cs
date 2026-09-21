using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ArmorBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private TextMeshProUGUI text;

    private Health playerHealth;

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

        if (playerHealth != null)
        {
            playerHealth.OnArmorChanged -= OnArmorChanged;
        }
    }

    private void HandlePlayerSpawned()
    {
        if (playerHealth != null)
        {
            playerHealth.OnArmorChanged -= OnArmorChanged;
        }

        playerHealth = Player.Instance.Health;

        if (playerHealth != null)
        {
            playerHealth.OnArmorChanged += OnArmorChanged;
        }
    }

    private void OnArmorChanged(float currentHealth, float maxHealth)
    {
        Debug.Log("Armor");
        fill.fillAmount = currentHealth / maxHealth;
        text.SetText(currentHealth.ToString());
    }
}