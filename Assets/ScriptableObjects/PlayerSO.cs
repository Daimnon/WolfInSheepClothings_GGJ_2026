using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Scriptable Objects/PlayerSO", order = 100)]
public class PlayerSO : ScriptableObject
{
    
    [SerializeField] private float normalForce;
    public float NormalForce => normalForce;
    [SerializeField] private float sprintForce;
    public float SprintForce => sprintForce;
    [SerializeField] private float attackDuration;
    public float AttackDuration => attackDuration;
    [SerializeField] private float stainDuration;
    public float StainDuration => stainDuration;
    [SerializeField] private float eatingDurationToConsume;
    public float EatingDurationToConsume => eatingDurationToConsume;
    
}
