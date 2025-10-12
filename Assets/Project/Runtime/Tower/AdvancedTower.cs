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
    private GameObject projectilePoolPrefab;



    [SerializeField]
    private float rangeRadius = 10f;

    [SerializeField]
    private float targetCheckInterval = 0.03f;

    [SerializeField]
    private float rotationSpeed = 0.001f;

    [SerializeField]
    private GameObject YawWheel;

    private Quaternion initialPitchRotation;

    private GameObject currentTarget;
    private ObjectPool pool;

    void Awake()
    {
        sprocketCosts = 100;
        GameObject poolInstance = Instantiate(projectilePoolPrefab);
        pool = poolInstance.GetComponent<ObjectPool>();
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
                Vector3 yawDirection = targetPos - YawWheel.transform.position;
                yawDirection.y = 0; // Ignore vertical component
                
                if (yawDirection.sqrMagnitude > 0.001f)
                {
                    // Calculate target yaw angle
                    float targetYaw =
                        Mathf.Atan2(yawDirection.x, yawDirection.z) * Mathf.Rad2Deg + 90f;

                    // Get current yaw angle
                    float currentYaw = YawWheel.transform.eulerAngles.y;

                    // Smoothly interpolate yaw
                    float smoothYaw = Mathf.LerpAngle(
                        currentYaw,
                        targetYaw,
                        Time.deltaTime * rotationSpeed
                    );

                    // Apply rotation (preserve other axes if needed)
                    YawWheel.transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
                }
            }
            timePassed += Time.deltaTime;
            if (timePassed >= currentFireRate)
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
        GameObject projectile = pool.GetFromPool();
        projectile.transform.position = projectileSpawnPoint.position;
        projectile.transform.rotation = Quaternion.identity;

        var poolable = projectile.GetComponent<IPoolable>();
        poolable?.OnActivate();

        var projectileComp = projectile.GetComponent<Projectile>();
        if (currentTarget != null)
            projectileComp?.SetTarget(currentTarget.GetComponent<Enemy>());

        AudioManager.instance.PlaySound("turret2_shoot");
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
