using UnityEngine;

public interface IHealth
{

    public void TakeDamage(int damage) { }
    public void Heal(int health) { }
    public void DestroyObject() { }

}
