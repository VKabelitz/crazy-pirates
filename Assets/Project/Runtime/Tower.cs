using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    public static int range;
    public static bool attackingEnemy;
    public int sprocketCosts;
    [SerializeField]
    protected GameObject projectilePoolObject;

    [SerializeField]
    protected float fireRate = 0.4f;
    protected float currentFireRate;
    public virtual int GetSprocketCosts()
    {
        return sprocketCosts;
    }

    protected virtual void Update()
    {
        if (currentFireRate < fireRate)
        {
            currentFireRate += Time.deltaTime;
        }
    }

    public void SlowTower(float slowScale)
    {
        currentFireRate = fireRate * slowScale;
    }
}