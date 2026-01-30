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
}
