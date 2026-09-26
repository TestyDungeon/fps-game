using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives under PersistentUI (DontDestroyOnLoad). Never holds a reference to the
// Player or to Sway — it just listens for whichever Sway instance is currently
// broadcasting. Its own uiTransforms/hudTransforms refs are safe to wire in the
// Inspector because they point at children of this same persistent hierarchy.
public class UISway : MonoBehaviour
{
    [SerializeField] private List<RectTransform> uiTransforms;
    [SerializeField] private RectTransform hudTransform;

    private Vector3 rot;
    private Vector3 hudOrigin;

    private Vector3 hudTargetPosition;

    void Awake()
    {
        hudOrigin = hudTransform.transform.localPosition;
        hudTargetPosition = hudOrigin;       
    }

    void OnEnable()
    {
        Sway.OnRotationChanged += HandleRotationChanged;
    }

    void OnDisable()
    {
        Sway.OnRotationChanged -= HandleRotationChanged;
    }

    private void HandleRotationChanged(Vector3 newRot)
    {
        rot = newRot;
    }

    void Update()
    {
        if (uiTransforms.Count > 0)
        {
            ApplyUIRotation();
        }

        if (hudTransform != null)
        {
            ApplyHUDRotation();
            ApplyHUDPosition();
            //if(Player.Instance.MovementController.isGrounded || Vector3.Distance(hudTransform.position, hudTargetPosition) < 2)
            //    hudTargetPosition = hudOrigin;
        }
    }

    private void ApplyUIRotation()
    {
        foreach (Transform uiTransform in uiTransforms)
        {
            uiTransform.localRotation = Quaternion.Lerp(uiTransform.localRotation, Quaternion.Euler(rot * 2.5f), Time.unscaledDeltaTime * 5);
        }
    }

    private void ApplyHUDRotation()
    {
        hudTransform.localRotation = Quaternion.Lerp(hudTransform.localRotation, Quaternion.Euler(rot * 1.5f), Time.deltaTime * 5);
        
    }

    private void ApplyHUDPosition()
    {
        hudTransform.localPosition = Vector3.Lerp(hudTransform.localPosition, hudTargetPosition, Time.deltaTime * 7);
    }

    public IEnumerator OnJump()
    {
        hudTargetPosition = hudOrigin + new Vector3(0, 25, 0);
        yield return new WaitForSeconds(0.05f);
        hudTargetPosition = hudOrigin;
    }
}
