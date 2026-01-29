
using System;
using System.Collections.Generic;
using NewMachine.input_and_environment;
using NewMachine.input_and_environment.Interfaces;
using UnityEngine;

namespace NewMachine.Generics
{
    public abstract class StateMachine : IStateMachine
    {
        // a generic state machine that can handle any type of state that implements the IState interface
        protected IStateMachineController StateMachineController { get; private set; }
        public PlayerContext PlayerContext { get; private set; }
        public BehaviourSO BehaviourSo { get; private set; }
        
        public IState CurrentState => current.State;
        public IState PreviousState => previous.State;
        
        private StateNode current;
        private StateNode previous;
        
        private readonly Dictionary<Type, StateNode> states = new();
        private readonly List<ITransition> anyTransitions = new();
        
        public bool IsAlive { get; private set; }
        
        protected StateMachine(IStateMachineController stateMachineController, PlayerContext playerContext, BehaviourSO behaviourSO)
        {
            this.StateMachineController = stateMachineController;
            this.PlayerContext = playerContext;
            this.BehaviourSo = behaviourSO;
        }

        public void NotifyEndBehaviour()
        {
            CheckForNextState();
        }

        public virtual void Start() // called once at the beginning
        {
            IsAlive = true;
        }

        public virtual void PassInput(IInputCommand inputCommand)
        {
            current.State.NewInputUpdate(inputCommand);
        }

        private void CheckForNextState()
        {
            var transition = GetTransition();
            if (transition != null)
                ChangeState(transition.TargetState);
        }

        protected void AddState(IState state, Type type)
        {
            var node = new StateNode(state); 
            states.Add(type, node);
        }
        
        protected void SetState(IState state)
        {
            current = states[state.GetType()];
            current.State?.OnEnter();
        }
        
        public void ChangeState(IState nextState)
        {
            if (nextState.GetType() == current.State.GetType()) return;
            
            previous = current;
            current.State?.OnExit();
            current = states[nextState.GetType()];
            current.State?.OnEnter();
            StateMachineController.NotifyStateEnter(current.State);
#if UNITY_EDITOR
            Debug.Log($"{GetType()}: Entered {current.State.GetType().Name}");
#endif
        }
        
        protected void AddTransition(IState from, IState to, IPredicate condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }
        
        protected StateNode GetOrAddNode(IState state)
        {
            var node = states.GetValueOrDefault(state.GetType());
            if (node == null)
            {
                node = new StateNode(state);
                states.Add(state.GetType(), node);
            }
            return node;
        }
        
        protected void AddAnyTransition(Type to, IPredicate condition)
        {
            if (states.TryGetValue(to, out var toNode))
            {
                anyTransitions.Add(new Transition(toNode.State, condition));
            }
        }
        
        public ITransition GetTransition()
        {
            foreach (var transition in anyTransitions)
            {
                if (transition.TargetState.GetType() == current.State.GetType()) continue;
                if (transition.Condition.Evaluate())
                    return transition;
            }
            
            foreach (var transition in current.Transitions)
            {
                if (transition.TargetState.GetType() == current.State.GetType()) continue;
                if (transition.Condition.Evaluate())
                    return transition;
            }
            
            return null;
        }
        
        protected class StateNode
        {
            public IState State { get; }
            public List<ITransition> Transitions{ get; }
            
            public StateNode(IState state)
            {
                State = state;
                Transitions = new List<ITransition>();
            }
            
            public void AddTransition(IState targetState, IPredicate condition)
            {
                Transitions.Add(new Transition(targetState, condition));
            }
        }
    }
}