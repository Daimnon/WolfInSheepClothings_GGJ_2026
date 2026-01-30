using Player;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    private const string _playerTag = "Player";
    private const string _sheepTag = "Sheep";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag) && other.CompareTag(_sheepTag))
        {
            other.transform.GetComponent<IShootable>().NotifyPuddleEnter();
            Debug.Log("NotifyPaddleEnter");
        }
    }
}
