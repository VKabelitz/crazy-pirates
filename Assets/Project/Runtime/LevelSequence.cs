using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelSequence", menuName = "SpaceShooter/LevelSequence")]
public class LevelSequence : ScriptableObject
{
    public List<EnemyWave> waves;
}
