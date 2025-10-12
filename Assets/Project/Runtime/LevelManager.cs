using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class LevelManager : MonoBehaviour
{

    [SerializeField]
    private LevelSequence levelSequence;

    [SerializeField]
    private ObjectPool enemyPool;

    [SerializeField]
    private GameObject spawnPosition;
    public static LevelManager instance = null;

    [SerializeField]
    private string activeLevelName = "undefined";
    public Vector3 currentPlayerResetPos;

    [SerializeField]
    private GameObject baseLevel;

    // Alle Levels zum Laden
    public List<GameObject> levelObjects = new List<GameObject>();

    [SerializeField]
    private List<string> levels = new List<string>();

    public void LoadActiveLevelObjects(int positionId)
    {
        switch (positionId)
        {
            case 0:
                levelObjects[0].SetActive(true);
                levelObjects[1].SetActive(true);
                levelObjects[2].SetActive(false); // GuardianRoom an
                levelObjects[3].SetActive(false);
                levelObjects[4].SetActive(false); // Hallway 2 an
                levelObjects[5].SetActive(false);
                levelObjects[6].SetActive(false);
                levelObjects[7].SetActive(false);
                levelObjects[8].SetActive(false);

                break;
            case 1:
                levelObjects[0].SetActive(true);
                levelObjects[1].SetActive(true);
                levelObjects[2].SetActive(true); // GuardianRoom an
                levelObjects[3].SetActive(false);
                levelObjects[4].SetActive(true); // Hallway 2 an
                levelObjects[5].SetActive(false);
                levelObjects[6].SetActive(false);
                levelObjects[7].SetActive(false);
                levelObjects[8].SetActive(false);

                break;
            case 2:
                // Tür von Guardian Room muss zu gehen
                levelObjects[0].SetActive(false); // StartLevel aus
                levelObjects[1].SetActive(false); // Hallway1 aus
                levelObjects[2].SetActive(true); // GuardianRoom an
                levelObjects[3].SetActive(true); // ClosedDoor1 an
                levelObjects[4].SetActive(true);
                levelObjects[5].SetActive(true); //TwinRoom an
                levelObjects[6].SetActive(false);
                levelObjects[7].SetActive(true); // Hallway3 an
                levelObjects[8].SetActive(false);
                break;
            case 3:
                // Tür von Twin Room muss zu gehen
                levelObjects[0].SetActive(false);
                levelObjects[1].SetActive(false);
                levelObjects[2].SetActive(false); // GuardianRoom aus
                levelObjects[3].SetActive(true); // ClosedDoor1 an
                levelObjects[4].SetActive(false);
                levelObjects[5].SetActive(true);
                levelObjects[6].SetActive(true); // ClosedDoor2 an
                levelObjects[7].SetActive(true); // Hallway3
                levelObjects[8].SetActive(true); // Hallway 5 an
                break;
            case 4:
                // Finale Cutscene, alles aus
                levelObjects[0].SetActive(false);
                levelObjects[1].SetActive(false);
                levelObjects[2].SetActive(false);
                levelObjects[3].SetActive(false);
                levelObjects[4].SetActive(false);
                levelObjects[5].SetActive(false);
                levelObjects[6].SetActive(false);
                levelObjects[7].SetActive(false);
                levelObjects[8].SetActive(false);
                break;
        }
    }

    void Awake()
    {
        if (!instance)
        {
            instance = this;
            foreach (GameObject levelObject in levelObjects)
            {
                levels.Add(levelObject.name);
            }

            // The LevelManager should not be destroyed when Loading a Scene e.g. if the level is being reset.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // so that the game Object doesnt duplicate on scene load
            Destroy(gameObject);
        }
    }

    public void SetActiveLevel(Level level)
    {
        activeLevelName = level.gameObject.name;
        currentPlayerResetPos = level.resetPosition;
    }

    // OnSceneLoaded Pattern from https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneManagement.SceneManager-sceneLoaded.html
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Level müssen wieder neu gefunden werden
        levelObjects = new List<GameObject>();
        foreach (string levelName in levels)
        {
            GameObject currentLevel = GameObject.Find(levelName);
            Debug.Log(currentLevel);
            levelObjects.Add(currentLevel);
        }
        baseLevel = GameObject.Find("BaseLevelNew");
        if (activeLevelName == "undefined")
        {
            return;
        }
        else
            Debug.LogWarning("The start level is not defined in the Level Manager!");
        GameObject levelObject = GameObject.Find(activeLevelName);
        if (levelObject != null)
        {
            Debug.Log($"Active Level: {activeLevelName}");
            if (levelObject.TryGetComponent(out Level level))
                LoadActiveLevelObjects(level.positionID);
            else
                Debug.LogWarning("Active level has no level script attached!");
            GameObject.Find(activeLevelName).SetActive(true);
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


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
}
