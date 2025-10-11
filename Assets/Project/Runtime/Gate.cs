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
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (gameObject.TryGetComponent(out Health health))
            {
                health.TakeDamage(other.gameObject.GetComponent<Enemy>().collisionDamage);
                if (other.gameObject.TryGetComponent(out IPoolable poolable))
                {
                    poolable.ReturnToPool();
                }
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
    }

}