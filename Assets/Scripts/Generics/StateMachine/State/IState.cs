using NewMachine.input_and_environment.Interfaces;

namespace NewMachine.Generics
{
    public interface IState
    { 
        // Methods for state behavior
        protected bool IsInterruptible { get; }
        public void OnEnter();
        public void OnExit();
        public void NewInputUpdate(IInputCommand inputCommand);
    }
}