using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour
{
    [HideInInspector] public GameObject player;
    protected PlayerMovement pm;
    [HideInInspector] public Transform cameraPivot;
    protected Vector3 startPos = new Vector3(0, -1f, 0f);
    public string itemName;
    protected Inventory inventory;
    protected Animator animator = null;
    protected bool canUse = false;
    private Transform modelTransform;

    protected virtual void Awake()
    {
        pm = PlayerMovement.Instance;
        animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        pm = PlayerMovement.Instance;
        //pm = player.GetComponent<PlayerMovement>();
        Animator anim = GetComponentInChildren<Animator>();
        modelTransform = anim != null ? anim.transform : transform;
    }

    public void OnEquip()
    {
        canUse = false;
        gameObject.SetActive(true);
        StartCoroutine(EquipAnimation());
        
        transform.DOKill();
    }

    public void OnUnequip()
    {
        StopCoroutine(EquipAnimation());
        canUse = false;
        gameObject.SetActive(false);
    }

    IEnumerator EquipAnimation()
    {
        if(animator != null)
            animator.Play("Equip");
        Player.Instance.CameraRecoil.ApplyRecoil(-4, 6, 1, false);
        yield return new WaitForSeconds(0.1f);
        Player.Instance.CameraRecoil.ApplyRecoil(-4, 10, 5, false);
        canUse = true;
    }

    public void SetInventory(Inventory inv)
    {
        inventory = inv;
    }

    public bool GetCanUse()
    {
        return canUse;
    }
}
