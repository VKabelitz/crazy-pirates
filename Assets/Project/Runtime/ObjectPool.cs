using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectPool : MonoBehaviour
{
    [SerializeField]
    GameObject prefab;

    [SerializeField]
    int initialPoolSize = 10;
    bool isInitialized = false;

    private Queue<GameObject> pool = new();

    public void Awake()
    {
        Debug.Log("Awake called, oobject id: " + this.GetInstanceID());
        InitializePool();
    }

    public void InitializePool()
    {
        Debug.Log("Initializing pool " + prefab.name + " with size: " + initialPoolSize);
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject prefabInstance = Instantiate(prefab);
            Assert.IsNotNull(prefab, "Prefab is null!");
            prefabInstance.SetActive(false);
            if (prefabInstance.TryGetComponent(out IPoolable poolable))
            {
                poolable.SetPool(this);
            }
            Assert.IsNotNull(prefabInstance, "Prefab instance is null!");
            pool.Enqueue(prefabInstance);
        }
        isInitialized = true;
    }
    public GameObject GetFromPool()
    {

        Debug.Log("Is initialized: " + isInitialized);
        Debug.Log("Pool count: " + pool.Count);
        Debug.Log("Initial Pool Size: " + initialPoolSize);
        if (!isInitialized)
            InitializePool();
        if (pool.Count > 0)
        {
            GameObject poolObject = pool.Dequeue();
            Debug.Log("Pool: " + pool + ", " + pool.Count);
            Debug.Log("Object taken from pool: " + poolObject);
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
