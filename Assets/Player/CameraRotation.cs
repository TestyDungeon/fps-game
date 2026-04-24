using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    [SerializeField] private Transform playerPivot;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float xMouseSensitivity = 3;
    [SerializeField] private float yMouseSensitivity = 3;
    [SerializeField] private float sideTilt = 15;
    [SerializeField] private float sideTiltSmooth = 5;
    private float targetSideTilt = 0;
    private float currentSideTilt = 0;

    private float rotX, rotY;

    void Update()
    {
        if(GameManager.Instance.inUI)
            return;
        Mouse();
        CameraTilt();
    }

    private void Mouse()
    {
        rotX -= Input.GetAxisRaw("Mouse Y") * xMouseSensitivity;
        rotY += Input.GetAxisRaw("Mouse X") * yMouseSensitivity;

        if (rotX > 90)
            rotX = 90;
        if (rotX < -90)
            rotX = -90;

        playerPivot.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
        cameraPivot.transform.localRotation = Quaternion.Euler(rotX, 0f, currentSideTilt);
    }

    private void CameraTilt()
    {
        float side = Input.GetAxisRaw("Horizontal");
        if (side == 1)
        {
            targetSideTilt = -sideTilt;
        }
        else if (side == -1)
            targetSideTilt = sideTilt;
        else
            targetSideTilt = 0;

        currentSideTilt = Mathf.Lerp(currentSideTilt, targetSideTilt, sideTiltSmooth * Time.deltaTime);

    }
}
