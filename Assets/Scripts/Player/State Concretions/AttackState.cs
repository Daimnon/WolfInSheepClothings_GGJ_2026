using Generics;

namespace Player.State_Concretions
{
    public class AttackState : BaseState
    {
        private float timer = 0;
        public AttackState(StateMachine stateMachine, PlayerSO playerSO) : base(stateMachine, playerSO)
        {
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
            timer = 0;
        }

        public override void Tick(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= playerSO.AttackDuration)
            {
                StateMachine.NotifyEndBehaviour();
            }
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