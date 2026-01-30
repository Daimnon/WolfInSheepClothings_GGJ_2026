using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveAgent : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private InputType myMovementInputType;

    [Header("Animation")] 
    [SerializeField] private AnimationController animationController;
    [SerializeField] private float jumpHeight = 2.0f;
    [SerializeField] private float jumpDuration = 1.0f;

    private bool isIdle = false;
    
    private const string FinishLineAreaName = "FinishLine";

    public static event Action<GameObject> PlayerTouchedFinishLine;

    #region MONO_BEHAVIOUR_METHODS

    private void Awake()
    {
        //PlayerControlsHandler.OnInputPerformed += SendInputToAgent;
        agent.autoTraverseOffMeshLink = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(FinishLineAreaName))
        {
            PlayerTouchedFinishLine?.Invoke(gameObject);
        }
    }

    private void Update()
    {
        if (agent.isOnOffMeshLink)
        {
            if (IsJumpLink(agent.currentOffMeshLinkData))
            {
                StartCoroutine(JumpRoutine());
            }
        }

        if (!agent.hasPath)
        {
            if (!isIdle)
            {
                animationController.PlayIdleAnimation();
                isIdle = true;
            }
        }
        else
        {
            if (isIdle)
            {
                animationController.PlayRunAnimation();
                isIdle = false;
            }
        }
    }

    private void OnValidate()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        
        if (animationController == null)
        {
            animationController = GetComponent<AnimationController>();
        }
    }

    #endregion

    #region INPUT_METHODS

    private void SendInputToAgent(InputType inputType)
    {
        if (myMovementInputType != inputType) return;
        Vector2 inputPosition = Mouse.current.position.ReadValue();

        Physics.Raycast(Camera.main.ScreenPointToRay(inputPosition), out var hit);
        if (hit.collider == null) return;
        Debug.Log(hit.collider.gameObject.name + hit.point);
        agent.SetDestination(hit.point);
    }

    #endregion

    #region JUMP_METHODS

    bool IsJumpLink(OffMeshLinkData linkData)
    {
        return linkData.linkType == OffMeshLinkType.LinkTypeJumpAcross;
    }
    
    IEnumerator JumpRoutine()
    {
        animationController.PlayJumpAnimation();

        OffMeshLinkData linkData = agent.currentOffMeshLinkData;
        Vector3 startPos = linkData.startPos;
        Vector3 endPos = linkData.endPos;

        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / jumpDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, normalizedTime);

            float parabola = 4 * jumpHeight * normalizedTime * (1 - normalizedTime);
            currentPos.y += parabola;

            transform.position = currentPos;

            yield return null;
        }

        transform.position = endPos;

        animationController.PlayIdleAnimation();
        isIdle = true;
        agent.CompleteOffMeshLink();
        agent.enabled = true;
    }

    #endregion
    
}