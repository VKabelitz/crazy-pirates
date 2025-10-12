using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private LevelSequence levelSequence;

    [SerializeField]
    private ObjectPool enemyPool;

    [SerializeField]
    private GameObject spawnPosition;


    public void Start()
    {
        AudioManager.instance.SwitchMusic("Theme");
        AudioManager.instance.PlaySound("level_start");
        AudioManager.instance.SwitchMusic("BackgroundMusic");
        StartCoroutine(RunLevelSequence());
    }

    private IEnumerator RunLevelSequence()
    {
        foreach (EnemyWave wave in levelSequence.waves)
        {
            yield return StartCoroutine(SpawnWave(wave));
        }

        AudioManager.instance.PlaySound("victory");
        // Pause Menu einzeigen mit Button "Nächstes Level" und Anzeige mit Stats
        LevelEndMenu.instance.activateMenu();

    }

    private IEnumerator SpawnWave(EnemyWave wave)
    {
        int currentEntryIndex = 0;

        while (currentEntryIndex < wave.enemies.Count)
        {
            var entry = wave.enemies[currentEntryIndex];

            // Warte die Spawn-Zeit des aktuellen Gegners ab
            yield return new WaitForSeconds(entry.spawnTime);

            // Spawne den Gegner
            SpawnEnemy(entry);
            currentEntryIndex++;
        }
        yield return new WaitForSeconds(wave.waveDuration);
    }

    private void SpawnEnemy(EnemyWaveEntry enemyWaveEntry)
    {
        GameObject enemy = enemyPool.GetFromPool();
        if (enemy.TryGetComponent(out Enemy enemyComponent))
        {
            enemyComponent.SetMovementType(enemyWaveEntry.movementType, enemyWaveEntry.movementSpeed);
        }
        enemy.transform.position = spawnPosition.transform.position;
    }

    public void StartNextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
            return;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
