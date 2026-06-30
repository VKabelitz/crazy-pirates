using UnityEngine;
using Unity.Mathematics;

public class SplittingEnemy : Enemy
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int splitLevel;
    [SerializeField] private int maxSplitLevel = 3;
    [SerializeField] private float sizeFactor = 0.7f;

    private void Start()
    {
        health = GetComponent<Health>();

    }

    public override void ReturnToPool()
    {
        Split();
        base.ReturnToPool();
    }

    protected override void ResetEnemy()
    {
        base.ResetEnemy();
        splitLevel = 0;
        transform.localScale = Vector3.one;

        health.SetMaxHealth(health.originalMaxHealth);
        health.HealthPoints = health.maxHealth;
    }

    private void Split()
    {
        if (splitLevel >= maxSplitLevel)
            return;

        SpawnChild(Vector3.left);
        SpawnChild(Vector3.right);
    }

    public void OnSpawnedByParent(int level, int wayPointIndex)
    {
        splitLevel = level;
        linearMovement.currentWayPointIndex = wayPointIndex;
        int divisor = 1 << splitLevel;       // divison by 2^splitLevel by bit shifting
        Debug.Log("Enemy spawned at split level " + splitLevel + " with health " + health.HealthPoints + "/" + health.maxHealth);
        health.SetMaxHealth((int)math.ceil(health.maxHealth / divisor));
        sprocketAmount = (int)math.ceil(1.2f * (float)sprocketAmount / ((float)splitLevel));
    }

    private void SpawnChild(Vector3 offset)
    {
        // GameObject child = Instantiate(
        //     enemyPrefab,
        //     transform.position + offset,
        //     Quaternion.identity);
        GameObject child = enemyPool.GetFromPool();
        child.transform.position = transform.position + offset;
        child.transform.rotation = transform.rotation;
        child.transform.localScale =
            transform.localScale * sizeFactor;

        SplittingEnemy childEnemy =
            child.GetComponent<SplittingEnemy>();
        childEnemy.OnSpawnedByParent(splitLevel + 1, linearMovement.currentWayPointIndex);
    }
}