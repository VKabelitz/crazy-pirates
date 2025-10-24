using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BasicTower : Tower
{

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Attack()
    {
        base.Attack();
    }

    protected override void FaceTarget()
    {
        Vector3 targetPos = currentTarget.transform.position;

        // --- YAW: Horizontal rotation around Y axis ---
        Vector3 yawDirection = targetPos - YawWheel.transform.position;
        yawDirection.y = 0; // Ignore vertical component

        if (yawDirection.sqrMagnitude > 0.001f)
        {
            // Calculate target yaw angle
            float targetYaw =
                Mathf.Atan2(yawDirection.x, yawDirection.z) * Mathf.Rad2Deg + 180f;

            // Get current yaw angle
            float currentYaw = YawWheel.transform.eulerAngles.y;

            // Smoothly interpolate yaw
            float smoothYaw = Mathf.LerpAngle(
                currentYaw,
                targetYaw,
                Time.deltaTime * rotationSpeed
            );

            // Apply rotation (preserve other axes if needed)
            YawWheel.transform.rotation = Quaternion.Euler(0f, smoothYaw, 90.0f);
        }
    }
}
