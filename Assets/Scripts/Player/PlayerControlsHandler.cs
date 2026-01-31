using System;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlsHandler: MonoBehaviour
{
    public event Action<InputCommand> OnInput;
    public event Action<Vector2> MoveVectorEvent;
    public event Action<Vector2> LookVectorEvent;
    
    private readonly Dictionary<InputType,InputCommand> playerInputs = new Dictionary<InputType, InputCommand>();
    
    public Vector2 lookVector = Vector2.zero;
    public Vector2 moveVector = Vector2.zero;

    // private void Awake()
    // {
    //     OnInput += TesterMethod;
    //     MoveVectorEvent += TesterMethodMove;
    //     LookVectorEvent += TesterMethodLook;
    // }

    private void TesterMethod(InputCommand inputCommand)
    {
        Debug.Log(inputCommand.inputType + inputCommand.phase.ToString());
    }

    private void TesterMethodLook(Vector2 inputVector)
    {
        Debug.Log("Look" + inputVector);
    }

    private void TesterMethodMove(Vector2 inputVector)
    {
        Debug.Log("Move" + inputVector);

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveVector = context.phase == InputActionPhase.Performed ? context.ReadValue<Vector2>() : Vector2.zero;
        MoveVectorEvent?.Invoke(moveVector);
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        lookVector = context.phase == InputActionPhase.Performed ? context.ReadValue<Vector2>() : Vector2.zero;
        LookVectorEvent?.Invoke(lookVector);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        InputInvocation(InputType.Attack ,context.phase);

    }
    public void OnEat(InputAction.CallbackContext context)
    {
        InputInvocation(InputType.Eat ,context.phase);
    }

    public void OnStain(InputAction.CallbackContext context)
    {
        InputInvocation(InputType.Stain ,context.phase);
    }
    
    public void OnSprint(InputAction.CallbackContext context)
    {
        InputInvocation(InputType.Sprint ,context.phase);
    }
    
    public void OnHide(InputAction.CallbackContext context)
    {
        InputInvocation(InputType.Hide ,context.phase);
    }

    private void InputInvocation(InputType inputType, InputActionPhase phase)
    {
        switch (phase)
        {
            case InputActionPhase.Performed:
            {
                if (!playerInputs.ContainsKey(inputType))
                {
                    var playerInput = new InputCommand(inputType, phase);
                    playerInputs.Add(inputType, playerInput);
                    OnInput?.Invoke(playerInput);
                }
                break;
            }
            case InputActionPhase.Canceled:
                if (playerInputs.ContainsKey(inputType))
                {
                    var playerInput = new InputCommand(inputType, phase);
                    playerInputs.Remove(inputType);
                    OnInput?.Invoke(playerInput);
                }
                break;
        }
    }

    public InputActionPhase GetLatestPhaseOfInputType(InputType inputType)
    {
        return playerInputs.TryGetValue(inputType, out var input) ? input.phase : InputActionPhase.Canceled;
    }

    public bool IsInputTypeActive(InputType inputType)
    {
        return playerInputs.ContainsKey(inputType);
    }

    public bool IsNoInputActive()
    {
        return playerInputs.Count == 0 && PlayerHandler.isAlive;
    }

    public bool IsNoInputWithSprint()
    {
        if (playerInputs.Count == 1 && PlayerHandler.isAlive)
        {
            foreach (var input in playerInputs)
            {
                if (input.Value.inputType == InputType.Sprint)
                {
                    return true;
                }
            }
        }

        return playerInputs.Count == 0;
    }

}
public struct InputCommand
{
    public InputType inputType;
    public InputActionPhase phase;

    public InputCommand(InputType inputType, InputActionPhase phase)
    {
        this.inputType = inputType;
        this.phase = phase;
    }
}

public enum InputType
{
    Attack,
    Eat,
    Stain,
    Sprint,
    Hide,
    Death
}
