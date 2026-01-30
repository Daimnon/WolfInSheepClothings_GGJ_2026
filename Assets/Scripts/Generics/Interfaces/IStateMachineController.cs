namespace Generics
{
    public interface IStateMachineController
    {
        public void NotifyStateEnter(IState state);
        
        public void NotifyChangeInShootableType(ShootableType newType);

    }
}