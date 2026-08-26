using UnityEngine.UI;
using UnityEngine;

public class DashBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;

    private PlayerMovement pm;

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

        if (pm != null)
            pm.OnDash -= OnDash;
    }

    private void HandlePlayerSpawned()
    {
        if (pm != null)
            pm.OnDash -= OnDash;

        pm = Player.Instance.Movement;

        if (pm != null)
        {
            pm.OnDash += OnDash;
        }
    }

    private void OnDash(int maxDashAmount, int currentDashAmount, float dashCooldown)
    {
        fill.fillAmount = (float)currentDashAmount / (float)maxDashAmount;
    }
}