using UnityEngine;
using System.Collections;
public class BasicTower : MonoBehaviour, Tower
{
    [SerializeField]
    GameObject projectile;

    [SerializeField]
    Transform projectileSpawnPoint;

    [SerializeField]
    ObjectPool projectilePool;

    public void Attack()
    {
            StartCoroutine(Fire());
            Debug.Log("Attack");

    }

    private IEnumerator Fire()
    {
        while (true)
        {        
            Debug.Log("IEnumerator1");
            projectile = projectilePool.GetFromPool();
            projectile.transform.position = projectileSpawnPoint.position;
            projectile.transform.rotation = Quaternion.identity;

            var poolable = projectile.GetComponent<IPoolable>();
            Debug.Log("Poolable: " + poolable);
            poolable?.OnActivate();
            yield return new WaitForSeconds(0.4f);
        }
    }
    public void Start()
    {
        Debug.Log("Start");
        Attack();
    }
}
