using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputMethods: MonoBehaviour
{
    public static event Action<InputType> OnInputPerformed;

    public void OnMove1(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        OnInputPerformed?.Invoke(InputType.LeftClick);
    }
    
    public void OnMove2(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        
        OnInputPerformed?.Invoke(InputType.RightClick);
    }
}

public enum InputType
{
    RightClick,
    LeftClick,
}
