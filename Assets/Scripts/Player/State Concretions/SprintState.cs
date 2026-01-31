using Generics;
using UnityEngine;

namespace Player.State_Concretions
{
    public class SprintState: BaseState
    {
        public SprintState(StateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
        {
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            var playerMove = StateMachine.MoveInput;
            var playerMove3 = Quaternion.Euler(0f, 45f, 0f) * new Vector3(playerMove.x, 0f, playerMove.y).normalized;
            
            Animator.CrossFadeInFixedTime(RunHash, CrossFadeDuration);

            RigidbodyUtility.AddForce(playerMove3, playerSO.SprintForce);
            RigidbodyUtility.EnforceMaxVelocity(playerSO.SprintMaxVelocity);
        }
    }
}