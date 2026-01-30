using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private float radius;
    [SerializeField] private LayerMask groundMask;

    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
        {
            var targetPosition = hit.point;
            targetPosition.y = 0f;

            var playerPosition = new Vector3(player.position.x, 0f, player.position.z);
            var direction = targetPosition - playerPosition;

            if (direction.magnitude > radius)
            {
                direction = direction.normalized * radius;
            }

            var finalPosition = playerPosition + direction;
            transform.position = Vector3.Lerp(transform.position, finalPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
