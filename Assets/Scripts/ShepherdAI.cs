using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class ShepherdAI : MonoBehaviour
{
    private readonly int IdleHash = Animator.StringToHash("Animation_Idle");
    private readonly int WalkHash = Animator.StringToHash("Animation_Walk");
    private readonly int AimHash = Animator.StringToHash("Animation_Aiming");
    private readonly int AttackHash = Animator.StringToHash("Animation_Recoil");
    
    private const float CrossFadeDuration = 0.1f;
    
    [SerializeField] private Animator animator;

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player; // The Wolf
    [SerializeField] private Collider farmZone;
    [SerializeField] private LayerMask shootableLayer;

    [Header("State Timers")]
    [SerializeField] private float idleTimer = 3f;
    [SerializeField] private float observeTimer = 2f;

    [Header("Running Fail-safe")]
    [SerializeField] private float runningTimeout = 8f;
    [SerializeField] private float stuckTimeout = 1.5f;

    [Header("Chase")]
    [SerializeField] private float maxChaseTime = 10f;     // base chase time
    [SerializeField] private float minChaseMultiplier = 0.5f;
    [SerializeField] private float maxChaseMultiplier = 2.0f;

    [Header("Aggro Settings")]
    [SerializeField] private float aggroIncreasePerBody = 15f;
    [SerializeField] private float aggroDecayRate = 2f; // Per second when wolf hidden OR outside farm
    [SerializeField] private float minSpeed = 3.5f;
    [SerializeField] private float maxSpeed = 7f;

    [Header("Combat")]
    [SerializeField] private float shootingRadius = 15f;
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float shotDamage = 100f;
    [SerializeField] private float frenzyDuration = 8f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolTargets;

    // State
    private ShepherdBehaviour currentState;
    private Vector3 startPosition;

    // ONLY wolf kills should be added here
    private readonly List<Vector3> deadSheepPositions = new List<Vector3>();

    // Track which corpse positions already gave aggro (award once per corpse)
    private readonly HashSet<Vector3Int> aggroAwardedBodies = new HashSet<Vector3Int>();

    private IShootable currentTarget;
    private float aggroMeter = 0f;
    public float AggroMeter => aggroMeter;

    private float stateTimer = 0f;
    private float chaseTimer = 0f;

    private bool isInFrenzy = false;
    private int currentPoint = 0;
    private float observingAngle = 0f;

    private bool shotConsumed;

    // Running fail-safe internal
    private float runningTimer = 0f;
    private float stuckTimer = 0f;
    private float lastDistanceToBody = Mathf.Infinity;

    private void OnEnable()
    {
        GameManager.OnSheepKilled += OnSheepKilledHandler;
    }

    private void OnDisable()
    {
        GameManager.OnSheepKilled -= OnSheepKilledHandler;
    }

    void Start()
    {
        startPosition = transform.position;

        if (patrolTargets != null && patrolTargets.Length > 0)
            agent.SetDestination(patrolTargets[0].position);

        ChangeState(ShepherdBehaviour.Idle);
    }

    void Update()
    {
        // Aggro decay (rule B): decay if wolf is hidden OR outside farm
        if (aggroMeter > 0f && (IsWolfInBush() || !CheckIfWolfIsInFarm()))
        {
            aggroMeter = Mathf.Max(0f, aggroMeter - aggroDecayRate * Time.deltaTime);
        }

        UpdateSpeed();

        // Aggro > 0 chase rule (wolf identifiable only)
        if (aggroMeter > 0f && currentState != ShepherdBehaviour.Shooting)
        {
            IShootable wolf = ScanForWolf();
            if (wolf != null && !IsWolfInBush() && aggroMeter >= 50f)
            {
                if (currentState != ShepherdBehaviour.Chasing)
                {
                    currentTarget = wolf;
                    ChangeState(ShepherdBehaviour.Chasing);
                    return;
                }
            }
        }

        switch (currentState)
        {
            case ShepherdBehaviour.Idle: IdleState(); break;
            case ShepherdBehaviour.Observing: ObservingState(); break;
            case ShepherdBehaviour.Running: RunningState(); break;
            case ShepherdBehaviour.Chasing: ChasingState(); break;
            case ShepherdBehaviour.Shooting: ShootingState(); break;
            case ShepherdBehaviour.AggroScouting: AggroScoutingState(); break;
            case ShepherdBehaviour.Returning: ReturningState(); break;
        }
    }

    #region State Handlers

    private void IdleState()
    {
        stateTimer -= Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            GoToNextPoint();

        if (deadSheepPositions.Count > 0)
        {
            ChangeState(ShepherdBehaviour.Running);
            return;
        }

        if (stateTimer <= 0)
            ChangeState(ShepherdBehaviour.Observing);
    }

    private void ObservingState()
    {
        float speed = 2.0f;
        float yRotation = Mathf.Sin(Time.time * speed) * observingAngle;
        transform.localRotation = Quaternion.Euler(0, yRotation, 0);

        stateTimer -= Time.deltaTime;

        if (deadSheepPositions.Count > 0)
        {
            ChangeState(ShepherdBehaviour.Running);
            return;
        }

        if (stateTimer <= 0)
            ChangeState(ShepherdBehaviour.Idle);
    }

    private void RunningState()
    {
        runningTimer += Time.deltaTime;

        if (deadSheepPositions.Count == 0)
        {
            ChangeState(aggroMeter >= 50 ? ShepherdBehaviour.AggroScouting : ShepherdBehaviour.Returning);
            return;
        }

        Vector3 bodyPos = deadSheepPositions[0];
        agent.SetDestination(bodyPos);

        // If the navmesh says the path is impossible -> try next body
        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            TryNextBodyOrGiveUp();
            return;
        }

        // Stuck detection: distance not decreasing
        float dist = Vector3.Distance(transform.position, bodyPos);
        if (dist < lastDistanceToBody - 0.05f)
        {
            stuckTimer = 0f;
            lastDistanceToBody = dist;
        }
        else
        {
            stuckTimer += Time.deltaTime;
        }

        if (stuckTimer >= stuckTimeout)
        {
            TryNextBodyOrGiveUp();
            return;
        }

        if (runningTimer >= runningTimeout)
        {
            TryNextBodyOrGiveUp();
            return;
        }

        // Arrived?
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            // Award aggro ONCE per corpse (no streak loss, no double farming)
            AwardAggroOnceForBody(bodyPos);

            // Remove that body (we successfully reached it)
            deadSheepPositions.RemoveAt(0);

            // Reset fail-safe for next target
            ResetRunningFailSafe();

            // Scan for targets
            IShootable target = ScanForTarget();
            if (target != null)
            {
                currentTarget = target;

                if (target.GetShootableType() == ShootableType.Wolf && player != null)
                {
                    float distToWolf = Vector3.Distance(transform.position, player.position);
                    if (distToWolf > shootingRadius)
                    {
                        ChangeState(ShepherdBehaviour.Chasing);
                        return;
                    }
                }

                ChangeState(ShepherdBehaviour.Shooting);
                return;
            }

            if (deadSheepPositions.Count > 0)
                ChangeState(ShepherdBehaviour.Running);
            else
                ChangeState(aggroMeter >= 50 ? ShepherdBehaviour.AggroScouting : ShepherdBehaviour.Returning);
        }
    }

    // Rotate to next corpse instead of deleting the unreachable one.
    // If we've tried enough / only one corpse exists and still can't reach -> give up to scout/return.
    private void TryNextBodyOrGiveUp()
    {
        if (deadSheepPositions.Count <= 1)
        {
            // only one body, nothing else to try
            deadSheepPositions.Clear(); // can't resolve; abandon investigation
            ResetRunningFailSafe();
            ChangeState(aggroMeter >= 50 ? ShepherdBehaviour.AggroScouting : ShepherdBehaviour.Returning);
            return;
        }

        // Move current body to the end (try next)
        Vector3 first = deadSheepPositions[0];
        deadSheepPositions.RemoveAt(0);
        deadSheepPositions.Add(first);

        // Reset timers so he gets a fair attempt at the next body
        ResetRunningFailSafe();

        // Continue running
        agent.SetDestination(deadSheepPositions[0]);
    }

    private void AwardAggroOnceForBody(Vector3 bodyPos)
    {
        // Quantize position to avoid floating point mismatch
        Vector3Int key = new Vector3Int(
            Mathf.RoundToInt(bodyPos.x * 10f),
            Mathf.RoundToInt(bodyPos.y * 10f),
            Mathf.RoundToInt(bodyPos.z * 10f)
        );

        if (aggroAwardedBodies.Contains(key))
            return;

        aggroAwardedBodies.Add(key);

        aggroMeter = Mathf.Min(100f, aggroMeter + aggroIncreasePerBody);
    }

    private void ChasingState()
    {
        if (player == null) return;

        // As requested: chasing clears investigation list
        deadSheepPositions.Clear();

        chaseTimer += Time.deltaTime;

        if (IsWolfInBush() || !CheckIfWolfIsInFarm())
        {
            ChangeState(aggroMeter >= 50 ? ShepherdBehaviour.AggroScouting : ShepherdBehaviour.Returning);
            return;
        }

        float distToWolf = Vector3.Distance(transform.position, player.position);
        if (distToWolf <= shootingRadius)
        {
            currentTarget = player.GetComponent<IShootable>();
            ChangeState(ShepherdBehaviour.Shooting);
            return;
        }

        float chaseLimit = Mathf.Lerp(maxChaseTime * minChaseMultiplier, maxChaseTime * maxChaseMultiplier, aggroMeter / 100f);
        if (chaseTimer > chaseLimit)
        {
            ChangeState(aggroMeter >= 50 ? ShepherdBehaviour.AggroScouting : ShepherdBehaviour.Returning);
            return;
        }

        agent.SetDestination(player.position);
    }

    private void ShootingState()
    {
        if (shotConsumed)
            return;

        IShootable newTarget = ScanForTarget();
        if (newTarget != null)
            currentTarget = newTarget;

        agent.isStopped = true;

        if (currentTarget == null || !currentTarget.CanBeTargeted)
        {
            agent.isStopped = false;
            ChangeState(aggroMeter >= 50 ? ShepherdBehaviour.AggroScouting : ShepherdBehaviour.Returning);
            return;
        }

        Vector3 direction = currentTarget.GetGameObj().transform.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            FireShotgun();
            shotConsumed = true;
            agent.isStopped = false;

            if (aggroMeter >= 100f && !isInFrenzy)
            {
                StartCoroutine(FrenzyMode());
                return;
            }

            if (deadSheepPositions.Count > 0 && aggroMeter < 50)
                ChangeState(ShepherdBehaviour.Running);
            else if (aggroMeter >= 50)
                ChangeState(ShepherdBehaviour.AggroScouting);
            else
                ChangeState(ShepherdBehaviour.Returning);
        }
    }

    private void AggroScoutingState()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomPoint = GetRandomPointInFarm();
            agent.SetDestination(randomPoint);
        }

        if (deadSheepPositions.Count > 0)
        {
            ChangeState(ShepherdBehaviour.Running);
            return;
        }

        IShootable wolf = ScanForWolf();
        if (wolf != null && !IsWolfInBush() && aggroMeter >= 50f)
        {
            currentTarget = wolf;
            ChangeState(ShepherdBehaviour.Chasing);
            return;
        }

        if (aggroMeter < 50)
        {
            ChangeState(deadSheepPositions.Count > 0 ? ShepherdBehaviour.Running : ShepherdBehaviour.Returning);
        }
    }

    private void ReturningState()
    {
        if (deadSheepPositions.Count > 0)
        {
            ChangeState(ShepherdBehaviour.Running);
            return;
        }

        agent.SetDestination(startPosition);
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            ChangeState(ShepherdBehaviour.Idle);
        }
    }

    #endregion

    #region State Transitions

    private void ChangeState(ShepherdBehaviour newState)
    {
        currentState = newState;

        switch (newState)
        {
            case ShepherdBehaviour.Idle:
                animator.CrossFadeInFixedTime(IdleHash, CrossFadeDuration);
                stateTimer = idleTimer;
                agent.isStopped = false;
                shotConsumed = false;
                break;

            case ShepherdBehaviour.Observing:
                animator.CrossFadeInFixedTime(AimHash, CrossFadeDuration);
                stateTimer = observeTimer;
                observingAngle = Random.Range(0f, 180f);
                agent.isStopped = true;
                shotConsumed = false;
                break;

            case ShepherdBehaviour.Running:
                animator.CrossFadeInFixedTime(WalkHash, CrossFadeDuration);
                agent.isStopped = false;
                shotConsumed = false;
                ResetRunningFailSafe();
                if (deadSheepPositions.Count > 0)
                    agent.SetDestination(deadSheepPositions[0]);
                break;

            case ShepherdBehaviour.Chasing:
                animator.CrossFadeInFixedTime(WalkHash, CrossFadeDuration);
                agent.isStopped = false;
                shotConsumed = false;
                chaseTimer = 0f;
                break;

            case ShepherdBehaviour.Shooting:
                animator.CrossFadeInFixedTime(AttackHash, CrossFadeDuration);
                stateTimer = 0.5f;
                shotConsumed = false;
                agent.isStopped = true;
                break;

            case ShepherdBehaviour.AggroScouting:
                animator.CrossFadeInFixedTime(WalkHash, CrossFadeDuration);
                agent.isStopped = false;
                shotConsumed = false;
                break;

            case ShepherdBehaviour.Returning:
                animator.CrossFadeInFixedTime(WalkHash, CrossFadeDuration);
                agent.isStopped = false;
                shotConsumed = false;
                break;
        }
    }

    private void ResetRunningFailSafe()
    {
        runningTimer = 0f;
        stuckTimer = 0f;
        lastDistanceToBody = Mathf.Infinity;
    }

    #endregion

    #region Event Handlers

    private void OnSheepKilledHandler(Vector3 position, KilledBy source)
    {
        // Only wolf kills get added
        if (source != KilledBy.Wolf)
            return;

        // If wolf visible and identifiable, immediate react:
        IShootable wolf = ScanForWolf();
        if (wolf != null && aggroMeter > 0f && !IsWolfInBush() && aggroMeter >= 50f)
        {
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= shootingRadius)
            {
                if (currentState != ShepherdBehaviour.Shooting)
                {
                    currentTarget = wolf;
                    ChangeState(ShepherdBehaviour.Shooting);
                }
            }
            else
            {
                currentTarget = wolf;
                ChangeState(ShepherdBehaviour.Chasing);
            }
            return;
        }

        // Enqueue body (NavMesh-safe)
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            deadSheepPositions.Add(hit.position);
        }

        // Interrupt low priority states to Running
        if (currentState == ShepherdBehaviour.Idle ||
            currentState == ShepherdBehaviour.Observing ||
            currentState == ShepherdBehaviour.Returning ||
            currentState == ShepherdBehaviour.AggroScouting)
        {
            ChangeState(ShepherdBehaviour.Running);
        }
    }

    #endregion

    #region Helper Methods

    void GoToNextPoint()
    {
        if (patrolTargets == null || patrolTargets.Length == 0) return;
        agent.SetDestination(patrolTargets[currentPoint].position);
        currentPoint = (currentPoint + 1) % patrolTargets.Length;
    }

    private IShootable ScanForTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, shootableLayer);
        List<IShootable> potentialTargets = new List<IShootable>();

        foreach (Collider hit in hits)
        {
            IShootable shootable = hit.GetComponent<IShootable>();
            if (shootable == null || !shootable.CanBeTargeted) continue;
            if (IsInBush(hit.transform.position)) continue;
            potentialTargets.Add(shootable);
        }

        if (potentialTargets.Count == 0) return null;
        return SelectTargetWeighted(potentialTargets);
    }

    private IShootable ScanForWolf()
    {
        if (player == null) return null;
        IShootable wolfShootable = player.GetComponent<IShootable>();
        if (wolfShootable != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= detectionRadius)
                return wolfShootable;
        }
        return null;
    }

    private IShootable SelectTargetWeighted(List<IShootable> targets)
    {
        // Wolf priority ONLY if identifiable (aggro>=50) and not hidden.
        foreach (IShootable target in targets)
        {
            if (target.GetShootableType() == ShootableType.Wolf && aggroMeter >= 50f && !IsWolfInBush())
                return target;
        }

        int totalWeight = 0;
        foreach (IShootable target in targets)
        {
            ShootableType type = target.GetShootableType();
            if (type == ShootableType.BloodySheep) totalWeight += 2;
            else if (type == ShootableType.Sheep) totalWeight += 1;
        }

        if (totalWeight == 0) return null;

        int randomNum = Random.Range(1, totalWeight + 1);

        foreach (IShootable target in targets)
        {
            ShootableType type = target.GetShootableType();
            int weight = (type == ShootableType.BloodySheep) ? 2 :
                         (type == ShootableType.Sheep) ? 1 : 0;
            if (weight == 0) continue;

            randomNum -= weight;
            if (randomNum <= 0)
                return target;
        }

        return targets[0];
    }

    private void FireShotgun()
    {
        if (currentTarget == null) return;

        Vector3 origin = transform.position + Vector3.up * 0.8f;
        Vector3 targetPos = currentTarget.GetGameObj().transform.position + Vector3.up * 0.5f;
        Vector3 direction = (targetPos - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, shootingRadius, shootableLayer))
        {
            IShootable hitTarget = hit.collider.GetComponent<IShootable>();
            if (hitTarget != null && hitTarget.CanBeTargeted)
                hitTarget.GotShot();
        }
    }

    private IEnumerator FrenzyMode()
    {
        isInFrenzy = true;
        float frenzyTimer = 0f;

        while (frenzyTimer < frenzyDuration)
        {
            IShootable wolf = ScanForWolf();
            if (wolf != null && aggroMeter >= 50f && !IsWolfInBush())
            {
                currentTarget = wolf;
                FireShotgun();
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, shootableLayer);
                foreach (Collider hit in hits)
                {
                    IShootable shootable = hit.GetComponent<IShootable>();
                    if (shootable == null || !shootable.CanBeTargeted) continue;
                    if (IsInBush(hit.transform.position)) continue;

                    if (shootable.GetShootableType() == ShootableType.BloodySheep)
                    {
                        currentTarget = shootable;
                        FireShotgun();
                        yield return new WaitForSeconds(0.3f);
                    }
                }
            }

            frenzyTimer += Time.deltaTime;
            yield return null;
        }

        aggroMeter = 50f;
        isInFrenzy = false;
        ChangeState(ShepherdBehaviour.AggroScouting);
    }

    private bool IsWolfInBush()
    {
        if (player == null) return false;
        IShootable wolfShootable = player.GetComponent<IShootable>();
        if (wolfShootable == null) return false;
        return wolfShootable.GetShootableType() == ShootableType.Hidden;
    }

    private bool IsInBush(Vector3 position)
    {
        Collider[] bushes = Physics.OverlapSphere(position, 0.5f);
        foreach (Collider col in bushes)
            if (col.GetComponent<Bush>() != null) return true;
        return false;
    }

    private Vector3 GetRandomPointInFarm()
    {
        Bounds bounds = farmZone.bounds;
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            transform.position.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private bool CheckIfWolfIsInFarm()
    {
        if (player == null) return false;
        return farmZone.bounds.Contains(player.position);
    }

    private void UpdateSpeed()
    {
        agent.speed = Mathf.Lerp(minSpeed, maxSpeed, aggroMeter / 100f);
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, shootingRadius);
    }
}

public enum ShepherdBehaviour
{
    Idle,
    Observing,
    Running,
    Chasing,
    Shooting,
    AggroScouting,
    Returning
}