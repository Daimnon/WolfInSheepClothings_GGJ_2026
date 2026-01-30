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
            
            if (attackDirection3 == Vector3.zero)
            {
                attackDirection3 = StateMachine.StateMachineController.GetTransform().forward;
            }
            
            var sheepInRange = StateMachine.StateMachineController.GetSheepInSphereCast();
            if (sheepInRange && sheepInRange.isAlive)
            {
                sheepInRange.SetStained();
            }

            RigidbodyUtility.AddImpulse(attackDirection3, playerSO.StainForce);
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