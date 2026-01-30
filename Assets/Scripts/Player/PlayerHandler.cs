using Generics;
using UnityEngine;

namespace Player
{
    public class PlayerHandler : MonoBehaviour, IStateMachineController
    {
        [SerializeField] private PlayerControlsHandler playerControlsHandler;
        private WolfStateMachine stateMachine;
        
        private void Start()
        {
            stateMachine = new WolfStateMachine(this, playerControlsHandler);
            stateMachine.Start();
            playerControlsHandler.OnInput += stateMachine.PassInput;
        }

        public void NotifyStateEnter(IState state)
        {
            //Debug.Log(state.GetType().Name + " entered.");
        }
    }
}