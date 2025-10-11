using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyWave", menuName = "SpaceShooter/EnemyWave")]
public class EnemyWave : ScriptableObject
{
    public float waveDuration;
    public List<EnemyWaveEntry> enemies;
}

[System.Serializable]
public class EnemyWaveEntry
{
    public Vector3 spawnPosition;
    public float spawnTime;
    public MovementType movementType;
}
