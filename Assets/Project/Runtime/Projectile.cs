using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private int damage;
    public Transform target;
    private ObjectPool projectilePool = new ObjectPool();

    public void Move(float horizontal, float vertical)
    {
        transform.position = Vector3.MoveTowards(transform.position, target.position, movementSpeed* Time.deltaTime);
    }
        

    public void OnActivate()
    {
        gameObject.SetActive(true);
    }

    public void OnDeactivate()
    {
       gameObject.SetActive(false);
    }

    public void ReturnToPool()
    {
        projectilePool.ReturnToPool(gameObject);
    }

    public void SetPool(ObjectPool pool)
    {
        projectilePool = pool;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.tag == "Enemy")
        {
          
            if (other.gameObject.TryGetComponent(out Health hitObjectHealth))
            {
                
                hitObjectHealth.TakeDamage(damage);
                
            }
            ReturnToPool(); //Projektil soll dem Pool zurückgegeben werden
        }
        
    }

    public void Update()
    {

        Move(1f, 0f);

        if (transform.position.x > 10f)
        {
            ReturnToPool();
        }
    }

}
