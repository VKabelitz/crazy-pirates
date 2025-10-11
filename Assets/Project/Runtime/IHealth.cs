using UnityEngine;

public interface IHealth
{
    public int HealthPoints { get; set; }

    public void TakeDamage(int damage) { }
    public void Heal(int health) { }
    public void DestroyObject() { }

}
