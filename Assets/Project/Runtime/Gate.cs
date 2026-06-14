using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class Gate : MonoBehaviour
{
    private Health health;


    public void Awake()
    {
        if (gameObject.TryGetComponent(out Health health))
            this.health = health;
        Debug.Log("Set Health of Gate to " + health.HealthPoints);
    }

    public void OnHit(int damage)
    {
        health.TakeDamage(damage);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (other.TryGetComponent<Enemy>(out Enemy enemy))
            {
                AudioManager.instance.PlaySound("door_damage");
                OnHit(enemy.collisionDamage);
                UIManager.instance.UpdateHealth(enemy.collisionDamage);
                if (other.gameObject.TryGetComponent(out IPoolable poolable))
                {
                    poolable.ReturnToPool();
                }
                Debug.Log("Gate took " + enemy.collisionDamage + " damage!");
                Debug.Log("Gate is now at " + health.HealthPoints + " health!");
            }
            else
            {
                Debug.LogWarning("Enemy has no Health component!");
            }
        }

    }
    private void OnDestroy()
    {
        Debug.Log("Gate destroyed!");
        if (health.HealthPoints <= 0)
            LevelEndMenuLose.instance.activateMenu();
    }

}