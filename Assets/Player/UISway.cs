using System.Collections.Generic;
using UnityEngine;

// Lives under PersistentUI (DontDestroyOnLoad). Never holds a reference to the
// Player or to Sway — it just listens for whichever Sway instance is currently
// broadcasting. Its own uiTransforms/hudTransforms refs are safe to wire in the
// Inspector because they point at children of this same persistent hierarchy.
public class UISway : MonoBehaviour
{
    [SerializeField] private List<RectTransform> uiTransforms;
    [SerializeField] private List<RectTransform> hudTransforms;

    private Vector3 rot;

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

        if (hudTransforms.Count > 0)
        {
            ApplyHUDRotation();
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
        foreach (Transform hudTransform in hudTransforms)
        {
            hudTransform.localRotation = Quaternion.Lerp(hudTransform.localRotation, Quaternion.Euler(rot * 1.5f), Time.deltaTime * 5);
        }
    }
}
