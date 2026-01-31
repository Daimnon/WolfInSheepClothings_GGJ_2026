using Generics;
using Player.State_Concretions;
using UnityEngine;

namespace Player
{
    public class WolfStateMachine : StateMachine
    {
        public WolfStateMachine(IStateMachineController stateMachineController, PlayerControlsHandler playerControlsHandler, PlayerSO playerSO, Animator animator) : base(stateMachineController, playerControlsHandler, playerSO, animator)
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
            AddTransition(attackState, locomotionState, new FuncPredicate(playerControlsHandler.IsNoInputWithSprint));
            
            var hideState = new HideState(this, playerSO);
            AddState(hideState, hideState.GetType());
            AddAnyTransition(hideState.GetType(), new FuncPredicate(() => playerControlsHandler.IsInputTypeActive(InputType.Hide)));
            AddTransition(hideState, locomotionState, new FuncPredicate(playerControlsHandler.IsNoInputWithSprint));

            
            var stainState = new StainState(this, playerSO);
            AddState(stainState, stainState.GetType());
            AddAnyTransition(stainState.GetType(), new FuncPredicate(() => playerControlsHandler.IsInputTypeActive(InputType.Stain)));
            AddTransition(hideState, locomotionState, new FuncPredicate(playerControlsHandler.IsNoInputWithSprint));
            
            var deathState = new DeathState(this, playerSO);
            AddState(deathState, deathState.GetType());
            AddAnyTransition(deathState.GetType(), new FuncPredicate(() => !PlayerHandler.isAlive));

            
            //eating state left
            
            SetState(locomotionState);
        }
    }
}