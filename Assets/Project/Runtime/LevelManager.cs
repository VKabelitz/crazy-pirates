using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private LevelSequence levelSequence;

    [SerializeField]
    private ObjectPool enemyPool;

    public void Start()
    {
        StartCoroutine(RunLevelSequence());
    }

    private IEnumerator RunLevelSequence()
    {
        foreach (EnemyWave wave in levelSequence.waves)
        {
            yield return StartCoroutine(SpawnWave(wave));
        }
    }

    private IEnumerator SpawnWave(EnemyWave wave)
    {
        float elapsedTime = 0f;
        int currentEntryIndex = 0;

        while (elapsedTime < wave.waveDuration)
        {
            while (
                wave.enemies.Count > currentEntryIndex
                && wave.enemies[currentEntryIndex].spawnTime <= elapsedTime
            )
            {
                SpawnEnemy(wave.enemies[currentEntryIndex]);
                currentEntryIndex++;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void SpawnEnemy(EnemyWaveEntry enemyWaveEntry)
    {
        GameObject enemy = enemyPool.GetFromPool();
        if (enemy.TryGetComponent(out Enemy enemyComponent))
        {
            enemyComponent.SetMovementType(enemyWaveEntry.movementType);
        }
        enemy.transform.position = enemyWaveEntry.spawnPosition;
    }
}
