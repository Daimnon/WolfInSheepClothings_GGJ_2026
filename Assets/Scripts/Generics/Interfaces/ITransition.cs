namespace NewMachine.Generics
{
    public interface ITransition
    {
        public IState TargetState { get; }
        public IPredicate Condition { get; }
    }
}