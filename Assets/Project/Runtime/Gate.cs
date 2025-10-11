using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class Gate : MonoBehaviour
{
    public int health = 10;

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
        }
    }

}