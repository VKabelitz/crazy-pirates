using UnityEngine;

public class LinearMovement : BaseMovement, IMovable
{
    [SerializeField]
    private float movementSpeed = 1f;

    public void FixedUpdate()
    {
        Move(1f, 0f, 0f);
    }

    public void Move(float horizontal, float vertical, float depth)
    {
        Vector3 moveDirection = new Vector3(horizontal, vertical, depth).normalized;
        transform.position += moveDirection * Time.fixedDeltaTime * movementSpeed;
    }
}
