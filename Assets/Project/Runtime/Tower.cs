using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    public int damage;
    public int range;
    public bool attackingEnemy;

    public virtual void Attack()
    {
        //Attack wenn ein Gegner in Reichweite ist
        if (!attackingEnemy)
        {
            Debug.Log("Tower greift an!");
            attackingEnemy = true;
        }
    }
}
