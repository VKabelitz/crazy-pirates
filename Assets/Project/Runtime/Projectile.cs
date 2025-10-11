using UnityEngine;

public class Projectile : MonoBehaviour, IMovable, IPoolable
{
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private int damage;
    //public Transform target;
    private ObjectPool projectilePool = new ObjectPool();

    public void Move(float horizontal, float vertical, float depth)
    {
        Vector3 moveDirection = new Vector3(horizontal, vertical, depth).normalized;
        transform.position += moveDirection * Time.deltaTime * movementSpeed;
        //transform.position = Vector3.MoveTowards(transform.position, target.position, movementSpeed* Time.deltaTime);
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
            Debug.Log("Projectile hit enemy");
            ReturnToPool(); //Projektil soll dem Pool zurückgegeben werden
        }
        
    }

    public void Update()
    {
        Move(0f, 0f, -1f);

        if (transform.position.x > 10f)
        {
            ReturnToPool();
        }
    }

}
