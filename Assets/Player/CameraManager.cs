using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraManager : MonoBehaviour
{
    void Start()
    {
        var baseCameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
        Camera uiCamera = UI.Instance.UICamera;
        Debug.Log("CAmera " + uiCamera);
        if (uiCamera != null && !baseCameraData.cameraStack.Contains(uiCamera))
        {
            baseCameraData.cameraStack.Add(uiCamera);
        }
    }
}
