using UnityEngine;

public enum MovementType
{
    LinearMovement
}

public abstract class Enemy : MonoBehaviour, IEnemy, IPoolable
{
    [SerializeField]
    protected ObjectPool enemyPool;
    protected Health health;
    public int collisionDamage = 10;
    public int sprocketAmount = 30;
    private float defaultMovementSpeed = 2f;
    protected LinearMovement linearMovement;


    public void Awake()
    {
        SetMovementType(MovementType.LinearMovement, defaultMovementSpeed);
        if (gameObject.TryGetComponent(out Health health))
            this.health = health;
        Debug.Log("Enemy spawned at " + health.HealthPoints + "/ " + health.maxHealth);
    }

    protected virtual void ResetEnemy()
    {
        health.HealthPoints = health.maxHealth;
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
                linearMovement = gameObject.AddComponent<LinearMovement>();
                linearMovement.SetMovementSpeed(movementSpeed);

                Debug.Log("Linear Movement Added");
                break;
        }
    }

    public void SetPool(ObjectPool pool)
    {
        enemyPool = pool;
    }

    public virtual void ReturnToPool()
    {
        if (health.HealthPoints <= 0)
        {
            SprocketManager.instance.AddSprockets(sprocketAmount);
        }
        ResetEnemy();
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
