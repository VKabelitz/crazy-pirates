using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Health : MonoBehaviour, IHealth
{
    [SerializeField]
    public int maxHealth = 50;
    public int HealthPoints { get; set; }

    public void Awake()
    {
        HealthPoints = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        HealthPoints -= damage;
        if (HealthPoints <= 0)
        {
            DestroyObject();
        }
    }

    public void Heal(int health)
    {
        HealthPoints = Mathf.Min(HealthPoints + health, maxHealth);
    }

    public IEnumerator FadeAndDestroy(GameObject gameObject, float duration)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        List<Material> mats = new List<Material>();
        foreach (var r in renderers)
            mats.AddRange(r.materials);

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / duration);

            foreach (var m in mats)
            {
                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    public void DestroyObject()
    {
        Debug.Log("Object destroyed: " + gameObject.name, gameObject);
        if (gameObject.TryGetComponent(out IPoolable poolable))
            poolable.ReturnToPool();
        else
        {        
            if (TryGetComponent<Tower>(out var tower))
            {
                StartCoroutine(tower.FadeAndDestroy());
                return;
            }
            Destroy(gameObject);
        }
    }
}
