using UnityEngine;

public interface Tower 
{
    public int damage;
    public int range;
    public bool attackingEnemy;

    public void Attack();
}
