using UnityEngine;

public enum MovementType
{
    LinearMovement
}

public abstract class Enemy : MonoBehaviour, IEnemy, IPoolable
{
    [SerializeField]
    private ObjectPool enemyPool;
    private Health health;
    public int collisionDamage = 10;
    public int sprocketAmount = 30;
    private float defaultMovementSpeed = 2f;


    public void Awake()
    {
        SetMovementType(MovementType.LinearMovement, defaultMovementSpeed);
        if (gameObject.TryGetComponent(out Health health))
            this.health = health;
    }

    public void OnHit(int damage)
    {
        health.TakeDamage(damage);
    }

    public void SetMovementType(MovementType movementType, float movementSpeed)
    {
        if (gameObject.TryGetComponent(out BaseMovement movementComponent))
        {
            Destroy(movementComponent);
        }

        switch (movementType)
        {
            case MovementType.LinearMovement:
            default:
                LinearMovement linearMovement = gameObject.AddComponent<LinearMovement>();
                linearMovement.SetMovementSpeed(movementSpeed);

                Debug.Log("Linear Movement Added");
                break;
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
            // ScoreManager scoreManager = ServiceLocator.Get<ScoreManager>();
            // scoreManager.AddScore(5);

            SprocketManager.instance.AddSprockets(sprocketAmount);
        }
        gameObject.SetActive(false);
    }


}
