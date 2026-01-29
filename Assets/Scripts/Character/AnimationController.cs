using System;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private readonly int Idle = Animator.StringToHash("Idle");
    private readonly int Run = Animator.StringToHash("Run");
    private readonly int Jump = Animator.StringToHash("Jump");

    private const float CrossFadeTime = .1f;

    public void PlayIdleAnimation()
    {
        animator.CrossFadeInFixedTime(Idle, CrossFadeTime);
    }
    public void PlayRunAnimation()
    {
        animator.CrossFadeInFixedTime(Run, CrossFadeTime);
    }
    public void PlayJumpAnimation()
    {
        animator.CrossFadeInFixedTime(Jump, 0f);
    }
}
