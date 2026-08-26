using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class HealthBarUI : MonoBehaviour
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
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void HandlePlayerSpawned()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
        }

        playerHealth = Player.Instance.Health;

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        fill.fillAmount = currentHealth / maxHealth;
        text.SetText(currentHealth.ToString());
    }
}