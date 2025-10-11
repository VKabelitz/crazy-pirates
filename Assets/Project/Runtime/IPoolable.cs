using UnityEngine;

public interface IPoolable
{
    public void SetPool(ObjectPool pool);
    public void ReturnToPool();

    public void OnActivate();
    void OnDeactivate();
}
