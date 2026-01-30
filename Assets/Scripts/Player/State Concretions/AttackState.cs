using Generics;

namespace Player.State_Concretions
{
    public class AttackState : BaseState
    {
        public AttackState(IStateMachine stateMachine, PlayerSO p1) : base(stateMachine, p1)
        {
        }

        public override void Tick(float deltaTime)
        {
        }

        public override void FixedTick(float fixedDeltaTime)
        {
        }
        
        public override void NewInputUpdate(InputCommand inputCommand)
        {
            // dont implement anything here, we just wait until the attack animation is over
        }
    }
}