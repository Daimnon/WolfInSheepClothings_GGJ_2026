using Generics;

namespace Player.State_Concretions
{
    public class LocomotionState : BaseState
    {
        public LocomotionState(IStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Tick(float deltaTime)
        {
            throw new System.NotImplementedException();
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            throw new System.NotImplementedException();
        }
    }
}