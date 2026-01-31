using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ShepherdAI : MonoBehaviour
{
    [SerializeField] private TMP_Text stateText;
    
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player; // The Wolf
    [SerializeField] private Collider farmZone;
    [SerializeField] private Collider coneCollider;
    [SerializeField] private LayerMask shootableLayer;

    [Header("State Timers")]
    [SerializeField] private float idleTimer = 3f;
    [SerializeField] private float observeTimer = 2f;
    [SerializeField] private float maxChaseTime = 10f;

    [Header("Aggro Settings")]
    [SerializeField] private float aggroIncreasePerBody = 15f;
    [SerializeField] private float aggroDecayRate = 2f; // Per second when in bush
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
    private List<Vector3> deadSheepPositions = new List<Vector3>();
    private IShootable currentTarget;
    private float aggroMeter = 0f;
    public float AggroMeter => aggroMeter;
    private float stateTimer = 0f;
    private float chaseTimer = 0f;
    private bool isInFrenzy = false;
    private int currentPoint = 0;
    private float observingAngle = 0f;

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
        agent.SetDestination(patrolTargets[0].position);
        ChangeState(ShepherdBehaviour.Idle);
    }

    void Update()
    {
        // Aggro decay when Wolf is in bush
        if (IsWolfInBush() && aggroMeter > 0 || !CheckIfWolfIsInFarm() && aggroMeter > 0)
        {
            aggroMeter -= aggroDecayRate * Time.deltaTime;
            aggroMeter = Mathf.Max(0, aggroMeter);
        }

        // Update speed based on aggro
        UpdateSpeed();

        // State machine
        switch (currentState)
        {
            case ShepherdBehaviour.Idle:
                IdleState();
                break;
            case ShepherdBehaviour.Observing:
                ObservingState();
                break;
            case ShepherdBehaviour.Running:
                RunningState();
                break;
            case ShepherdBehaviour.Chasing:
                ChasingState();
                break;
            case ShepherdBehaviour.Shooting:
                ShootingState();
                break;
            case ShepherdBehaviour.AggroScouting:
                AggroScoutingState();
                break;
            case ShepherdBehaviour.Returning:
                ReturningState();
                break;
        }
        
        stateText.text = currentState.ToString();
    }

    #region State Handlers

    private void IdleState()
    {
        stateTimer -= Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            GoToNextPoint();
        
        if (stateTimer <= 0)
        {
            ChangeState(ShepherdBehaviour.Observing);
        }
    }

    private void ObservingState()
    {
        float speed = 2.0f; // How fast it swings
        float yRotation = Mathf.Sin(Time.time * speed) * observingAngle;
        transform.localRotation = Quaternion.Euler(0, yRotation, 0);

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            ChangeState(ShepherdBehaviour.Idle);
        }
    }

    private void RunningState()
    {
        if (deadSheepPositions.Count == 0)
        {
            ChangeState(aggroMeter >= 50 ? ShepherdBehaviour.AggroScouting : ShepherdBehaviour.Returning);
            return;
        }

        // Check if reached destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Arrived at body
            deadSheepPositions.RemoveAt(0);
            aggroMeter += aggroIncreasePerBody;
            aggroMeter = Mathf.Min(100, aggroMeter);

            // Scan for targets
            IShootable target = ScanForTarget();

            if (target != null)
            {
                currentTarget = target;

                if (target.GetShootableType() == ShootableType.Wolf)
                {
                    ChangeState(ShepherdBehaviour.Chasing);
                }
                else
                {
                    ChangeState(ShepherdBehaviour.Shooting);
                }
            }
            else
            {
                // No target found at body
                if (deadSheepPositions.Count > 0)
                {
                    // More bodies to investigate, go to next one
                    agent.SetDestination(deadSheepPositions[0]);
                    // Stay in Running state
                }
                else if (aggroMeter >= 50)
                {
                    // No more bodies, high aggro -> scout for Wolf
                    ChangeState(ShepherdBehaviour.AggroScouting);
                }
                else
                {
                    // No more bodies, low aggro -> return home
                    ChangeState(ShepherdBehaviour.Returning);
                }
            }
        }
    }

    private void ChasingState()
    {
        if (player == null) return;
        
        deadSheepPositions.Clear();

        chaseTimer += Time.deltaTime;

        // Check if Wolf entered bush
        if (IsWolfInBush() || !CheckIfWolfIsInFarm())
        {
            ChangeState(ShepherdBehaviour.AggroScouting);
            return;
        }

        // Check if in shooting range
        float distToWolf = Vector3.Distance(transform.position, player.position);
        if (distToWolf <= shootingRadius)
        {
            currentTarget = player.GetComponent<IShootable>();
            ChangeState(ShepherdBehaviour.Shooting);
            return;
        }

        // Check chase timeout
        if (chaseTimer > maxChaseTime)
        {
            if (aggroMeter > 50)
            {
                ChangeState(ShepherdBehaviour.AggroScouting);
            }
            else
            {
                ChangeState(ShepherdBehaviour.Returning);
            }
            return;
        }

        // Keep chasing
        agent.SetDestination(player.position);
    }

    private void ShootingState()
    {
        agent.isStopped = true;

        if (currentTarget == null || !currentTarget.CanBeTargeted)
        {
            agent.isStopped = false;
            ChangeState(ShepherdBehaviour.AggroScouting);
            return;
        }

        // Rotate towards target
        Vector3 direction = currentTarget.GetGameObj().transform.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        stateTimer -= Time.deltaTime;

        // Fire after aim time
        if (stateTimer <= 0)
        {
            FireShotgun();
            agent.isStopped = false;

            // Check for frenzy mode
            if (aggroMeter >= 100 && !isInFrenzy)
            {
                StartCoroutine(FrenzyMode());
            }
            else
            {
                // Check dead sheep queue first (low aggro priority)
                if (deadSheepPositions.Count > 0 && aggroMeter < 50)
                {
                    ChangeState(ShepherdBehaviour.Running);
                }
                else if (aggroMeter >= 50)
                {
                    ChangeState(ShepherdBehaviour.AggroScouting);
                }
                else
                {
                    ChangeState(ShepherdBehaviour.Returning);
                }
            }
        }
    }

    private void AggroScoutingState()
    {
        // Move to random positions in farm
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomPoint = GetRandomPointInFarm();
            agent.SetDestination(randomPoint);
        }

        // Continuously scan for Wolf
        IShootable wolf = ScanForWolf();
        if (wolf != null && !IsWolfInBush())
        {
            currentTarget = wolf;
            ChangeState(ShepherdBehaviour.Chasing);
        }

        // Exit aggro scout if aggro drops below 50
        if (aggroMeter < 50)
        {
            if (deadSheepPositions.Count > 0)
            {
                ChangeState(ShepherdBehaviour.Running);
            }
            else
            {
                ChangeState(ShepherdBehaviour.Returning);
            }
        }
    }

    private void ReturningState()
    {
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
                stateTimer = idleTimer;
                agent.isStopped = false;
                break;

            case ShepherdBehaviour.Observing:
                stateTimer = observeTimer;
                observingAngle = Random.Range(0, 180);
                agent.isStopped = true;
                break;

            case ShepherdBehaviour.Running:
                agent.isStopped = false;
                if (deadSheepPositions.Count > 0)
                {
                    agent.SetDestination(deadSheepPositions[0]);
                }
                break;

            case ShepherdBehaviour.Chasing:
                agent.isStopped = false;
                chaseTimer = 0f;
                break;

            case ShepherdBehaviour.Shooting:
                stateTimer = 0.5f; // Aim time
                break;

            case ShepherdBehaviour.AggroScouting:
                agent.isStopped = false;
                break;

            case ShepherdBehaviour.Returning:
                agent.isStopped = false;
                break;
        }
    }

    #endregion

    #region Event Handlers

    private void OnSheepKilledHandler(Vector3 position, KilledBy killedBy)
    {
        if (aggroMeter >= 50)
        {
            // High aggro: Know Wolf location, chase directly
            aggroMeter += aggroIncreasePerBody;
            aggroMeter = Mathf.Min(100, aggroMeter);
            ChangeState(ShepherdBehaviour.Chasing);
        }
        else
        {
            // Low aggro: Add to investigation list
            deadSheepPositions.Add(position);

            // Transition to Running from low-priority states
            if (currentState == ShepherdBehaviour.Idle ||
                currentState == ShepherdBehaviour.Observing ||
                currentState == ShepherdBehaviour.Returning)
            {
                ChangeState(ShepherdBehaviour.Running);
            }
            // If already Running/Shooting/AggroScouting, just add to queue
        }
    }

    #endregion

    #region Helper Methods
    
    void GoToNextPoint() {
        if (patrolTargets.Length == 0) return;

        // Set the destination to the current waypoint
        agent.SetDestination(patrolTargets[currentPoint].position);

        // Cycle to the next point (loops back to 0 at the end)
        currentPoint = (currentPoint + 1) % patrolTargets.Length;
    }

    private IShootable ScanForTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, shootableLayer);
        List<IShootable> potentialTargets = new List<IShootable>();

        foreach (Collider hit in hits)
        {
            IShootable shootable = hit.GetComponent<IShootable>();

            if (shootable != null && shootable.CanBeTargeted)
            {
                // Check if not in bush
                if (!IsInBush(hit.transform.position))
                {
                    potentialTargets.Add(shootable);
                }
            }
        }

        if (potentialTargets.Count == 0) return null;

        // Weighted random selection
        return SelectTargetWeighted(potentialTargets);
    }

    private IShootable ScanForWolf()
    {
        if (player == null) return null;

        IShootable wolfShootable = player.GetComponent<IShootable>();

        if (wolfShootable != null && CanDetectWolf(wolfShootable))
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= detectionRadius)
            {
                return wolfShootable;
            }
        }

        return null;
    }

    private bool CanDetectWolf(IShootable target)
    {
        if (target.GetShootableType() == ShootableType.Wolf)
        {
            return aggroMeter >= 50 && !IsWolfInBush();
        }

        // Wolf crouching appears as Sheep if aggro < 50
        if (target.GetShootableType() == ShootableType.Sheep && aggroMeter < 50)
        {
            return false; // It's actually the Wolf but we can't tell
        }

        return target.CanBeTargeted;
    }

    private IShootable SelectTargetWeighted(List<IShootable> targets)
    {
        // First pass: Check for Wolf - instant shoot if detected
        foreach (IShootable target in targets)
        {
            if (target.GetShootableType() == ShootableType.Wolf)
            {
                return target; // Shoot Wolf immediately
            }
        }

        // Second pass: Calculate total weight for sheep
        int totalWeight = 0;
        foreach (IShootable target in targets)
        {
            ShootableType type = target.GetShootableType();
            if (type == ShootableType.BloodySheep)
            {
                totalWeight += 2; // Bloody sheep = 2 points
            }
            else if (type == ShootableType.Sheep)
            {
                totalWeight += 1; // Normal sheep = 1 point
            }
        }

        if (totalWeight == 0) return null;

        // Random selection based on weight
        int randomNum = Random.Range(1, totalWeight + 1);

        // Third pass: Select target based on weighted random
        foreach (IShootable target in targets)
        {
            ShootableType type = target.GetShootableType();
            int weight = 0;

            if (type == ShootableType.BloodySheep)
            {
                weight = 2;
            }
            else if (type == ShootableType.Sheep)
            {
                weight = 1;
            }

            randomNum -= weight;

            if (randomNum <= 0)
            {
                return target; // Shoot this target
            }
        }

        // Fallback: return first target
        return targets[0];
    }

    private void FireShotgun()
    {
        if (currentTarget == null) return;

        Vector3 direction = currentTarget.GetGameObj().transform.position - transform.position;

        if (Physics.Raycast(transform.position + Vector3.up, direction, out RaycastHit hit, shootingRadius))
        {
            IShootable hitTarget = hit.collider.GetComponent<IShootable>();
            if (hitTarget != null)
            {
                hitTarget.GotShot();
                Debug.Log($"Shot {hitTarget.GetShootableType()} at {currentTarget.GetShootableType()}");
            }
        }
    }

    private IEnumerator FrenzyMode()
    {
        isInFrenzy = true;
        float frenzyTimer = 0f;

        while (frenzyTimer < frenzyDuration)
        {
            // Shoot every targetable entity in sight
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, shootableLayer);

            foreach (Collider hit in hits)
            {
                IShootable shootable = hit.GetComponent<IShootable>();
                if (shootable != null && shootable.CanBeTargeted && !IsInBush(hit.transform.position))
                {
                    currentTarget = shootable;
                    FireShotgun();
                    yield return new WaitForSeconds(0.3f); // Fire rate
                }
            }

            frenzyTimer += Time.deltaTime;
            yield return null;
        }

        // Calm down
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
        // Check if position overlaps with any bush collider
        Collider[] bushes = Physics.OverlapSphere(position, 0.5f);
        foreach (Collider col in bushes)
        {
            if (col.GetComponent<Bush>() != null)
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 GetRandomPointInFarm()
    {
        // Get random point within farm zone bounds
        Bounds bounds = farmZone.bounds;

        Vector3 randomPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            transform.position.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );

        return randomPoint;
    }

    private bool CheckIfWolfIsInFarm()
    {
        if (player == null) return false;
        Bounds bounds = farmZone.bounds;
        return bounds.Contains(player.position);
    }

    private void UpdateSpeed()
    {
        // Linearly interpolate speed based on aggro
        float speedMultiplier = Mathf.Lerp(1f, 2f, aggroMeter / 100f);
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

enum ShepherdBehaviour
{
    Idle,
    Observing,
    Running,
    Chasing,
    Shooting,
    AggroScouting,
    Returning
}
