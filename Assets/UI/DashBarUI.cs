using UnityEngine.UI;
using UnityEngine;

public class DashBarUI : MonoBehaviour
{
    private PlayerMovement pm;
    [SerializeField] private Image fill;

    void Awake()
    {
        if (pm == null)
            pm = FindAnyObjectByType<PlayerMovement>();

    }

    void OnEnable()
    {
        if (pm != null)
        {
            pm.OnDash += OnDash;
        }
    }

    void OnDisable()
    {
        if (pm != null)
            pm.OnDash -= OnDash;
    }

    private void OnDash(int maxDashAmount, int currentDashAmount, float dashCooldown)
    {
        fill.fillAmount = (float)currentDashAmount / (float)maxDashAmount;
    }
}
