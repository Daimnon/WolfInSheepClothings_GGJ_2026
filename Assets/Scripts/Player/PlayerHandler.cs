using System;
using Generics;
using UnityEngine;

namespace Player
{
    public class PlayerHandler : MonoBehaviour, IStateMachineController, IShootable
    {
        [SerializeField] private PlayerControlsHandler playerControlsHandler;
        [SerializeField] private PlayerSO playerSO;
        private WolfStateMachine stateMachine;
        private ShootableType shootableType;
        private bool isHiddenInBush = false;

        public static event Action<Vector3> OnSheepKilled;
        
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

        public void NotifyChangeInShootableType(ShootableType newType)
        {
            shootableType = newType;
        }

        public void NotifyBushEnter()
        {
            isHiddenInBush = true;
        }
        
        public void NotifyBushExit()
        {
            isHiddenInBush = false;
        }
        
        public void NotifyPuddleEnter()
        {
            
        }

        public GameObject GetGameObj()
        {
            return gameObject;
        }

        public void GotShot()
        {
            // implement game over here
        }

        public ShootableType GetShootableType()
        {
            if (isHiddenInBush)
                return ShootableType.Hidden;
            return shootableType;
        }
    }
}