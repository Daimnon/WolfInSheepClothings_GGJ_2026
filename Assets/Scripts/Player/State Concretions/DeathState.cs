using Generics;

namespace Player.State_Concretions
{
    public class DeathState :BaseState
    {
        public DeathState(StateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
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