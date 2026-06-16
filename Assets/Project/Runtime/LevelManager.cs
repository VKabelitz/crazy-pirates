using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private LevelSequence levelSequence;

    // [SerializeField]
    // private ObjectPool enemyPool;
    private Dictionary<GameObject, ObjectPool> enemyPools = new Dictionary<GameObject, ObjectPool>();
    [SerializeField]
    private GameObject spawnPosition;


    public void Awake()
    {
        initEnemyPools();
    }

    public void Start()
    {
        AudioManager.instance.PlaySound("level_start");
        AudioManager.instance.SwitchMusic("BackgroundMusic");
        StartCoroutine(RunLevelSequence());
    }
    private void initEnemyPools()
    {
        foreach (EnemyWave wave in levelSequence.waves)
        {
            foreach (EnemyWaveEntry entry in wave.enemies)
            {
                if (enemyPools.ContainsKey(entry.enemyPrefab))
                    continue;
                Debug.Log("Creating pool for enemy prefab " + entry.enemyPrefab);
                GameObject poolGO = new GameObject(entry.enemyPrefab.name + "_Pool");
                ObjectPool pool = poolGO.AddComponent<ObjectPool>();
                pool.Init(entry.enemyPrefab, 20);
                enemyPools.Add(entry.enemyPrefab, pool);
            }
        }
    }

    private IEnumerator RunLevelSequence()
    {
        Debug.Log("Starting Level Sequence, in total " + levelSequence.waves.Count + " waves.");
        foreach (EnemyWave wave in levelSequence.waves)
        {
            Debug.Log("Starting new wave with " + wave.enemies.Count + " enemies.");
            yield return StartCoroutine(SpawnWave(wave));
        }

        AudioManager.instance.PlaySound("victory");
        LevelEndMenu.instance.activateMenu();
    }

    private IEnumerator SpawnWave(EnemyWave wave)
    {
        int currentEntryIndex = 0;

        while (currentEntryIndex < wave.enemies.Count)
        {
            var entry = wave.enemies[currentEntryIndex];
            Debug.Log($"Spawning enemy of type {entry} after {entry.spawnTime} seconds.");
            yield return new WaitForSeconds(entry.spawnTime);
            SpawnEnemy(entry);
            currentEntryIndex++;
        }

        Debug.Log($"Wave completed. Waiting for {wave.waveDuration} seconds before next wave.");
        yield return new WaitForSeconds(wave.waveDuration);
    }

    private void SpawnEnemy(EnemyWaveEntry enemyWaveEntry)
    {
        ObjectPool enemyPool = enemyPools[enemyWaveEntry.enemyPrefab];
        GameObject enemy = enemyPool.GetFromPool();
        if (enemy.TryGetComponent(out Enemy enemyComponent))
        {
            enemyComponent.SetMovementType(enemyWaveEntry.movementType, enemyWaveEntry.movementSpeed);
        }
        enemy.transform.position = spawnPosition.transform.position;
    }

    public void StartNextLevel()
    {
        Debug.Log("Loading next level...");
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
            return;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
