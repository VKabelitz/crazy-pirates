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
        Debug.Log("Enemy spawned at " + health.HealthPoints + "/ " + health.maxHealth);
    }

    public void OnHit(int damage)
    {
        Debug.Log("Enemy took " + damage + " damage!");
        Debug.Log("Enemy is now at " + health.HealthPoints + "/ " + health.maxHealth);
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
        if (health.HealthPoints <= 0)
        {
            SprocketManager.instance.AddSprockets(sprocketAmount);
        }
        health.HealthPoints = health.maxHealth;
        enemyPool?.ReturnToPool(gameObject);
    }

    public void OnActivate()
    {
        gameObject.SetActive(true);
    }

    public void OnDeactivate()
    {
        gameObject.SetActive(false);
    }


}
