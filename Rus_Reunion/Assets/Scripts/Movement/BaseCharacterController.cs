using System;
using UnityEngine;

public class BaseCharacterController : MonoBehaviour
{
    /// <summary>Скорость движения в метрах в секунду</summary>
    [SerializeField] private float moveSpeed;
    [SerializeField] private float speedMultiplier;
    [SerializeField] private GroundCheckCasting groundCheck;
    [SerializeField] private float gravitationForce;
    [SerializeField] private float stepHeight = 0.4f;    // Максимальная высота ступеньки
    [SerializeField] private float stepForwardDist = 0.3f; // Дистанция вперёд для проверки препятствия
    [SerializeField] private float stepCheckDown = 0.1f;     // Дистанция вниз для проверки земли за ступенькой
    private Rigidbody rb;
    private float smoothTime = 0f;
    private float defaultSpeed;
    private Vector3 planeNormal;
    private Vector3 totalOffsetForMovement;
    private bool isGrounded;
    private const float MovementThresholdSqr = 0.01f;

    private void Start()
    {
        defaultSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
    }

    public void ChangeSpeed(bool isPressed)
    {
        moveSpeed = isPressed ? defaultSpeed * speedMultiplier : defaultSpeed;
    }

    public void MoveCharacter(Vector3 direction)
    {
        isGrounded = groundCheck.CheckGround(ref planeNormal);
        totalOffsetForMovement = Vector3.zero;

        if (!isGrounded)
            totalOffsetForMovement += Vector3.down * gravitationForce * Time.deltaTime;

        if (isGrounded)
        {
            var projectedDirection = Vector3.ProjectOnPlane(direction, planeNormal);
            Debug.Log("Angle " + projectedDirection);

            if (projectedDirection.sqrMagnitude > MovementThresholdSqr)
            {
                projectedDirection = projectedDirection.normalized;
                totalOffsetForMovement += projectedDirection * (moveSpeed * Time.deltaTime);

                var angle = Mathf.Atan2(projectedDirection.x, projectedDirection.z) * Mathf.Rad2Deg;
                var smooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref smoothTime, 0.1f);
                rb.MoveRotation(Quaternion.Euler(0, smooth, 0));
            }
        }

        HandleStepping(ref totalOffsetForMovement);

        rb.MovePosition(rb.position + totalOffsetForMovement);
    }

    private void HandleStepping(ref Vector3 movement)
    {
        // Работает только если на земле и есть движение
        if (!isGrounded || movement.magnitude < 0.1f) return;

        Vector3 origin = groundCheck.GroundCheck != null
            ? groundCheck.GroundCheck.position
            : rb.position;

        // Направление движения (горизонтальное)
        Vector3 forward = new Vector3(movement.x, 0, movement.z).normalized;
        if (forward.sqrMagnitude < 0.1f) return;

        // 1. Проверяем, есть ли препятствие впереди
        if (Physics.Raycast(origin, forward, out RaycastHit hit, stepForwardDist, groundCheck.GroundLayer))
        {
            // 2. Проверяем, насколько высоко оно
            Vector3 upCheckPos = origin + Vector3.up * stepHeight;
            if (Physics.Raycast(upCheckPos, forward, stepForwardDist, groundCheck.GroundLayer))
            {
                // Препятствие слишком высокое — не лезем
                return;
            }

            // 3. Проверяем, есть ли земля над препятствием (чтобы не лезть в потолок)
            Vector3 topPos = hit.point + Vector3.up * stepHeight;
            if (!Physics.Raycast(topPos, Vector3.down, out RaycastHit downHit, stepHeight + stepCheckDown, groundCheck.GroundLayer))
            {
                return;
            }

            // 4. Если земля близко сверху — можно подняться
            if (downHit.distance <= stepHeight + stepCheckDown)
            {
                // Поднимаемся на высоту ступеньки
                movement += Vector3.up * (stepHeight - 0.1f); // 0.1f — небольшой запас
            }
        }
    }
}
