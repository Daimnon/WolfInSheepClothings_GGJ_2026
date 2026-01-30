using Generics;
using Player.State_Concretions;

namespace Player
{
    public class WolfStateMachine : StateMachine
    {
        public WolfStateMachine(IStateMachineController stateMachineController, PlayerControlsHandler playerControlsHandler, PlayerSO playerSO) : base(stateMachineController, playerControlsHandler, playerSO)
        {
            var locomotionState = new LocomotionState(this, playerSO);
            AddState(locomotionState, locomotionState.GetType());
            AddAnyTransition(locomotionState.GetType(), new FuncPredicate(playerControlsHandler.IsNoInputActive));
            
            var sprintState = new SprintState(this, playerSO);
            AddState(sprintState, sprintState.GetType());
            AddTransition(locomotionState, sprintState, new FuncPredicate(() => playerControlsHandler.IsInputTypeActive(InputType.Sprint)));
            AddTransition(sprintState, locomotionState, new FuncPredicate(() => !playerControlsHandler.IsInputTypeActive(InputType.Sprint)));
            
            var attackState = new AttackState(this, playerSO);
            AddState(attackState, attackState.GetType());
            AddAnyTransition(attackState.GetType(), new FuncPredicate(() => playerControlsHandler.IsInputTypeActive(InputType.Attack)));
            
            var hideState = new HideState(this, playerSO);
            AddState(hideState, hideState.GetType());
            AddAnyTransition(hideState.GetType(), new FuncPredicate(() => playerControlsHandler.IsInputTypeActive(InputType.Hide)));
            
            var stainState = new StainState(this, playerSO);
            AddState(stainState, stainState.GetType());
            AddAnyTransition(stainState.GetType(), new FuncPredicate(() => playerControlsHandler.IsInputTypeActive(InputType.Stain)));
            
            SetState(locomotionState);
        }
    }
}