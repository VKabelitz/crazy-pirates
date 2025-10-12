using UnityEngine;

public class Projectile : MonoBehaviour, IMovable, IPoolable
{
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private int damage;
    [SerializeField] private ParticleSystem OnHitEffect;
    [SerializeField]
    private ObjectPool projectilePool;
    private Enemy currentTarget;

    public float MovementSpeed => movementSpeed;

    public void SetTarget(Enemy target)
    {
        currentTarget = target;
    }

    public void Move(float horizontal, float vertical, float depth)
    {
        Vector3 dir = new Vector3(horizontal, vertical, depth).normalized;
        transform.position += dir * Time.deltaTime * movementSpeed;
    }

    public void OnActivate() => gameObject.SetActive(true);
    public void OnDeactivate() => gameObject.SetActive(false);
    public void ReturnToPool() => projectilePool?.ReturnToPool(gameObject);
    public void SetPool(ObjectPool pool) => projectilePool = pool;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent(out Health hitObjectHealth))
                hitObjectHealth.TakeDamage(damage);

            if (OnHitEffect != null)
            {
                var onHitObj = Instantiate(OnHitEffect, transform.position, Quaternion.identity);
                Destroy(onHitObj.gameObject, 2f);
            }

            ReturnToPool();
        }
    }

    private void Start()
    {
        // if (projectilePool.;
        //     projectilePool.InitializePool();
    }
    private void Update()
    {
        // 💡 Safety check: If target is null OR inactive, return to pool.
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            ReturnToPool();
            return;
        }

        Vector3 moveDirection = (currentTarget.transform.position - transform.position).normalized;
        transform.position += moveDirection * movementSpeed * Time.deltaTime;

        // Optional: If projectile flies too far, recycle it
        if (transform.position.magnitude > 100f)
            ReturnToPool();
    }
}
