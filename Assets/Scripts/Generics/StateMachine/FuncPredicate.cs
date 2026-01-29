using System;

namespace NewMachine.Generics
{
    public class FuncPredicate : IPredicate
    {
        readonly Func<bool> predicate;


        public FuncPredicate(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public bool Evaluate()
        {
            return predicate.Invoke();
        }
    }
}