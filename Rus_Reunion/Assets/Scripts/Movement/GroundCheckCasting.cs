using UnityEngine;

public class GroundCheckCasting : MonoBehaviour
{
    [Tooltip("Точка, из которой идёт проверка. Должна быть у ног персонажа.")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Максимальное расстояние до земли (например, 0.2 для небольшого зазора).")]
    [SerializeField] private float checkDistance = 0.2f;

    [Tooltip("Слой, на котором находится земля (например, 'Ground').")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Радиус для более надёжной проверки (используется SphereCast, если > 0).")]
    [SerializeField] private float checkRadius = 0.1f; // Можно оставить 0 для чистого Raycast

    private bool isGrounded;
    public bool IsGrounded => isGrounded; // Для доступа извне
    
    public Transform GroundCheck => groundCheck;
    public LayerMask GroundLayer => groundLayer;

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Vector3 origin = groundCheck.position;
        Vector3 end = origin + Vector3.down * checkDistance;

        Gizmos.color = isGrounded ? Color.green : Color.red;

        if (checkRadius > 0)
        {
            // Визуализация как сфера + луч
            Gizmos.DrawWireSphere(origin, checkRadius);
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, checkRadius);
        }
        else
        {
            // Простой луч
            Gizmos.DrawRay(origin, Vector3.down * checkDistance);
        }
    }

    /// <summary>
    /// Проверяет, находится ли персонаж на земле, и обновляет нормаль.
    /// </summary>
    /// <returns>True, если на земле</returns>
    public bool CheckGround(ref Vector3 planeNormal)
    {
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck не назначен в инспекторе!");
            return false;
        }

        Vector3 origin = groundCheck.position;

        // Используем SphereCast, если задан радиус, иначе Raycast
        if (checkRadius > 0f)
        {
            if (Physics.SphereCast(origin, checkRadius, Vector3.down, out RaycastHit hit, checkDistance, groundLayer))
            {
                planeNormal = hit.normal;
                isGrounded = true;
                return true;
            }
        }
        else
        {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, checkDistance, groundLayer))
            {
                planeNormal = hit.normal;
                isGrounded = true;
                return true;
            }
        }

        // Если не попали — не на земле
        isGrounded = false;
        return false;
    }

}
