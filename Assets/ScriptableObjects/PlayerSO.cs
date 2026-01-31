using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Scriptable Objects/PlayerSO", order = 100)]
public class PlayerSO : ScriptableObject
{
    
    [SerializeField] private float normalForce;
    public float NormalForce => normalForce;
    [SerializeField] private float maxNormalVelocity;
    public float MaxNormalVelocity => maxNormalVelocity;
    [SerializeField] private float sprintForce;
    public float SprintForce => sprintForce;
    [SerializeField] private float sprintMaxVelocity;
    public float SprintMaxVelocity => sprintMaxVelocity;
    [SerializeField] private float attackDuration;
    public float AttackDuration => attackDuration;
    [SerializeField] private float attackForce;
    public float AttackForce => attackForce;
    [SerializeField] private float stainDuration;
    public float StainDuration => stainDuration;
    [SerializeField] private float stainForce;
    public float StainForce => stainForce;
    [SerializeField] private float eatingDurationToConsume;
    public float EatingDurationToConsume => eatingDurationToConsume;

    [SerializeField] private float detectionRange;
    public float DetectionRange => detectionRange;

    [SerializeField] private float detectionRadius;
    public float DetectionRadius => detectionRadius;
    
    [SerializeField] private int maxStainCount = 2;
    public int MaxStainCount => maxStainCount;
    


}
