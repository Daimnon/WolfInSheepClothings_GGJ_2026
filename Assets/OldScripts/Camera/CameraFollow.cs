using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private Vector3 offset;

    void LateUpdate()
    {
        if (targetA != null && targetB != null)
        {
            var midpoint = (targetA.position + targetB.position) / 2f;
            
            var desiredPosition = midpoint + offset;
            
            var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }
    }
}