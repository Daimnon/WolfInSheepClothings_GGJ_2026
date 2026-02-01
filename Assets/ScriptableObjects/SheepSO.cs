using UnityEngine;

[CreateAssetMenu(fileName = "new SheepSO", menuName = "Scriptable Objects/SheepSO", order = 1)]
public class SheepSO : ScriptableObject
{
    [SerializeField] private float _moveSpeed;
    public float MoveSpeed => _moveSpeed;
    [SerializeField] private float _minMoveDistance;
    public float MinMoveDistance => _minMoveDistance;
    [SerializeField] private float _maxMoveDistance;
    public float MaxMoveDistance => _maxMoveDistance;

    [SerializeField] private float _fearSpeed;
    public float FearSpeed => _fearSpeed;
    [SerializeField] private float _fearRadius;
    public float FearRadius => _fearRadius;
    [SerializeField] private float _chaosFactor;
    public float ChaosFactor => _chaosFactor;
    [SerializeField] private float stainDuration;
    public float StainDuration => stainDuration;
    [SerializeField] private float stainForce;
    public float StainForce => stainForce;

    [SerializeField] private float detectionRange;
    public float DetectionRange => detectionRange;

    [SerializeField] private float detectionRadius;
    public float DetectionRadius => detectionRadius;

    [SerializeField] private int maxStainCount = 2;
    public int MaxStainCount => maxStainCount;
}
