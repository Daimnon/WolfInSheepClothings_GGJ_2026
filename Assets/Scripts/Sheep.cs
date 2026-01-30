using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour, IShootable
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float _moveSpeed = 4.0f;
    [SerializeField] private float _minMoveDistance = 2.0f;
    [SerializeField] private float _maxMoveDistance = 5.0f;
    [SerializeField] private float _fearSpeed = 8.0f;
    [SerializeField] private float _fearRadius = 3.0f;

    [Header("Debugging")]
    [SerializeField] private Transform _testPoint;
    private Vector3 _moveVector;

    private void Start()
    {
        _agent.speed = _moveSpeed;
    }

    #region IShootable
    public GameObject GetGameObj()
    {
        return gameObject;
    }
    public void GotShot()
    {
        Debug.Log("GotShot");
    }
    #endregion

    #region Set New Destination
    private Vector3 SetNewDirection()
    {
        float randX = Random.Range(-1.0f, 1.0f);
        float randY = Random.Range(-1.0f, 1.0f);
        return new Vector3(randX, 0.0f, randY).normalized;
    }

    [ContextMenu("SetNewMoveVector")]
    private Vector3 SetNewMoveVector(Vector3 direction)
    {
        float newMagnitude = Random.Range(_minMoveDistance, _maxMoveDistance);
        Vector3 newTargetPos = direction * newMagnitude;
        _moveVector = newTargetPos;
        return newTargetPos;
    }

    [ContextMenu("SetNewDestination")]
    private void SetNewDestination()
    {
        Debug.Log("New Destination");
        Vector3 newTargetPos = transform.position + SetNewMoveVector(SetNewDirection());
        _agent.SetDestination(newTargetPos);
    }
    #endregion

    #region CheckForFear
    private bool IsPointInsideRadius(Vector3 point)
    { 
        Vector3 center = transform.position;
        return (point - center).sqrMagnitude <= _fearRadius * _fearRadius;
    }
    private void RunChaotically(Vector3 point)
    {
        if (IsPointInsideRadius(point))
        {
            Debug.Log("Inside Radius");
            Vector3 oppositeDirection = (transform.position - point).normalized;
            Vector3 newTargetPos = transform.position + SetNewMoveVector(oppositeDirection);
            _agent.speed = _fearSpeed;
            _agent.SetDestination(newTargetPos);
        }
        else
        {
            Debug.Log("Not Inside Radius");
        }
    }

    [ContextMenu("RunChaotically")]
    private void RunChaotically()
    {
        if (IsPointInsideRadius(_testPoint.position))
        {
            Debug.Log("Inside Radius");
            Vector3 oppositeDirection = (transform.position - _testPoint.position).normalized;
            Vector3 newTargetPos = transform.position + SetNewMoveVector(oppositeDirection);
            _agent.speed = _fearSpeed;
            _agent.SetDestination(newTargetPos);
        }
        else
        {
            Debug.Log("Not Inside Radius");
        }
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (_testPoint == null)
            return;

        Vector3 center = transform.position;
        Vector3 point = _testPoint.position;

        bool isInside = (point - center).sqrMagnitude <= _fearRadius * _fearRadius;

        // Draw the radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, _fearRadius);

        // Draw line to the point
        Gizmos.color = isInside ? Color.green : Color.red;
        Gizmos.DrawLine(center, point);

        // Draw the point
        Gizmos.DrawSphere(point, 0.1f);
    }

    /*private IEnumerator MoveToTarget()
    {
        Vector2 endPos = (Vector2)transform.position + _targetPos;

        while (true)
        {
            Vector2 currentPos = transform.position;
            Vector2 newPos = Vector2.MoveTowards(currentPos,endPos,_moveSpeed * Time.deltaTime);
            transform.position = newPos;

            if ((endPos - newPos).sqrMagnitude <= 0.0001f)
            {
                transform.position = endPos;
                yield break;
            }

            yield return null;
        }
    }*/
}
