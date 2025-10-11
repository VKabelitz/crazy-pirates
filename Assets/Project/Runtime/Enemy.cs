using UnityEngine;

public enum MovementType
{
    LinearMovement
}

public class Enemy : MonoBehaviour, IEnemy, IPoolable
{
    [SerializeField]
    private float movementSpeed;
    private ObjectPool enemyPool;
    private Health health;
    public int collisionDamage = 10;

    public void Awake()
    {
        if (gameObject.TryGetComponent(out Health health))
            this.health = health;
    }

    public void OnHit(int damage)
    {
        health.TakeDamage(damage);
    }

    public void SetMovementType(MovementType movementType)
    {
        if (gameObject.TryGetComponent(out BaseMovement movementComponent))
        {
            Destroy(movementComponent);
        }

        switch (movementType)
        {
            case MovementType.LinearMovement:
            default:
                gameObject.AddComponent<LinearMovement>();
                break;
        }
    }

    public void Update()
    {
        if (transform.position.x < -10f)
        {
            ReturnToPool();
        }
    }

    public void SetPool(ObjectPool pool)
    {
        enemyPool = pool;
    }

    public void ReturnToPool()
    {
        enemyPool?.ReturnToPool(gameObject);
    }

    public void OnActivate()
    {
        gameObject.SetActive(true);
    }

    public void OnDeactivate()
    {
        if (health.HealthPoints < 1)
        {
            ScoreManager scoreManager = ServiceLocator.Get<ScoreManager>();
            scoreManager.AddScore(5);
        }
        gameObject.SetActive(false);
    }
}
