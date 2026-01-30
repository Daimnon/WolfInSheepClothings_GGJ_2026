using Generics;
using Player.State_Concretions;

namespace Player
{
    public class WolfStateMachine : StateMachine
    {
        public WolfStateMachine(IStateMachineController stateMachineController, PlayerControlsHandler playerControlsHandler) : base(stateMachineController, playerControlsHandler)
        {
            var locomotionState = new LocomotionState(this);
            AddState(locomotionState, locomotionState.GetType());
            AddAnyTransition(locomotionState.GetType(), new FuncPredicate(playerControlsHandler.IsNoInputActive));
            
            var sprintState = new SprintState(this);
            AddState(sprintState, sprintState.GetType());
            AddTransition(locomotionState, sprintState, new FuncPredicate(() => playerControlsHandler.IsInputTypeActive(InputType.Sprint)));
            AddTransition(sprintState, locomotionState, new FuncPredicate(() => !playerControlsHandler.IsInputTypeActive(InputType.Sprint)));
            
            var attackState = new AttackState(this);
            AddState(attackState, attackState.GetType());
            AddAnyTransition(attackState.GetType(), new FuncPredicate(() => playerControlsHandler.IsInputTypeActive(InputType.Attack)));
            
            SetState(locomotionState);
            
        }
    }
}