using UnityEngine;
using System;

public class Health : MonoBehaviour, IHealth
{
    [SerializeField]
    public int maxHealth = 50;
    public int HealthPoints { get; set; }
    public event Action OnDeath;

    public void Awake()
    {
        HealthPoints = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        HealthPoints -= damage;
        Debug.Log("Health: " + HealthPoints, gameObject);
        if (HealthPoints <= 0)
        {
            OnDeath?.Invoke();
            DestroyObject();
        }
    }

    public void Heal(int health)
    {
        HealthPoints = Mathf.Min(HealthPoints + health, maxHealth);
    }

    public void DestroyObject()
    {
        Debug.Log("Object destroyed: " + gameObject.name, gameObject);
        if (gameObject.TryGetComponent(out IPoolable poolable))
            poolable.ReturnToPool();
        else
            Destroy(gameObject);
    }
}
