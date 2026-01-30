using System;
using Generics;
using UnityEngine;

namespace Player
{
    public class PlayerHandler : MonoBehaviour, IStateMachineController, IShootable
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private PlayerControlsHandler playerControlsHandler;
        [SerializeField] private PlayerSO playerSO;
        private WolfStateMachine stateMachine;
        private ShootableType shootableType;
        private bool isHiddenInBush = false;
        private bool isBloody = false;

        public static event Action<Vector3> OnSheepKilled;

        public void Awake()
        {
            RigidbodyUtility.Initialize(rb);
        }

        private void Start()
        {
            stateMachine = new WolfStateMachine(this, playerControlsHandler, playerSO);
            stateMachine.Start();
            playerControlsHandler.OnInput += stateMachine.PassInput;
        }

        public void NotifyStateEnter(IState state)
        {
            //Debug.Log(state.GetType().Name + " entered.");
        }

        public void NotifyChangeInShootableType(ShootableType newType)
        {
            switch (newType)
            {
                case ShootableType.Wolf:
                    shootableType = newType;
                    break;
                case ShootableType.Sheep:
                    shootableType = isBloody ? ShootableType.BloodySheep : newType;
                    break;
                case ShootableType.BloodySheep:
                    shootableType = newType;
                    break;
            }
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
            isBloody = false;
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

        public void OnValidate()
        {
            rb = GetComponent<Rigidbody>();
        }
    }
}