using UnityEngine;

[CreateAssetMenu(menuName = "Game/Game Assets")]
public class GameAssets : ScriptableObject
{
    static GameAssets instance;
    public static GameAssets Instance
    {
        get
        {
            if (instance == null) instance = Resources.Load<GameAssets>("GameAssets");
            return instance;
        }
    }

    public GameObject decalParticles;
    public GameObject enemySpawnParticles;
    public GameObject bloodParticles;
    public Material whiteMaterial;
    public GameObject healthOrb;
    public GameObject ammoOrb;
}