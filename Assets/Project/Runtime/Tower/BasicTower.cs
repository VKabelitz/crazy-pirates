using UnityEngine;

public class BasicTower : Tower
{
    [SerializeField]
    GameObject projectile;

    [SerializeField]
    Transform projectileSpawnPoint;

    [SerializeField]
    ObjectPool projectilePool;

    public void Attack()
    {
        projectile = projectilePool.GetFromPool();
        projectile.transform.position = projectileSpawnPoint.position;
        projectile.transform.rotation = Quaternion.identity;

        var poolable = projectile.GetComponent<IPoolable>();
        poolable?.OnActivate();
    }
}
