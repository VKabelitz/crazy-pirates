using System.Collections;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
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
    private Health health;
    private GameObject gate;
    [SerializeField]
    private float rangeRadius = 10f;
    private float targetCheckInterval = 0.03f;
    [SerializeField]
    private int targets = 1;
    [SerializeField]
    private GameObject projectilePoolPrefab;
    protected List<GameObject> currentTargets;

    [SerializeField]
    protected float fireRate = 0.4f;
    [SerializeField]
    protected Transform projectileSpawnPoint;
    protected float currentFireRate;
    public static int range;
    public static bool attackingEnemy;
    public float destroyFadeDuration = 1.0f;
    public bool fireActive = false;
    protected ObjectPool pool;

    private TowerPlaceManager manager;


    public int GetSprocketCosts()
    {
        return sprocketCosts;
    }


    protected virtual void Awake()
    {
        GameObject poolInstance = Instantiate(projectilePoolPrefab);
        pool = poolInstance.GetComponent<ObjectPool>();
        if (gameObject.TryGetComponent(out Health health))
            this.health = health;
        Debug.Log("Set Health of Tower to " + health.HealthPoints);
        fireActive = true;
        currentTargets = new List<GameObject>();

        manager = Object.FindFirstObjectByType<TowerPlaceManager>();
    }

    protected virtual void Start()
    {
        if (gate == null)
        {
            gate = FindFirstObjectByType<Gate>().gameObject;
            if (gate == null)
            {
                Debug.LogError("Tower cannot find Gate in the scene!");
            }
        }
        StartCoroutine(TargetUpdater());
        StartCoroutine(Fire());
    }
    protected virtual void Update()
    {
        if (currentFireRate < fireRate)
        {
            currentFireRate += Time.deltaTime;
        }
        OnHit(1);

    }

    public void OnHit(int damage)
    {
        health.TakeDamage(damage);
    }
    private IEnumerator TargetUpdater()
    {
        while (true)
        {
            ChooseNearestEnemies();
            yield return new WaitForSeconds(targetCheckInterval);
        }
    }

    public void SlowTower(float slowScale)
    {
        currentFireRate = fireRate * slowScale;
    }


    protected void ChooseNearestEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Filter, sort, limit, convert to list
        currentTargets = enemies
            .Where(e => e != null)
            .Where(e => Vector3.Distance(transform.position, e.transform.position) < rangeRadius)
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .Take(targets)
            .ToList();
    }



    protected virtual void Attack()
    {

        if (currentTargets.Count == 0)
        {
            return;
        }

        foreach (GameObject currentTarget in currentTargets)

        {
            if (currentTarget == null)
                continue;
            GameObject projectile = pool.GetFromPool();
            projectile.transform.position = projectileSpawnPoint.position;
            // Berechne die Richtung zum Ziel
            Vector3 directionToTarget = (currentTarget.transform.position - projectileSpawnPoint.position).normalized;

            // Setze die Rotation des Projektils so, dass es in Richtung des Gegners zeigt
            projectile.transform.rotation = Quaternion.LookRotation(directionToTarget);

            var poolable = projectile.GetComponent<IPoolable>();
            poolable?.OnActivate();

            var projectileComp = projectile.GetComponent<Projectile>();
            if (currentTarget != null)
                projectileComp?.SetTarget(currentTarget.GetComponent<Enemy>());
        }
        AudioManager.instance.PlaySound(projectileSound);
    }

    private void OnDestroy()
    {
        Debug.Log("Tower destroyed!");
    }

    public IEnumerator FadeAndDestroy()
    {
        fireActive = false;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Cache all materials
        List<Material> mats = new List<Material>();
        foreach (var r in renderers)
            mats.AddRange(r.materials);

        float t = 0f;

        while (t < destroyFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / destroyFadeDuration);

            foreach (var m in mats)
            {
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;
                }
            }

            yield return null;
        }
        manager.ReleaseSpotByPosition(this.transform.position);
        Destroy(gameObject);
    }

    private IEnumerator Fire()
    {

        float timePassed = 0f;
        while (true)
        {
            if (!fireActive)
            {
                yield return null;
                continue;
            }
            if (currentTargets != null)
            {
                FaceTarget();
            }
            timePassed += Time.deltaTime;
            if (timePassed >= currentFireRate)
            {
                timePassed = 0f;
                if (currentTargets != null)
                    Attack();
            }
            yield return null;
        }
    }

    protected virtual void FaceTarget()
    {
        if (currentTargets.Count == 0)
            return;
        Vector3 targetPos = currentTargets[0].transform.position;

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

        if (currentTargets.Count != 0)
        {
            GameObject currentTarget = currentTargets[0];
            if (currentTarget == null)
                return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
#endif
}