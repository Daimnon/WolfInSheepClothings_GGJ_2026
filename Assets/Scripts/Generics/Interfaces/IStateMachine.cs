using NewMachine.input_and_environment;
using NewMachine.input_and_environment.Interfaces;

namespace NewMachine.Generics
{
    public interface IStateMachine
    {
        // Methods for state machine behavior
        IState CurrentState { get; }
        IState PreviousState { get; }
        public void NotifyEndBehaviour();
        public void ChangeState(IState newState);
        public ITransition GetTransition();
        public void Start();
        public void PassInput(IInputCommand inputCommand);
    }
}