using UnityEngine;

public class LinearMovement : BaseMovement, IMovable
{
    [SerializeField]
    private float movementSpeed = 1f;

    public void FixedUpdate()
    {
        Move(-1f, 0f);
    }

    public void Move(float horizontal, float vertical)
    {
        Vector3 moveDirection = new Vector3(horizontal, vertical, 0).normalized;
        transform.position += moveDirection * Time.fixedDeltaTime * movementSpeed;
    }
}
