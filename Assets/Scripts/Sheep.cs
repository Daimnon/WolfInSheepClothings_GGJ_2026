using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Sheep : MonoBehaviour, IShootable
{
    [SerializeField] private List<Material> sheepMaterials;
    [SerializeField] private SkinnedMeshRenderer sheepMeshRenderer;
    [SerializeField] private ShootableType _shootableType = ShootableType.Sheep;
    [SerializeField] private Animator _animator;
    
    [Header("Movements")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float _moveSpeed = 2.0f;
    [SerializeField] private float _minMoveDistance = 2.0f;
    [SerializeField] private float _maxMoveDistance = 5.0f;
    [SerializeField] private float _fearSpeed = 6.0f;
    [SerializeField] private float _fearRadius = 6.0f;
    [SerializeField] private float _chaosFactor = 1.5f;

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
    
    [SerializeField] private GameObject bloodPuddle;

    public bool isAlive = true;

    private void Awake()
    {
        GameManager.OnSheepKilled += RunChaotically;
    }

    private void Start()
    {
        _agent.speed = _moveSpeed;
        int randInt = Random.Range(0, 2);

        if (randInt == 0) SetNewDestination();
        StartWandering();
    }

    #region IShootable
    public GameObject GetGameObj()
    {
        return gameObject;
    }
    public void GotShot()
    {
        OnShepperdKilledSheep();
        
    }
    public ShootableType GetShootableType()
    {
        return _shootableType;
    }
    public bool CanBeTargeted => true;
    public void NotifyPuddleEnter(PuddleType puddleType)
    {
        switch (puddleType)
        {
            case PuddleType.Blood:
                SetStained();
                break;
            case PuddleType.Water:
                SetUnStained();
                break;
        }
    }

    private void SetUnStained()
    {
        _shootableType = ShootableType.Sheep;
        var randomIndex = 0;
        var outline = GetComponent<Outline>();
        outline.enabled = false;
        sheepMeshRenderer.materials = new[] { sheepMaterials[randomIndex]};
        outline.enabled = true;
        outline.enabled = true;
        
    }

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
        _animator.SetBool("IsWalk", true);
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
        if (!isAlive) return false;
        Vector3 center = transform.position;
        return (point - center).sqrMagnitude <= _fearRadius * _fearRadius;
    }
    private void RunChaotically(Vector3 point)
    {
        if (IsPointInsideRadius(point))
        {
            _animator.SetBool("IsScared", true);
            //Debug.Log("Inside Radius");
            Vector3 oppositeDirection = (transform.position - point).normalized;
            float randomChaosY = Random.Range(-_chaosFactor, _chaosFactor);
            var oppositeWithChaos = (Quaternion.Euler(new Vector3(0 ,randomChaosY ,0)) * oppositeDirection).normalized;
            Vector3 newTargetPos = transform.position + GetNewMoveVector(oppositeWithChaos);
            _agent.speed = _fearSpeed;
            _agent.SetDestination(newTargetPos);
            StartStuckCheck();
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
        _animator.SetBool("IsScared", false);
        _animator.SetBool("IsWalk", false);
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
        if (_shootableType == ShootableType.BloodySheep) return;
        _shootableType = ShootableType.BloodySheep;
        var randomIndex = Random.Range(1, sheepMaterials.Count-1);
        var outline = GetComponent<Outline>();
        outline.enabled = false;
        sheepMeshRenderer.materials = new[] { sheepMaterials[randomIndex]};
        outline.enabled = true;
    }
    #endregion

    public void Die()
    {
        GameManager.OnSheepKilled?.Invoke(transform.position);
        GameManager.UpdateSheepCount?.Invoke();
        _agent.isStopped = true;
        _agent.enabled = false;
        isAlive = false;
        StopAllCoroutines();
        Destroy(gameObject);
        Instantiate(bloodPuddle, new Vector3(transform.position.x, 0, transform.position.z), transform.rotation);
    }

    private void OnShepperdKilledSheep()
    {
        _agent.isStopped = true;
        _agent.enabled = false;
        isAlive = false;
        StopAllCoroutines();
        Destroy(gameObject);
        Instantiate(bloodPuddle, transform.position, transform.rotation);
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
}
