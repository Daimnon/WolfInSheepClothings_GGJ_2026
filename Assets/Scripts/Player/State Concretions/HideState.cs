using Generics;
using UnityEngine.InputSystem;

namespace Player.State_Concretions
{
    public class HideState : BaseState
    {
        public HideState(StateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
        {
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            // Here you can add logic to handle what happens when the player enters the hide state
            StateMachine.NotifyChangeInShootableType(ShootableType.Sheep);
        }

        public override void OnExit()
        {
            base.OnExit();
            // Here you can add logic to handle what happens when the player exits the hide state
            StateMachine.NotifyChangeInShootableType(ShootableType.Wolf);
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void FixedTick(float fixedDeltaTime)
        {
        }
        
        public override void NewInputUpdate(InputCommand inputCommand)
        {
            if (inputCommand.inputType == InputType.Hide && inputCommand.phase == InputActionPhase.Canceled)
            {
                StateMachine.NotifyEndBehaviour();
            }
        }
    }
}