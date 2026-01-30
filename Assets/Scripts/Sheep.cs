using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour, IShootable
{
    [SerializeField] private ShootableType _shootableType = ShootableType.Sheep;
    
    [Header("Movements")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float _moveSpeed = 2.0f;
    [SerializeField] private float _minMoveDistance = 2.0f;
    [SerializeField] private float _maxMoveDistance = 5.0f;
    [SerializeField] private float _fearSpeed = 6.0f;
    [SerializeField] private float _fearRadius = 6.0f;

    [Header("Unstuck Settings")]
    [SerializeField] private float _stuckCheckInterval = 1.0f;
    [SerializeField] private float _minDistanceProgress = 0.15f;
    private Coroutine _stuckCoroutine;

    [Header("Wander Settings")]
    [SerializeField] private float _minWanderDelay = 2.0f;
    [SerializeField] private float _maxWanderDelay = 10.0f;
    private Coroutine _wanderCoroutine;

    [Header("Debugging")]
    [SerializeField] private Transform _testPoint;
    private Vector3 _moveVector;

    private void Start()
    {
        _agent.speed = _moveSpeed;
        StartWandering();
    }

    #region IShootable
    public GameObject GetGameObj()
    {
        return gameObject;
    }
    public void GotShot()
    {
        Debug.Log("GotShot");
        Die();
    }
    public ShootableType GetShootableType()
    {
        return _shootableType;
    }
    public bool CanBeTargeted => true;
    #endregion

    #region Set New Destination
    private Vector3 SetNewDirection()
    {
        float randX = Random.Range(-1.0f, 1.0f);
        float randY = Random.Range(-1.0f, 1.0f);
        return new Vector3(randX, 0.0f, randY).normalized;
    }

    [ContextMenu("GetNewMoveVector")]
    private Vector3 GetNewMoveVector(Vector3 direction)
    {
        float newMagnitude = Random.Range(_minMoveDistance, _maxMoveDistance);
        Vector3 newTargetPos = direction * newMagnitude;
        _moveVector = newTargetPos;
        return newTargetPos;

    }

    [ContextMenu("SetNewDestination")]
    private void SetNewDestination()
    {
        //Debug.Log("New Destination");
        Vector3 newTargetPos = transform.position + GetNewMoveVector(SetNewDirection());
        _agent.SetDestination(newTargetPos);
        StartStuckCheck();
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
            //Debug.Log("Inside Radius");
            Vector3 oppositeDirection = (transform.position - point).normalized;
            Vector3 newTargetPos = transform.position + GetNewMoveVector(oppositeDirection);
            _agent.speed = _fearSpeed;
            _agent.SetDestination(newTargetPos);
            StartStuckCheck();
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
            //Debug.Log("Inside Radius");
            Vector3 oppositeDirection = (transform.position - _testPoint.position).normalized;
            Vector3 newTargetPos = transform.position + GetNewMoveVector(oppositeDirection);
            _agent.speed = _fearSpeed;
            _agent.SetDestination(newTargetPos);
        }
        else
        {
            Debug.Log("Not Inside Radius");
        }
    }
    #endregion

    #region Unstuck
    private IEnumerator CheckIfStuckRoutine()
    {
        Vector3 lastPosition = transform.position;

        while (true)
        {
            yield return new WaitForSeconds(_stuckCheckInterval);

            if (!_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance)
            {
                StopStuckCheck();
                yield break;
            }

            float movedDistance = (transform.position - lastPosition).magnitude;

            if (movedDistance < _minDistanceProgress)
            {
                OnStuck();
                yield break;
            }

            lastPosition = transform.position;
        }
    }
    private void StartStuckCheck()
    {
        StopStuckCheck();
        _stuckCoroutine = StartCoroutine(CheckIfStuckRoutine());
    }
    private void StopStuckCheck()
    {
        if (_stuckCoroutine != null)
        {
            StopCoroutine(_stuckCoroutine);
            _stuckCoroutine = null;
        }
    }
    private void OnStuck()
    {
        //Debug.Log("Sheep is stuck, recovering.");
        _agent.ResetPath();
    }
    #endregion

    #region Wander
    private void StartWandering()
    {
        StopWandering();
        _wanderCoroutine = StartCoroutine(WanderRoutine());
    }
    private void StopWandering()
    {
        if (_wanderCoroutine != null)
        {
            StopCoroutine(_wanderCoroutine);
            _wanderCoroutine = null;
        }
    }
    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(_minWanderDelay, _maxWanderDelay);
            yield return new WaitForSeconds(waitTime);

            // If sheep is currently moving or fleeing, skip this tick
            if (_agent.hasPath)
                continue;

            // Normal wander movement
            _agent.speed = _moveSpeed;
            SetNewDestination();
        }
    }
    #endregion

    #region Staining
    public void SetStained()
    {
        _shootableType = ShootableType.BloodySheep;
    }
    #endregion

    private void Die()
    {
        GameManager.OnSheepKilled.Invoke(transform.position);
        GameManager.UpdateSheepCount.Invoke();
        _agent.enabled = false;
    }

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
