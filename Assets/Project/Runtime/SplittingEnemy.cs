using UnityEngine;

public class SplittingEnemy : Enemy
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int splitLevel;
    [SerializeField] private int maxSplitLevel = 3;
    [SerializeField] private float sizeFactor = 0.7f;

    private Health health;

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
    }

    private void Split()
    {
        if (splitLevel >= maxSplitLevel)
            return;

        SpawnChild(Vector3.left);
        SpawnChild(Vector3.right);
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

        SplittingEnemy splittingEnemy =
            child.GetComponent<SplittingEnemy>();

        splittingEnemy.splitLevel = splitLevel + 1;
    }
}