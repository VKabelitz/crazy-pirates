
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IceBall : Enemy
{
    private float slowScale = 2.0f;
    private float range = 8f;

    public void Update()
    {
        SlowTowers();
    }

    private void SlowTowers()
    {
        Tower[] allTowers = FindObjectsByType<Tower>(FindObjectsSortMode.None);
        foreach (Tower tower in allTowers)
        {
            float distance = Vector3.Distance(transform.position, tower.transform.position);
            if (distance <= range)
            {
                tower.SlowTower(slowScale);
            }
        }

    }
}