using Player;
using UnityEngine;

public class Bush : MonoBehaviour
{
    private const string _playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            other.transform.GetComponent<PlayerHandler>().NotifyBushEnter();
            Debug.Log("NotifyBushEnter");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_playerTag))
        {
            other.transform.GetComponent<PlayerHandler>().NotifyBushExit();
            Debug.Log("NotifyBushExit");
        }
    }
}
