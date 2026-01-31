using Generics;
using UnityEngine;

namespace Player.State_Concretions
{
    public class StainState: BaseState
    {
        private float timer = 0;
        public StainState(StateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
        {
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            timer = 0;
            var attackDirection = StateMachine.MoveInput;
            var attackDirection3 = new Vector3(attackDirection.x, 0, attackDirection.y);
            var rotatedDirection = (Quaternion.Euler(0f, 45f, 0f) * attackDirection3).normalized; ;
            
            if (rotatedDirection == Vector3.zero)
            {
                rotatedDirection = StateMachine.StateMachineController.GetTransform().forward;
            }
            
            StateMachine.StateMachineController.StartCheckForSheepCoroutine(InputType.Stain);

            RigidbodyUtility.AddImpulse(rotatedDirection, playerSO.StainForce);
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= playerSO.StainDuration)
            {
                StateMachine.NotifyEndBehaviour();
            }
        }

        public override void FixedTick(float fixedDeltaTime)
        {
        }
        
        public override void NewInputUpdate(InputCommand inputCommand)
        {
            // dont implement anything here, we just wait until the attack animation is over
        }
    }
}