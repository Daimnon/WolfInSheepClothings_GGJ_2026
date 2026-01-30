using Generics;

namespace Player.State_Concretions
{
    public class EatState : BaseState
    {
        public EatState(IStateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
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