using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour, IShootable
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float _moveSpeed = 8.0f;
    [SerializeField] private float _minMoveDist = 2.0f;
    [SerializeField] private float _maxMoveDist = 5.0f;
    private Vector2 _moveVector;

    public GameObject GetGameObj()
    {
        return gameObject;
    }
    public void GotShot()
    {
        Debug.Log("GotShot");
    }

    private void Start()
    {
        _agent.speed = _moveSpeed;
    }

    private Vector2 SetNewDirection()
    {
        float randX = Random.Range(-1.0f, 1.0f);
        float randY = Random.Range(-1.0f, 1.0f);
        return new Vector2(randX, randY).normalized;
    }

    [ContextMenu("SetNewMoveVector")]
    private Vector2 SetNewMoveVector()
    {
        float newMagnitude = Random.Range(_minMoveDist, _maxMoveDist);
        Vector2 newTargetPos = SetNewDirection() * newMagnitude;
        _moveVector = newTargetPos;
        return newTargetPos;
    }

    [ContextMenu("SetNewDestination")]
    private void SetNewDestination()
    {
        Vector2 newTargetPos = (Vector2)transform.position + SetNewMoveVector();
        _agent.SetDestination(newTargetPos);
    }

    /*private IEnumerator MoveToTarget()
    {
        Vector2 endPos = (Vector2)transform.position + _targetPos;

        while (true)
        {
            Vector2 currentPos = transform.position;
            Vector2 newPos = Vector2.MoveTowards(currentPos,endPos,_moveSpeed * Time.deltaTime);
            transform.position = newPos;

            if ((endPos - newPos).sqrMagnitude <= 0.0001f)
            {
                transform.position = endPos;
                yield break;
            }

            yield return null;
        }
    }*/
}
