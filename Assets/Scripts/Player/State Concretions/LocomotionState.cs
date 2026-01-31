using Generics;
using UnityEngine;

namespace Player.State_Concretions
{
    public class LocomotionState : BaseState
    {
        private bool isIdle;
        public LocomotionState(StateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
        {
            
        }

        public override void Tick(float deltaTime)
        {
            Animator.CrossFadeInFixedTime(IdleHash, CrossFadeDuration);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            var playerMove = StateMachine.MoveInput;
            // rotating the input to match camera angle
            var playerMove3 = Quaternion.Euler(0f, 45f, 0f) * new Vector3(playerMove.x, 0f, playerMove.y).normalized;

            if (playerMove == Vector2.zero)
            {
                if (!isIdle)
                {
                    Animator.CrossFadeInFixedTime(IdleHash, CrossFadeDuration);
                    isIdle = true;
                }
            }
            else
            {
                if (isIdle)
                {
                    Animator.CrossFadeInFixedTime(WalkHash, CrossFadeDuration);
                    isIdle = false;
                }
            }
            
            RigidbodyUtility.AddForce(playerMove3, playerSO.NormalForce);
            RigidbodyUtility.EnforceMaxVelocity(playerSO.MaxNormalVelocity);
        }
    }
}