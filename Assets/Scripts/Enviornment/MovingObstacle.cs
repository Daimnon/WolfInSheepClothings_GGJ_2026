using UnityEngine;

public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float distance = 3f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }   

    void Update()
    {
        float offset = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}