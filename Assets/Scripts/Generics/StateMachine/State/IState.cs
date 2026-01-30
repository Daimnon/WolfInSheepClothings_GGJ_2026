
namespace Generics
{
    public interface IState
    { 
        // Methods for state behavior
        public void OnEnter();
        public void OnExit();
        public void NewInputUpdate(InputCommand inputCommand);
    }
}