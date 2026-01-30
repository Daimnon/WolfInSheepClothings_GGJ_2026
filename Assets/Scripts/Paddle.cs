using Player;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    private const string _playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(_playerTag))
        {

        }
    }
}
