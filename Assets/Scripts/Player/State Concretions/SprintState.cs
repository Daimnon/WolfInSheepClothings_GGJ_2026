using Generics;

namespace Player.State_Concretions
{
    public class SprintState: BaseState
    {
        public SprintState(IStateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
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