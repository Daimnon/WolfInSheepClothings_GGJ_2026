using Player;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    private const string _playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            other.transform.GetComponent<PlayerHandler>().NotifyPuddleEnter();
            Debug.Log("NotifyPaddleEnter");
        }
    }
}
