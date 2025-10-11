using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    GameObject prefab;

    [SerializeField]
    int initialPoolSize = 10;

    private Queue<GameObject> pool = new();

    public void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject prefabInstance = Instantiate(prefab);
            prefabInstance.SetActive(false);
            if (prefabInstance.TryGetComponent(out IPoolable poolable))
            {
                poolable.SetPool(this);
            }
            pool.Enqueue(prefabInstance);
        }
    }

    public GameObject GetFromPool()
    {
        if (pool.Count > 0)
        {
            GameObject poolObject = pool.Dequeue();
            if (poolObject.TryGetComponent(out IPoolable poolable))
            {
                poolable.OnActivate();
            }

            return poolObject;
        }
        else
        {
            GameObject prefabInstance = Instantiate(prefab);
            if (prefabInstance.TryGetComponent(out IPoolable poolable))
            {
                poolable.SetPool(this);
                poolable.OnActivate();
            }
            return prefabInstance;
        }
    }

    public void ReturnToPool(GameObject poolObject)
    {
        if (poolObject.TryGetComponent(out IPoolable poolable))
        {
            poolable.OnDeactivate();
        }
        pool.Enqueue(poolObject);
    }

}
