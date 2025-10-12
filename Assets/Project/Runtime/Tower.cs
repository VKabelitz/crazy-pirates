using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    public static int range;
    public static bool attackingEnemy;
    public int sprocketCosts;

    public virtual int GetSprocketCosts()
    {
        return sprocketCosts;
    }
}
