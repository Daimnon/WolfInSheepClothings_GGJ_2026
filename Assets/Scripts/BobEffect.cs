using UnityEngine;

public class BobEffect3D : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float _bobHeight = 0.1f;
    [SerializeField, Range(0f, 30f)] private float _bobSpeed = 10f;

    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _visualTransform;
    [SerializeField] private ParticleSystem _particleSystem;

    private Vector3 _originalLocalPosition;

    private void Start()
    {
        _originalLocalPosition = _visualTransform.localPosition;
    }

    private void Update()
    {
        ApplyBob();
    }

    private void ApplyBob()
    {
        // Check real movement in 3D (XZ mostly, but full vector is fine)
        if (_rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            if (_particleSystem != null && !_particleSystem.isPlaying)
            {
                _particleSystem.Play();
            }

            float yOffset = Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;

            _visualTransform.localPosition = new Vector3(
                _originalLocalPosition.x,
                _originalLocalPosition.y + yOffset,
                _originalLocalPosition.z
            );
        }
        else
        {
            _visualTransform.localPosition = _originalLocalPosition;

            if (_particleSystem != null && _particleSystem.isPlaying)
            {
                _particleSystem.Stop();
            }
        }
    }
}