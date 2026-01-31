using Singleton;
using UnityEngine;

namespace Generics
{
    public abstract class BaseState : IState, IUpdateable
    {
        public IUpdatingParent UpdatingParent { get; set; }
        protected StateMachine StateMachine { get; set; }

        protected PlayerSO playerSO;
        
        protected Animator Animator => StateMachine.Animator;
    
        protected static readonly int IdleHash = Animator.StringToHash("Idle");
        protected static readonly int WalkHash = Animator.StringToHash("Walking");
        protected static readonly int RunHash = Animator.StringToHash("Running");
        protected static readonly int AttackHash = Animator.StringToHash("SwipeAttack");
        protected static readonly int HideHash = Animator.StringToHash("Hide");
        protected static readonly int StainHash = Animator.StringToHash("Stain");
        protected static readonly int DeathHash = Animator.StringToHash("Death");
    
        protected const float CrossFadeDuration = 0.1f;
        
        protected BaseState(StateMachine stateMachine, PlayerSO playerSO)
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
        
        public virtual void ForceUnregisterFromUpdate()
        {
            UnregisterFromUpdatingParent();
        }

        public virtual void NewInputUpdate(InputCommand inputCommand)
        {
            StateMachine.NotifyEndBehaviour();
        }

        public abstract void Tick(float deltaTime);
        public abstract void FixedTick(float fixedDeltaTime);
    }
}