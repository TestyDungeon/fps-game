using System.Collections;
using UnityEngine;

public class Item : MonoBehaviour
{
    [HideInInspector] public GameObject player;
    [HideInInspector] public Transform cameraPivot;
    protected Vector3 startPos = new Vector3(0, -0.5f, 0f);
    Vector3 startRot = new Vector3(50, 0, 0);
    public string itemName;
    protected Inventory inventory;

    protected bool canUse = false; 

    public void OnEquip()
    {
        transform.localPosition = startPos;
        canUse = false;
        gameObject.SetActive(true);
        StartCoroutine(EquipAnimation());
    }

    public void OnUnequip()
    {
        StopCoroutine(EquipAnimation());
        transform.localPosition = startPos;
        canUse = false;
        gameObject.SetActive(false);
    }

    IEnumerator EquipAnimation()
    {
        
        transform.localPosition = startPos;
        //transform.localEulerAngles = startRot;
        Vector3 targetPos = Vector3.zero;
        //Vector3 targetRot = Vector3.zero;
        float duration = 0.1f;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            //transform.localEulerAngles = Vector3.Lerp(startRot, targetRot, t);
            yield return null;
        }
        canUse = true;
        transform.localPosition = targetPos;
    }
}
