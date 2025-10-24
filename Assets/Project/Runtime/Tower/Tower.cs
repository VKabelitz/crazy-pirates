using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
public abstract class Tower : MonoBehaviour
{

    [SerializeField]
    protected int sprocketCosts = 200;
    [SerializeField]
    private string projectileSound = "arrow_shoot";
    [SerializeField]
    protected GameObject YawWheel;
    [SerializeField]
    protected float rotationSpeed = 0.001f;
    [SerializeField]
    private GameObject gate;
    [SerializeField]
    private float rangeRadius = 10f;
    private float targetCheckInterval = 0.03f;
    [SerializeField]
    private GameObject projectilePoolPrefab;
    protected GameObject currentTarget;
    [SerializeField]
    protected float fireRate = 0.4f;
    [SerializeField]
    protected Transform projectileSpawnPoint;
    protected float currentFireRate;
    public static int range;
    public static bool attackingEnemy;
    protected ObjectPool pool;



    public int GetSprocketCosts()
    {
        return sprocketCosts;
    }


    protected virtual void Awake()
    {
        GameObject poolInstance = Instantiate(projectilePoolPrefab);
        pool = poolInstance.GetComponent<ObjectPool>();
    }

    protected virtual void Start()
    {
        if (gate == null)
            Debug.LogWarning("Gate reference is missing in BasicTower.");
        StartCoroutine(TargetUpdater());
        StartCoroutine(Fire());
    }
    protected virtual void Update()
    {
        if (currentFireRate < fireRate)
        {
            currentFireRate += Time.deltaTime;
        }
    }

    private IEnumerator TargetUpdater()
    {
        while (true)
        {
            ChooseNearestEnemy();
            yield return new WaitForSeconds(targetCheckInterval);
        }
    }

    public void SlowTower(float slowScale)
    {
        currentFireRate = fireRate * slowScale;
    }


    protected void ChooseNearestEnemy()
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

    protected virtual void Attack()
    {
        GameObject projectile = pool.GetFromPool();
        projectile.transform.position = projectileSpawnPoint.position;

        if (currentTarget != null)
        {
            // Berechne die Richtung zum Ziel
            Vector3 directionToTarget = (currentTarget.transform.position - projectileSpawnPoint.position).normalized;

            // Setze die Rotation des Projektils so, dass es in Richtung des Gegners zeigt
            projectile.transform.rotation = Quaternion.LookRotation(directionToTarget);
        }
        else
        {
            // Standardrotation, falls kein Ziel vorhanden ist
            projectile.transform.rotation = Quaternion.identity;
        }

        var poolable = projectile.GetComponent<IPoolable>();
        poolable?.OnActivate();

        var projectileComp = projectile.GetComponent<Projectile>();
        if (currentTarget != null)
            projectileComp?.SetTarget(currentTarget.GetComponent<Enemy>());

        AudioManager.instance.PlaySound(projectileSound);
    }



    private IEnumerator Fire()
    {
        float timePassed = 0f;
        while (true)
        {
            if (currentTarget != null)
            {
                FaceTarget();
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

    protected virtual void FaceTarget()
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