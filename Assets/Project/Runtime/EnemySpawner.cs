using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private ObjectPool enemyPool;

    [SerializeField]
    private float spawnRate;
    private float timer;
   
    private void SpawnEnemy()
    {
        GameObject enemy = enemyPool.GetFromPool();
        if (enemy != null)
        {
            float positionX = 10f;
            float positionY = Random.Range(-4.5f, 4.5f);
            enemy.transform.position = new Vector3(positionX, positionY, 0f);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0;
        }
    }
}
