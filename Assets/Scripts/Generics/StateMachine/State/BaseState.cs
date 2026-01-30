using Singleton;
namespace Generics
{
    public abstract class BaseState : IState, IUpdateable
    {
        public IUpdatingParent UpdatingParent { get; set; }
        protected IStateMachine StateMachine { get; set; }

        protected PlayerSO playerSO;
        
        protected BaseState(IStateMachine stateMachine, PlayerSO playerSO)
        {
            StateMachine = stateMachine;
            UpdatingParent = UpdatingManager.Instance;
            this.playerSO = playerSO;
        }
        
        protected void ExitStateEarly()
        {
            var transitionState = StateMachine.GetTransition();
            if (transitionState != null)
            {
                StateMachine.ChangeState(transitionState.TargetState);
            }
        }

        public virtual void OnEnter()
        {
            var transitionState = StateMachine.GetTransition();
            if (transitionState == null)
            {
                RegisterToUpdatingParent();
            }
            else
            {
                StateMachine.ChangeState(transitionState.TargetState);
            }
        }

        public virtual void OnExit()
        {
            UnregisterFromUpdatingParent();
        }

        protected virtual void RegisterToUpdatingParent()
        {
            // note: this only registers for FixedUpdate. Override if you need Update.

            UpdatingParent.RegisterUpdateable(this, UpdateType.Both, UpdatePriority.Medium);
        }

        protected virtual void UnregisterFromUpdatingParent()
        {
            UpdatingParent.UnregisterUpdateable(this);
        }

        public virtual void NewInputUpdate(InputCommand inputCommand)
        {
            StateMachine.NotifyEndBehaviour();
        }

        public abstract void Tick(float deltaTime);
        public abstract void FixedTick(float fixedDeltaTime);
    }
}