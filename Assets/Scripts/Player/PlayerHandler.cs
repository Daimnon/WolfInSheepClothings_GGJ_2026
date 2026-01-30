using Generics;
using UnityEngine;

namespace Player
{
    public class PlayerHandler : MonoBehaviour, IStateMachineController
    {
        [SerializeField] private PlayerControlsHandler playerControlsHandler;
        [SerializeField] private PlayerSO playerSO;
        private WolfStateMachine stateMachine;
        
        
        private void Start()
        {
            stateMachine = new WolfStateMachine(this, playerControlsHandler,playerSO);
            stateMachine.Start();
            playerControlsHandler.OnInput += stateMachine.PassInput;
        }

        public void NotifyStateEnter(IState state)
        {
            //Debug.Log(state.GetType().Name + " entered.");
        }

        public void NotifyBushEnter()
        {
            
        }
        
        public void NotifyBushExit()
        {
            
        }
        
        public void NotifyPuddleEnter()
        {
            
        }
    }
}