using Generics;
using UnityEngine;

namespace Player.State_Concretions
{
    public class LocomotionState : BaseState
    {
        public LocomotionState(StateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
        {
            
        }

        public override void Tick(float deltaTime)
        {
            
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            var playerMove = StateMachine.MoveInput;
            RigidbodyUtility.AddForce(new Vector3(playerMove.x, 0, playerMove.y), playerSO.NormalForce);
            RigidbodyUtility.EnforceMaxVelocity(playerSO.MaxNormalVelocity);
        }
    }
}