using Generics;

namespace Player.State_Concretions
{
    public class SprintState: BaseState
    {
        public SprintState(IStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void FixedTick(float fixedDeltaTime)
        {
        }
    }
}