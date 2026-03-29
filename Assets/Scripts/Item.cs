using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour
{
    [HideInInspector] public GameObject player;
    [HideInInspector] public Transform cameraPivot;
    protected Vector3 startPos = new Vector3(0, -1f, 0f);
    public string itemName;
    protected Inventory inventory;

    protected bool canUse = false; 

    public void OnEquip()
    {
        transform.localPosition = startPos;
        canUse = false;
        gameObject.SetActive(true);
        StartCoroutine(EquipAnimation());

        transform.DOKill();
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
        Vector3 targetPos = Vector3.zero;
        float duration = 0.1f;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        canUse = true;
        transform.localPosition = targetPos;
    }
}
