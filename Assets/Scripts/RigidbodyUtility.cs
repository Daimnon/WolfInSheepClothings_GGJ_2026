using UnityEngine;

public static class RigidbodyUtility
{
    private static Rigidbody _playerRb = null;
    public static Rigidbody PlayerRb { get => _playerRb; set => _playerRb = value; }

    public static void AddForce(Vector3 direction, float magnitude)
    {
        _playerRb.AddForce(direction.normalized * magnitude, ForceMode.Force);
    }
    public static void AddImpulse(Vector3 direction, float magnitude)
    {
        _playerRb.AddForce(direction.normalized * magnitude, ForceMode.Impulse);
    }
    
    public static void Initialize(Rigidbody playerRb)
    {
        _playerRb = playerRb;
    }
    
    public static void EnforceMaxVelocity(float maxVelocity)
    {
        if (_playerRb.linearVelocity.magnitude > maxVelocity)
        {
            _playerRb.linearVelocity = _playerRb.linearVelocity.normalized * maxVelocity;
        }
    }
}
