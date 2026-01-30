using UnityEngine;
using UnityEngine.AI;

public class ShepherdAI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform startPosition;

    private IShootable currentTarget;
    private ShepherdBehaviour behaviour;
    private float distanceToTarget;
    private float currentAggro;
    private float aggro;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

enum ShepherdBehaviour
{
    IdleState,
    ObservingState,
    RunningState,
    ChasingState,
    ShootingState,
    AggroScoutingState,
    ReturningState,
}
