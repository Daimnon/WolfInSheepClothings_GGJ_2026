namespace Generics
{
    public interface IStateMachineController
    {
        public void NotifyStateEnter(IState state);
    }
}