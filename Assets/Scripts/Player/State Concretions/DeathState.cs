using Generics;

namespace Player.State_Concretions
{
    public class DeathState :BaseState
    {
        bool flag = false;
        public DeathState(StateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
        {
        }

        public override void Tick(float deltaTime)
        {
            if(flag)
            {
                Animator.CrossFadeInFixedTime(DeathHash, 0.05f);
                flag = false;
            }
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            
        }
    }
}