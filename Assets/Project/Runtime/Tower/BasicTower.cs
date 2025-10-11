using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.AI;

public class BasicTower : MonoBehaviour, Tower
{
    [SerializeField] private GameObject gate;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private ObjectPool projectilePool;
    [SerializeField] private float fireRate = 0.4f;
    [SerializeField] private float rangeRadius = 10f;
    [SerializeField] private float targetCheckInterval = 0.5f;

    private GameObject currentTarget;

    private void Start()
    {
        if (gate == null)
            Debug.LogWarning("Gate reference is missing in BasicTower.");
        StartCoroutine(TargetUpdater());
        StartCoroutine(Fire());
    }

    private IEnumerator TargetUpdater()
    {
        while (true)
        {
            ChooseNearestEnemy();
            yield return new WaitForSeconds(targetCheckInterval);
        }
    }

    private void ChooseNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies == null || enemies.Length == 0)
        {
            currentTarget = null;
            return;
        }

        var enemiesInRange = enemies
            .Where(e => Vector3.Distance(transform.position, e.transform.position) < rangeRadius)
            .ToArray();

        if (enemiesInRange.Length == 0)
        {
            currentTarget = null;
            return;
        }

        currentTarget = enemiesInRange
            .OrderBy(e => Vector3.Distance(gate.transform.position, e.transform.position))
            .FirstOrDefault();
    }

    private IEnumerator Fire()
    {
        while (true)
        {
            if (currentTarget != null)
                Attack();
            yield return new WaitForSeconds(fireRate);
        }
    }

    public void Attack()
    {
        GameObject projectile = projectilePool.GetFromPool();
        projectile.transform.position = projectileSpawnPoint.position;
        projectile.transform.rotation = Quaternion.identity;

        var poolable = projectile.GetComponent<IPoolable>();
        poolable?.OnActivate();

        var projectileComp = projectile.GetComponent<Projectile>();
        projectileComp?.SetTarget(currentTarget.GetComponent<Enemy>());
    }

    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.12f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, rangeRadius);

            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    #endif
}
