using UnityEngine;

public class RocketLauncher : Gun
{
    [Header("Important Setup")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private GameObject explosionPrefab;

    [Header("Gun")]
    [SerializeField] private float rocketSpeed = 1f;
    [SerializeField] private float explosionRadius = 10;
    [SerializeField] private float explosionForce = 5;
    [SerializeField] private float explosionForceRB = 5;

    private GameObject newRocket;


    protected override void Shoot()
    {
        if(itemName == "rocket")
            SoundManager.PlaySound(SoundType.ROCKETSHOT, 0.5f);
        newRocket = Instantiate(rocketPrefab, cameraPivot.position, cameraPivot.rotation);
        Rocket rocketComp =  newRocket.GetComponent<Rocket>();
        rocketComp.rocketStart = cameraPivot;
        rocketComp.damage = damage;
        rocketComp.explosionPrefab = explosionPrefab;
        rocketComp.rocketSpeed = rocketSpeed;
        rocketComp.explosionRadius = explosionRadius;
        rocketComp.explosionForce = explosionForce;
        rocketComp.explosionForceRB = explosionForceRB;
        newRocket.GetComponent<MeshRenderer>().enabled = false;
    }

}
