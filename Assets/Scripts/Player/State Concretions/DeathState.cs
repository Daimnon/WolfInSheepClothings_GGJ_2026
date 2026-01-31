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
            Animator.CrossFadeInFixedTime(DeathHash, 0.05f);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            throw new System.NotImplementedException();
        }
    }
}