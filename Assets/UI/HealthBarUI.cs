using UnityEngine.UI;
using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    private Health playerHealth;
    [SerializeField] private Image fill;

    void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHitResponder>().gameObject.GetComponent<Health>();

    }

    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += OnHealthChanged;
        }
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float currentHealth, float maxHealth)
    {
        fill.fillAmount = currentHealth / maxHealth;
    }
}
