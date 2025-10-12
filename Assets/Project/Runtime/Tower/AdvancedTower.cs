using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AdvancedTower : Tower
{
    [SerializeField]
    private GameObject gate;

    [SerializeField]
    private GameObject projectilePrefab;

    [SerializeField]
    private Transform projectileSpawnPoint;

    [SerializeField]
    private ObjectPool projectilePool;

    [SerializeField]
    private float fireRate = 0.4f;

    [SerializeField]
    private float rangeRadius = 10f;

    [SerializeField]
    private float targetCheckInterval = 0.03f;

    [SerializeField]
    private float rotationSpeed = 0.001f;

    private Quaternion initialPitchRotation;

    private GameObject currentTarget;

    void Awake()
    {
        sprocketCosts = 100;
    }

    private void Start()
    {
        if (gate == null)
            Debug.LogWarning("Gate reference is missing in BasicTower.");
        StartCoroutine(TargetUpdater());
        StartCoroutine(Fire());
    }

    public override int GetSprocketCosts()
    {
        return base.GetSprocketCosts();
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
        float timePassed = 0f;
        while (true)
        {
            if (currentTarget != null)
            {
                Vector3 targetPos = currentTarget.transform.position;

                // --- YAW: Horizontal rotation around Y axis ---

                // Vector3 targetDirection = currentTarget.transform.position - PitchWheel.transform.position;
                // Vector3 localDirection = PitchWheel.transform.InverseTransformDirection(targetDirection);

                // if (localDirection.sqrMagnitude > 0.001f)
                // {
                //     float targetPitch = Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;

                //     // Apply pitch only on X axis, preserving original Y/Z
                //     Quaternion pitchRotation = Quaternion.Euler(targetPitch, 90f, 90f);
                //     PitchWheel.transform.localRotation = initialPitchRotation * pitchRotation;
                // }
            }
            timePassed += Time.deltaTime;
            if (timePassed >= fireRate)
            {
                timePassed = 0f;
                if (currentTarget != null)
                    Attack();
            }
            yield return null;
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
        if (currentTarget != null)
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
